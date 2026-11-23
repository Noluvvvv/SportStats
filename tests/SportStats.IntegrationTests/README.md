# Testes de integração do BACK-09

Execute a partir da raiz do repositório, com o SDK .NET 10 e uma distribuição binária extraída do MySQL 8. A suíte foi verificada com o MySQL Community Server 8.4.13 para Windows. A variável `SPORTSTATS_TEST_MYSQLD` deve apontar para o executável em `bin/` dessa distribuição.

```powershell
$env:SPORTSTATS_TEST_MYSQLD = 'C:\ferramentas\mysql-8.4.13-winx64\bin\mysqld.exe'
dotnet build SportStats.csproj
dotnet test tests/SportStats.IntegrationTests/SportStats.IntegrationTests.csproj
```

Sem a variável ou com um caminho inválido, a suíte falha na inicialização. Ela cria uma instância em diretório temporário, com porta local efêmera e senha exclusiva da execução. A fixture aplica somente as definições de tabela e índice necessárias de `Database/script.sql`. Os testes criam seus próprios usuários e usam os IDs gerados pelo MySQL. Ao finalizar, a fixture encerra o processo que iniciou e remove o diretório temporário. A configuração do aplicativo e as bases existentes ficam fora desse fluxo.

A suíte cobre favoritos, histórico, notificações e a exclusão em cascata. Os testes chamam os repositórios reais, com Dapper e MySqlConnector, e não precisam iniciar o aplicativo web.
