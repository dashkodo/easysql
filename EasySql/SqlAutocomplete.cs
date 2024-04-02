using System.Data;
using System.Text;
using Terminal.Gui;
using Rune = System.Rune;

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
                
                AllSuggestions.AddRange(
                    columns.Select(arg => "." + arg.ColumnName).Distinct()
                        .Concat(columns.Select(arg => arg.ColumnName).Distinct())
                        .Concat(columns.Select(arg => arg.TableName).Distinct())
                        .Concat(columns.Select(arg => arg.SchemaName).Distinct())
                        .Concat(columns.Select(arg => arg.CatalogName).Distinct())
                        .Cast<string>());
            }).Start();

        this.MaxWidth = 50;
    }

    protected override string IdxToWord(List<Rune> line, int idx, int columnOffset = 0)
    {
        StringBuilder sb = new StringBuilder();
        var endIdx = idx;

        // get the ending word index
        while (endIdx < line.Count)
        {
            if (IsWordChar(line[endIdx]))
            {
                endIdx++;
            }
            else
            {
                break;
            }
        }

        // It isn't a word char then there is no way to autocomplete that word
        if (endIdx == idx && columnOffset != 0)
        {
            return null;
        }

        // we are at the end of a word.  Work out what has been typed so far

        var prevIsDot = false;
        while (endIdx-- > 0)
        {
            if (IsWordChar(line[endIdx]))
            {
                if (prevIsDot)
                    break;

                sb.Insert(0, (char)line[endIdx]);
            }
            else
            {
                if (line[endIdx] == '.')
                {
                    prevIsDot = true;
                    sb.Insert(0, (char)line[endIdx]);
                }
                else
                    break;
            }
        }

        return sb.ToString();
    }
}