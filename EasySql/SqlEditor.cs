using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace EasySql;

class SqlEditor : TextView
{
    private HashSet<string> keywords = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
    private Attribute blue;
    private Attribute white;
    private Attribute magenta;


    public override bool ProcessKey(KeyEvent kb)
    {
        return base.ProcessKey(kb);
    }

    public override bool OnEnter(View view)
    {
        return base.OnEnter(view);
    }

    public SqlEditor(): base()
    {

    }

    public void Init(Action executeCommand)
    {
        //ReplaceKeyBinding(Key.Enter, Key.Enter | Key.AltMask);
        AddCommand(Command.Refresh, () =>
        {
            executeCommand();
            return true;
        });
        ClearKeybinding(Key.e | Key.CtrlMask);
        ClearKeybinding(Key.f | Key.CtrlMask);
        AddKeyBinding(Key.e | Key.CtrlMask, Command.Refresh);
        AddKeyBinding(Key.f | Key.CtrlMask, Command.Refresh);
        keywords.Add("select");
        keywords.Add("distinct");
        keywords.Add("top");
        keywords.Add("from");
        keywords.Add("create");
        keywords.Add("CIPHER");
        keywords.Add("CLASS_ORIGIN");
        keywords.Add("CLIENT");
        keywords.Add("CLOSE");
        keywords.Add("COALESCE");
        keywords.Add("CODE");
        keywords.Add("COLUMNS");
        keywords.Add("COLUMN_FORMAT");
        keywords.Add("COLUMN_NAME");
        keywords.Add("COMMENT");
        keywords.Add("COMMIT");
        keywords.Add("COMPACT");
        keywords.Add("COMPLETION");
        keywords.Add("COMPRESSED");
        keywords.Add("COMPRESSION");
        keywords.Add("CONCURRENT");
        keywords.Add("CONNECT");
        keywords.Add("CONNECTION");
        keywords.Add("CONSISTENT");
        keywords.Add("CONSTRAINT_CATALOG");
        keywords.Add("CONSTRAINT_SCHEMA");
        keywords.Add("CONSTRAINT_NAME");
        keywords.Add("CONTAINS");
        keywords.Add("CONTEXT");
        keywords.Add("CONTRIBUTORS");
        keywords.Add("COPY");
        keywords.Add("CPU");
        keywords.Add("CURSOR_NAME");
        keywords.Add("primary");
        keywords.Add("key");
        keywords.Add("insert");
        keywords.Add("alter");
        keywords.Add("add");
        keywords.Add("update");
        keywords.Add("set");
        keywords.Add("delete");
        keywords.Add("truncate");
        keywords.Add("as");
        keywords.Add("order");
        keywords.Add("by");
        keywords.Add("asc");
        keywords.Add("desc");
        keywords.Add("between");
        keywords.Add("where");
        keywords.Add("and");
        keywords.Add("or");
        keywords.Add("not");
        keywords.Add("limit");
        keywords.Add("null");
        keywords.Add("is");
        keywords.Add("drop");
        keywords.Add("database");
        keywords.Add("table");
        keywords.Add("having");
        keywords.Add("in");
        keywords.Add("join");
        keywords.Add("on");
        keywords.Add("union");
        keywords.Add("exists");

        Autocomplete.AllSuggestions = keywords.ToList();

        magenta = Driver.MakeAttribute(Color.Magenta, Color.Black);
        blue = Driver.MakeAttribute(Color.Cyan, Color.Black);
        white = Driver.MakeAttribute(Color.White, Color.Black);
    }

    protected override void SetNormalColor()
    {
        Driver.SetAttribute(white);
    }

    protected override void SetNormalColor(List<System.Rune> line, int idx)
    {
        if (IsInStringLiteral(line, idx))
        {
            Driver.SetAttribute(magenta);
        }
        else if (IsKeyword(line, idx))
        {
            Driver.SetAttribute(blue);
        }
        else
        {
            Driver.SetAttribute(white);
        }
    }

    private bool IsInStringLiteral(List<System.Rune> line, int idx)
    {
        string strLine = new string(line.Select(r => (char)r).ToArray());

        foreach (Match m in Regex.Matches(strLine, "'[^']*'"))
        {
            if (idx >= m.Index && idx < m.Index + m.Length)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsKeyword(List<System.Rune> line, int idx)
    {
        var word = IdxToWord(line, idx);

        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        return keywords.Contains(word, StringComparer.CurrentCultureIgnoreCase);
    }

    private string IdxToWord(List<System.Rune> line, int idx)
    {
        var words = Regex.Split(
            new string(line.Select(r => (char)r).ToArray()),
            "\\b");


        int count = 0;
        string current = null;

        foreach (var word in words)
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

    public void Format()
    {
        Text = new Formatter(Text.ToString()).Format();
    }
}