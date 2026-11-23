using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SportStats.Repositories.Dapper;

namespace SportStats.IntegrationTests;

[CollectionDefinition(nameof(MySqlFixture))]
public sealed class MySqlCollection : ICollectionFixture<MySqlFixture> { }

public sealed class MySqlFixture : IAsyncLifetime
{
    private const string Database = "sportstats_tests";
    private const string ExecutavelVariavel = "SPORTSTATS_TEST_MYSQLD";
    private DirectoryInfo? _temporario;
    private Process? _servidor;
    private string? _conexaoServidor;
    private string? _conexaoBanco;

    public IDbConnectionFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var executavel = Environment.GetEnvironmentVariable(ExecutavelVariavel);
        if (string.IsNullOrWhiteSpace(executavel) || !Path.IsPathFullyQualified(executavel) || !File.Exists(executavel))
            throw new InvalidOperationException($"Defina {ExecutavelVariavel} com o caminho absoluto do mysqld de teste.");

        var baseDir = Directory.GetParent(Path.GetDirectoryName(executavel)!)!.FullName;
        _temporario = Directory.CreateTempSubdirectory("sportstats-mysql-");
        var pasta = _temporario.FullName;
        var dados = Path.Combine(pasta, "data");
        var log = Path.Combine(pasta, "server.log");
        var inicializacao = Path.Combine(pasta, "init.sql");
        var senha = Guid.NewGuid().ToString("N");

