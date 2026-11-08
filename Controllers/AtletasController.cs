// TODO [BACK-03] Atleta e Transferencia  (manual: R1, R2, R4)
//
// 1. Models/Atleta.cs: Nome, Nickname (opcional), Nacionalidade, ModalidadeId.
//    Propriedades privadas, construtor validando, metodo de dominio para transferir.
// 2. Models/Transferencia.cs: AtletaId, TimeId, DataInicio, DataFim (nula = time atual).
// 3. Composicao (R2): Atleta guarda List<Transferencia> e controla o ciclo de vida.
// 4. Regra: um atleta so pode ter um time ativo (DataFim nula).
//    Violacao lanca AtletaEmDoisTimesException (criar em Exceptions/, herdando de DominioException).
// 5. Repositories/Dapper/AtletaRepository.cs : IRepository<Atleta> e
//    TransferenciaRepository (SQL parametrizado, sem SQL no controller).
// 6. AtletasController: Index, Criar, Editar, Excluir e Transferir (GET/POST).
// 7. Views em Views/Atletas/. Mostrar o historico de times do atleta.
// 8. Registrar os repositorios no Program.cs (AddScoped).