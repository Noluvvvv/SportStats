(() => {
    const raiz = document.querySelector('[data-ao-vivo-url]');
    if (!raiz) return;

    const INTERVALO_MS = 30000;

    async function atualizarPlacares() {
        try {
            const resposta = await fetch(raiz.dataset.aoVivoUrl, { headers: { Accept: 'application/json' } });
            if (!resposta.ok) return;

            for (const p of await resposta.json()) {
                const casa = document.querySelector(`[data-placar-casa="${p.id}"]`);
                const fora = document.querySelector(`[data-placar-fora="${p.id}"]`);
                if (casa) casa.textContent = p.placarCasa;
                if (fora) fora.textContent = p.placarFora;
            }
        } catch {
            // sem conexao: tenta de novo no proximo ciclo
        }
    }

    // BACKEND: trocar este polling por SignalR (WebSocket). (BACK-05)
    // Exemplo: new signalR.HubConnectionBuilder().withUrl('/hubs/placar').build()
    // e, em connection.on('placarAtualizado', p => ...), atualizar os mesmos elementos
    // [data-placar-casa="id"] e [data-placar-fora="id"]. Tambem mover partidas entre
    // "Proximas", "Ao vivo" e "Encerradas" quando o status mudar.
    setInterval(atualizarPlacares, INTERVALO_MS);
})();