        try
        {
            await InicializarBanco(executavel, baseDir, dados);
            await File.WriteAllTextAsync(inicializacao, $"ALTER USER 'root'@'localhost' IDENTIFIED BY '{senha}';\n");
            using var portaLivre = new TcpListener(IPAddress.Loopback, 0);
            portaLivre.Start();
            var porta = ((IPEndPoint)portaLivre.LocalEndpoint).Port;
            portaLivre.Stop();

            var config = new MySqlConnectionStringBuilder
            {
                Server = IPAddress.Loopback.ToString(),
                Port = (uint)porta,
                UserID = "root",
                Password = senha,
                ConnectionTimeout = 2,
                Pooling = false
            };

            _servidor = Iniciar(executavel, false, $"--basedir={baseDir}", $"--datadir={dados}",
                "--bind-address=127.0.0.1", $"--port={porta}", "--mysqlx=OFF",
                $"--socket={Path.Combine(pasta, "mysql.sock")}", $"--pid-file={Path.Combine(pasta, "mysql.pid")}",
                $"--init-file={inicializacao}", $"--log-error={log}");
            await using var conexao = await AguardarConexao(config.ConnectionString, _servidor);
            var pastaServidor = await conexao.ExecuteScalarAsync<string>("SELECT @@datadir");
            if (!Path.TrimEndingDirectorySeparator(Path.GetFullPath(pastaServidor!))
                    .Equals(dados, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw new InvalidOperationException("O MySQL conectado não pertence à instância de teste.");

            _conexaoServidor = config.ConnectionString;
            var versao = await conexao.ExecuteScalarAsync<string>("SELECT VERSION()");
            if (versao is null || !versao.StartsWith("8.", StringComparison.Ordinal))
                throw new InvalidOperationException("A suíte requer MySQL 8.");

            File.Delete(inicializacao);
            await conexao.ExecuteAsync("CREATE DATABASE sportstats_tests CHARACTER SET utf8mb4");
            conexao.ChangeDatabase(Database);
            await AplicarSchema(conexao);
            config.Database = Database;
            Factory = new MySqlConnectionFactory(new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:Padrao"] = config.ConnectionString }).Build());
            _conexaoBanco = config.ConnectionString;
        }
        catch (Exception erro)
        {
            var detalhe = File.Exists(log) ? (await File.ReadAllTextAsync(log)).Replace(senha, "[omitida]") : "";
            throw new InvalidOperationException($"Falha ao preparar o MySQL descartável. {detalhe}", erro);
        }
        finally
        {
            if (_conexaoBanco is null)
                await DisposeAsync();
        }
    }

    public MySqlConnection CriarConexao() => new(_conexaoBanco);

    public async Task<int> CriarUsuario()
    {
        await using var conexao = CriarConexao();
        return await conexao.ExecuteScalarAsync<int>(
            """
            INSERT INTO Usuario (Nome, Email, SenhaHash) VALUES ('Fixture de persistência', @email, 'fixture');
            SELECT LAST_INSERT_ID();
            """,
            new { email = $"{Guid.NewGuid():N}@example.invalid" });
    }

    public async Task ExcluirUsuario(int usuarioId)
    {
        await using var conexao = CriarConexao();
        await conexao.ExecuteAsync("DELETE FROM Usuario WHERE Id = @usuarioId", new { usuarioId });
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_servidor is { HasExited: false })
            {
                if (_conexaoServidor is not null)
                {
                    try
                    {
                        await using var conexao = new MySqlConnection(_conexaoServidor);
                        await conexao.OpenAsync();
                        await conexao.ExecuteAsync("SHUTDOWN", commandTimeout: 5);
                    }
                    catch (MySqlException)
                    {
                        // O servidor pode fechar a conexão durante o encerramento.
                    }

                    using var prazo = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    try
                    {
                        await _servidor.WaitForExitAsync(prazo.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // O processo ainda ativo será encerrado abaixo.
                    }
                }

                if (!_servidor.HasExited)
                {
                    _servidor.Kill(entireProcessTree: true);
                    await _servidor.WaitForExitAsync();
                }
            }
        }
        finally
        {
            _servidor?.Dispose();
            _servidor = null;
            if (_temporario is not null)
            {
                await RemoverTemporario(_temporario);
                _temporario = null;
            }
        }
    }

    private static async Task RemoverTemporario(DirectoryInfo diretorio)
    {
        var raiz = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()));
        var caminho = diretorio.FullName;
        var comparacao = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!string.Equals(Path.GetDirectoryName(caminho), raiz, comparacao) ||
            !diretorio.Name.StartsWith("sportstats-mysql-", StringComparison.Ordinal))
            throw new InvalidOperationException("O diretório a remover não pertence à fixture MySQL.");

        // No Windows, arquivos podem continuar bloqueados por um breve intervalo após a saída.
        for (var tentativa = 0; Directory.Exists(caminho); tentativa++)
        {
            try
            {
                Directory.Delete(caminho, recursive: true);
                break;
            }
            catch (IOException) when (tentativa < 20)
            {
                await Task.Delay(250);
            }
        }
    }

    private static async Task InicializarBanco(string executavel, string baseDir, string dados)
    {
        var argumentos = new List<string> { "--initialize-insecure", $"--basedir={baseDir}", $"--datadir={dados}" };
        if (OperatingSystem.IsWindows())
            argumentos.Add("--console");
        using var processo = Iniciar(executavel, true, argumentos.ToArray());
        var stderr = processo.StandardError.ReadToEndAsync();
        using var prazo = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        try
        {
            await processo.WaitForExitAsync(prazo.Token);
        }
        catch (OperationCanceledException)
        {
            if (!processo.HasExited)
                processo.Kill(entireProcessTree: true);
            await processo.WaitForExitAsync();
            throw;
        }

        if (processo.ExitCode != 0)
            throw new InvalidOperationException($"Falha ao inicializar o diretório de teste. {await stderr}");
    }

    private static Process Iniciar(string executavel, bool capturarErros, params string[] argumentos)
    {
        var configuracao = new ProcessStartInfo(executavel)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = capturarErros
        };
        configuracao.ArgumentList.Add("--no-defaults");
        foreach (var argumento in argumentos)
            configuracao.ArgumentList.Add(argumento);

        return Process.Start(configuracao) ?? throw new InvalidOperationException("Falha ao iniciar o MySQL.");
    }

    private static async Task<MySqlConnection> AguardarConexao(string configuracao, Process servidor)
    {
        using var prazo = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (!servidor.HasExited)
        {
            var conexao = new MySqlConnection(configuracao);
            try
            {
                await conexao.OpenAsync(prazo.Token);
                return conexao;
            }
            catch (MySqlException)
            {
                await conexao.DisposeAsync();
                await Task.Delay(200, prazo.Token);
            }
            catch
            {
                await conexao.DisposeAsync();
                throw;
            }
        }

        throw new InvalidOperationException($"O processo do MySQL terminou com código {servidor.ExitCode}.");
    }

    private static async Task AplicarSchema(MySqlConnection conexao)
    {
        var schema = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "schema.sql"));
        // Carrega somente as declarações necessárias, sem o preâmbulo e sem os dados de exemplo.
        foreach (var tabela in new[] { "Usuario", "PreferenciaUsuario", "Favorito", "HistoricoVisualizacao", "Notificacao" })
            await ExecutarDdl($@"(?ms)^CREATE TABLE {tabela} \(.*?^\);", tabela);

        foreach (var indice in new[] { "idx_hist_usuario", "idx_notif_usuario" })
            await ExecutarDdl($@"(?m)^CREATE INDEX {indice} [^;]+;", indice);

        async Task ExecutarDdl(string pattern, string nome)
        {
            var trecho = Regex.Match(schema, pattern);
            if (!trecho.Success)
                throw new InvalidOperationException($"DDL de {nome} não encontrado no schema.");
            await conexao.ExecuteAsync(trecho.Value);
        }
    }
}
