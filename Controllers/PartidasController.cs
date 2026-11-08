// TODO [BACK-02] CRUD de Partida, placar e detalhes  (manual: R1, R3, R4)
//
// 1. Criar PartidasController injetando IPartidaRepository e IRepository<Time>.
// 2. Criar partida: formulario com Tipo (Futebol, Basquete, Volei, Esport).
//    Montar a subclasse certa (PartidaFutebol, PartidaBasquete, PartidaVolei, PartidaEsport).
//    Esport exige MelhorDe = 1, 3 ou 5.
// 3. RegistrarPlacar(id, casa, fora) e Encerrar(id): usar os metodos de dominio
//    Partida.RegistrarPlacar() e Partida.Encerrar(); depois IPartidaRepository.Atualizar().
// 4. Capturar PlacarInvalidoException, PartidaEncerradaException e EscalacaoInvalidaException
//    (todas herdam de DominioException) e exibir alerta amigavel na view (R4).
// 5. Action Detalhes(id): pagina da partida. Depois trocar href="#" em
//    Views/Shared/_PartidaCard.cshtml pelo link de Detalhes.
// 6. Views em Views/Partidas/ (Index, Criar, Detalhes).
// 7. Na Detalhes, mostrar retrospecto entre os dois times (consulta nova em
//    IPartidaConsultaRepository, com SQL parametrizado).