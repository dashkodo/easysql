using System.Diagnostics;
using Testcontainers.MsSql;
using Tomlyn;
using Tomlyn.Model;

namespace EasySql;

public class PlaygroundSqlServer
{
    private static string localConnectionString;
    private static MsSqlContainer container;

    public static async Task Init()
    {
        var configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sqlconfig");

        if (!File.Exists(configFilePath))
            File.WriteAllBytes(configFilePath, []);
        
        var model = Toml.ToModel(File.ReadAllText(configFilePath));
        var s = ((TomlTableArray)model["Databases"])[0]["ConnectionString"] as string;
        localConnectionString = s; // TODO: Load from config
        if(localConnectionString == null)
            container = await CreateSqlServerContainer();
    }

    public static string ConnectionString =>
        localConnectionString ?? container?.GetConnectionString() ?? throw new InvalidOperationException("Run Init method first");

    private static async Task<MsSqlContainer> CreateSqlServerContainer()
    {
        var sqlServerContainerBuilder = new MsSqlBuilder().WithImage("dangl/mssql-tmpfs:0.1.12");

        const int SqlServerMemoryLimitInMb = 512;

        sqlServerContainerBuilder = sqlServerContainerBuilder
            .WithEnvironment("MSSQL_MEMORY_LIMIT_MB", SqlServerMemoryLimitInMb.ToString());

        if (Debugger.IsAttached)
        {
            const int SqlServerStandardPort = 1433;
            sqlServerContainerBuilder =
                sqlServerContainerBuilder.WithPortBinding(10_000 + SqlServerStandardPort, SqlServerStandardPort);
        }

        var testContainer = sqlServerContainerBuilder.Build();

        await testContainer.StartAsync(CancellationToken.None).ConfigureAwait(false);
        return testContainer;
    }
}