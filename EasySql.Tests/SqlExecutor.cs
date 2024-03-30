using System.Diagnostics;
using FluentAssertions;
using Testcontainers.MsSql;

namespace EasySql.Tests;

public class SqlExecutorTests
{
    private MsSqlContainer testContainer;

    [OneTimeSetUp]
    public async Task Setup()
    {
        await CreateSqlServerContainer();
    }

    [Test]
    public void ShouldReturnDataTable()
    {
        var dataTable = new SqlExecutor(testContainer.GetConnectionString()).ExecuteQuery("select name, object_id, * from sys.tables");
        dataTable.Columns.Should().BeEmpty();
        dataTable.Rows.Should().BeEmpty();

    }
}