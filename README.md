# SportStats

<!-- TODO [BACK-07] Resumo da proposta de valor do sistema (2 a 3 frases). -->
Portal de estatísticas de esportes e e-sports, inspirado no SofaScore.

## Equipe
<!-- TODO [BACK-07] Cada integrante preenche a propria linha (nome completo e RA). -->
| Nome completo | RA |
|---|---|
| Ângelo Avezum Marques da Silva | 2840482511017 |
| Maxwell Carlos Silvério | 2840482511020 |

## Diagrama de classes
<!-- TODO [BACK-07] Inserir o diagrama UML (ou MER) das entidades principais. -->


## Como executar

1. Instale o .NET 10+ e o MySQL.
2. Crie `appsettings.Development.json` com sua connection string (arquivo ignorado pelo git).
3. Execute o script do banco de dados:

   **Windows:**
   ```cmd
   cmd /c "mysql -u root -p < Database\script.sql"
   ```

   **Linux:**
   ```bash
   mysql -u root -p < Database/script.sql
   ```

4. Rode o projeto:

   ```bash
   dotnet run
   ```
