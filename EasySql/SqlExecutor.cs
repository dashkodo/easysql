using System.Data;
using Microsoft.Data.SqlClient;

namespace EasySql;

public class SqlExecutor(string connectionString)
{
    public DataTable GetSchema()
    {
        return ExecuteQuery("""
             select TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME
             from INFORMATION_SCHEMA.COLUMNS
             """);
    }

    public DataTable ExecuteQuery(string commandText)
    {
        var dataTable = new DataTable();
        using var sqlConnection = new SqlConnection(connectionString);        
        using var cmd = new SqlCommand(commandText.LimitNumberOfRows(), sqlConnection);
        sqlConnection.Open();

        // create data adapter
        using var sqlDataAdapter = new SqlDataAdapter(cmd);
        // this will query your database and return the result to your datatable
        try
        {
            sqlDataAdapter.Fill(dataTable);
        }
        catch (Exception e)
        {
            dataTable.Columns.Add("Err");
            dataTable.Rows.Add(e.Message);
        }
        return dataTable;
    }
}