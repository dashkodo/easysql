using System.Data;
using System.Text;
using Terminal.Gui;
using Rune = System.Text.Rune;

namespace EasySql;

public class SqlAutocomplete : TextViewAutocomplete
{
    public static HashSet<string> keywords = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase)
    {
        "select",
        "distinct",
        "top",
        "from",
        "create",
        "CIPHER",
        "CLASS_ORIGIN",
        "CLIENT",
        "CLOSE",
        "COALESCE",
        "CODE",
        "COLUMNS",
        "COLUMN_FORMAT",
        "COLUMN_NAME",
        "COMMENT",
        "COMMIT",
        "COMPACT",
        "COMPLETION",
        "COMPRESSED",
        "COMPRESSION",
        "CONCURRENT",
        "CONNECT",
        "CONNECTION",
        "CONSISTENT",
        "CONSTRAINT_CATALOG",
        "CONSTRAINT_SCHEMA",
        "CONSTRAINT_NAME",
        "CONTAINS",
        "CONTEXT",
        "CONTRIBUTORS",
        "COPY",
        "CPU",
        "CURSOR_NAME",
        "primary",
        "key",
        "insert",
        "alter",
        "add",
        "update",
        "set",
        "delete",
        "truncate",
        "as",
        "order",
        "by",
        "asc",
        "desc",
        "between",
        "where",
        "and",
        "or",
        "not",
        "limit",
        "null",
        "is",
        "drop",
        "database",
        "table",
        "having",
        "in",
        "join",
        "on",
        "union",
        "exists",
    };

    private IEnumerable<string> AllSuggestions = [];

    public SqlAutocomplete(SqlExecutor sqlExecutor)
    {
        new Thread(() =>
        {
            var schema = sqlExecutor.GetSchema();
            if(!schema.Columns.Contains("TABLE_CATALOG"))
                return;
            var columns = schema.Rows.Cast<DataRow>()
                    .Select(arg => new
                    {
                        CatalogName = arg["TABLE_CATALOG"],
                        SchemaName = arg["TABLE_SCHEMA"],
                        TableName = arg["TABLE_NAME"],
                        ColumnName = arg["COLUMN_NAME"],
                    });
                
            AllSuggestions  = new List<string>(
                    columns.Select(arg => "." + arg.ColumnName).Distinct()
                        .Concat(columns.Select(arg => arg.ColumnName).Distinct())
                        .Concat(columns.Select(arg => arg.TableName).Distinct())
                        .Concat(columns.Select(arg => arg.SchemaName).Distinct())
                        .Concat(columns.Select(arg => arg.CatalogName).Distinct())
                        .Cast<string>());
            }).Start();

        this.MaxWidth = 50;
    }
}