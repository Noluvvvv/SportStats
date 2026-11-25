(() => {
    const raiz = document.querySelector("[data-ao-vivo-url]");
    if (!raiz) return;

    const url = raiz.dataset.aoVivoUrl;
    const grupo = raiz.dataset.grupo;
    const modalidade = raiz.dataset.modalidade;

    const INTERVALO_MS = 30000;

    function atualizarElemento(chave, placarCasa, placarFora) {
        document.querySelectorAll("[data-placar-casa]").forEach(elemento => {
            if (elemento.dataset.placarCasa === chave) {
                elemento.textContent = placarCasa;
            }
        });

        document.querySelectorAll("[data-placar-fora]").forEach(elemento => {
            if (elemento.dataset.placarFora === chave) {
                elemento.textContent = placarFora;
            }
        });
    }

    function aplicarPlacar(partida) {
        if (!partida) return;

        const chave = partida.chave ?? partida.Chave;
        const casa = partida.placarCasa ?? partida.PlacarCasa;
        const fora = partida.placarFora ?? partida.PlacarFora;

        if (!chave) return;

        atualizarElemento(chave, casa, fora);
    }

    async function atualizarPlacares() {
        try {
            const resposta = await fetch(url, {
                headers: { Accept: "application/json" },
                cache: "no-store"
            });

            if (!resposta.ok) return;

            const partidas = await resposta.json();

            for (const partida of partidas) {
                aplicarPlacar(partida);
            }
        } catch {
            // Se a API estiver indisponivel, tenta novamente no proximo ciclo.
        }
    }

    // Polling funciona como alternativa caso SignalR ou a conexao WebSocket falhe.
    atualizarPlacares();
    setInterval(atualizarPlacares, INTERVALO_MS);

    if (!window.signalR) {
        console.warn("SignalR nao carregado. Atualizacao por polling ativada.");
        return;
    }

    const conexao = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/placar")
        .withAutomaticReconnect()
        .build();

    conexao.on("placarAtualizado", partidas => {
        if (!Array.isArray(partidas)) return;

        partidas.forEach(aplicarPlacar);
    });

    async function conectar() {
        try {
            await conexao.start();

            if (grupo) {
                await conexao.invoke("EntrarNoGrupo", grupo);
            }

            if (modalidade) {
                await conexao.invoke("EntrarNoGrupo", modalidade);
            }

            console.info("SignalR conectado ao placar do SportStats.");
        } catch (erro) {
            console.warn("Falha ao conectar SignalR:", erro);
        }
    }

    conexao.onreconnected(async () => {
        if (grupo) await conexao.invoke("EntrarNoGrupo", grupo);
        if (modalidade) await conexao.invoke("EntrarNoGrupo", modalidade);

        atualizarPlacares();
    });

    conectar();
})();
