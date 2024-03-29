using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace EasySql;

public class SqlEditor : TextView
{
    private readonly HashSet<string> _keywords = new(StringComparer.CurrentCultureIgnoreCase)
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
        "exists"
    };

    private readonly string _path = "RuneCells.rce";


    public SqlEditor()
    {
        ApplySyntaxHighlighting();
    }

    public void Format()
    {
        Text = new Formatter(Text).Format();
    }
    private void ApplySyntaxHighlighting()
    {
        ClearAllEvents();

        
        //var colours = (Dictionary<string, ColorScheme>)ThemeManager.Instance[ThemeManager.Instance.Theme]["ColorSchemes"].RetrieveValue();
        //var colorSchemeNormal = colours["TopLevel"].Normal;


        this.DesiredCursorVisibility = CursorVisibility.Box;
        ColorScheme = Theme.TextColor;
        Text = "/*Query to select:\nLots of data*/\nSELECT TOP 100 * \nfrom\n MyDb.dbo.Biochemistry where TestCode = 'blah';";

        Autocomplete.SuggestionGenerator = new SingleWordSuggestionGenerator
        {
            AllSuggestions = _keywords.ToList()
        };
        
        TextChanged += (s, e) => HighlightTextBasedOnKeywords();
        DrawContent += (s, e) => HighlightTextBasedOnKeywords();
        DrawContentComplete += (s, e) => HighlightTextBasedOnKeywords();
    }


    private void ClearAllEvents()
    {
        this.ClearEventHandlers("TextChanged");
        this.ClearEventHandlers("DrawContent");
        this.ClearEventHandlers("DrawContentComplete");
    }

    private bool ContainsPosition(Match m, int pos)
    {
        return pos >= m.Index && pos < m.Index + m.Length;
    }

    private void HighlightTextBasedOnKeywords()
    {
        // Comment blocks, quote blocks etc
        Dictionary<Rune, ColorScheme> blocks = new();

        var comments = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
        MatchCollection commentMatches = comments.Matches(Text);

        var singleQuote = new Regex(@"'.*?'", RegexOptions.Singleline);
        MatchCollection singleQuoteMatches = singleQuote.Matches(Text);

        // Find all keywords (ignoring for now if they are in comments, quotes etc)
        Regex[] keywordRegexes =
            _keywords.Select(k => new Regex($@"\b{k}\b", RegexOptions.IgnoreCase)).ToArray();
        Match[] keywordMatches = keywordRegexes.SelectMany(r => r.Matches(Text)).ToArray();

        var pos = 0;

        for (var y = 0; y < Lines; y++)
        {
            List<RuneCell> line = GetLine(y);

            for (var x = 0; x < line.Count; x++)
            {
                if (commentMatches.Any(m => ContainsPosition(m, pos)))
                {
                    line[x].ColorScheme = Theme.CommentColor;
                }
                else if (singleQuoteMatches.Any(m => ContainsPosition(m, pos)))
                {
                    line[x].ColorScheme = Theme.StringColor;
                }
                else if (keywordMatches.Any(m => ContainsPosition(m, pos)))
                {
                    line[x].ColorScheme = Theme.KeywordColor;
                }
                else
                {
                    line[x].ColorScheme = Theme.TextColor;
                }

                pos++;
            }

            // for the \n or \r\n that exists in Text but not the returned lines
            pos += Environment.NewLine.Length;
        }
    }

    private string IdxToWord(List<Rune> line, int idx)
    {
        string[] words = Regex.Split(
            new string(line.Select(r => (char)r.Value).ToArray()),
            "\\b"
        );

        var count = 0;
        string current = null;

        foreach (string word in words)
        {
            current = word;
            count += word.Length;

            if (count > idx)
            {
                break;
            }
        }

        return current?.Trim();
    }

    private bool IsKeyword(List<Rune> line, int idx)
    {
        string word = IdxToWord(line, idx);

        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        return _keywords.Contains(word, StringComparer.CurrentCultureIgnoreCase);
    }
}