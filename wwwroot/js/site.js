(() => {
    const raiz = document.querySelector("[data-ao-vivo-url]");
    if (!raiz) return;

    const url = raiz.dataset.aoVivoUrl;
    const grupo = raiz.dataset.grupo;
    const modalidade = (raiz.dataset.modalidade || "").toLowerCase();

    const INTERVALO_MS = 30000;
    const INTERVALO_RECONEXAO_MS = 5000;

    let atualizando = false;
    let recarregando = false;

    function obterChave(partida) {
        return partida?.chave ?? partida?.Chave ?? null;
    }

    function obterModalidade(partida) {
        return (
            partida?.modalidadeSlug ??
            partida?.ModalidadeSlug ??
            ""
        ).toLowerCase();
    }

    function filtrarPartidas(partidas) {
        if (!Array.isArray(partidas)) return [];

        if (!modalidade) return partidas;

        return partidas.filter(
            partida => obterModalidade(partida) === modalidade
        );
    }

    function atualizarElemento(seletor, atributo, chave, valor) {
        if (valor === null || valor === undefined) return;

        document.querySelectorAll(seletor).forEach(elemento => {
            if (elemento.dataset[atributo] === chave) {
                elemento.textContent = String(valor);
            }
        });
    }

    function aplicarPlacar(partida) {
        if (!partida) return;

        const chave = obterChave(partida);
        if (!chave) return;

        const casa = partida.placarCasa ?? partida.PlacarCasa;
        const fora = partida.placarFora ?? partida.PlacarFora;

        atualizarElemento(
            "[data-placar-casa]",
            "placarCasa",
            chave,
            casa
        );

        atualizarElemento(
            "[data-placar-fora]",
            "placarFora",
            chave,
            fora
        );
    }

    function reconciliarPartidas(partidas) {
        const bloco = document.querySelector(
            '[data-ao-vivo-bloco="true"]'
        );

        if (!bloco || recarregando) return;

        const chavesApi = new Set(
            partidas.map(obterChave).filter(Boolean)
        );

        const elementos = bloco.querySelectorAll("[data-placar-casa]");

        const chavesTela = new Set(
            Array.from(elementos)
                .map(elemento => elemento.dataset.placarCasa)
                .filter(Boolean)
        );

        const mudou =
            chavesApi.size !== chavesTela.size ||
            [...chavesApi].some(chave => !chavesTela.has(chave));

        if (mudou) {
            recarregando = true;
            window.location.reload();
        }
    }

    async function atualizarPlacares() {
        if (atualizando || recarregando) return;

        atualizando = true;

        try {
            const resposta = await fetch(url, {
                headers: { Accept: "application/json" },
                cache: "no-store"
            });

            if (!resposta.ok) return;

            const dados = await resposta.json();

            if (!Array.isArray(dados)) return;

            const partidas = filtrarPartidas(dados);

            // O endpoint local consulta o cache.
            // Uma resposta HTTP 200 com lista vazia representa
            // o snapshot atual do cache, não uma chamada direta ao provedor.
            reconciliarPartidas(partidas);

            if (recarregando) return;

            partidas.forEach(aplicarPlacar);
        } catch (erro) {
            console.warn("Falha ao atualizar partidas:", erro);
        } finally {
            atualizando = false;
        }
    }

    atualizarPlacares();
    setInterval(atualizarPlacares, INTERVALO_MS);

    if (!window.signalR) {
        console.warn("SignalR indisponível. Polling continua ativo.");
        return;
    }

    const conexao = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/placar")
        .withAutomaticReconnect()
        .build();

    conexao.on("placarAtualizado", dados => {
        const partidas = Array.isArray(dados) ? dados : [dados];

        partidas.forEach(aplicarPlacar);
    });

    async function entrarNosGrupos() {
        if (grupo) {
            await conexao.invoke("EntrarNoGrupo", grupo);
        }

        if (modalidade) {
            await conexao.invoke("EntrarNoGrupo", modalidade);
        }
    }

    async function conectar() {
        while (conexao.state === signalR.HubConnectionState.Disconnected) {
            try {
                await conexao.start();
                await entrarNosGrupos();

                console.info("SignalR conectado ao SportStats.");

                await atualizarPlacares();
                return;
            } catch (erro) {
                console.warn(
                    "Falha na conexão SignalR. Nova tentativa em 5 segundos.",
                    erro
                );

                await new Promise(resolve =>
                    setTimeout(resolve, INTERVALO_RECONEXAO_MS)
                );
            }
        }
    }

    conexao.onreconnected(async () => {
        try {
            await entrarNosGrupos();
            await atualizarPlacares();
        } catch (erro) {
            console.warn("Falha ao reinscrever grupos:", erro);
        }
    });

    conectar();
})();
