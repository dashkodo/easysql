using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace EasySql;

class SqlEditor : TextView
{
 
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

    public SqlEditor(IAutocomplete autocomplete) : base()
    {
        this.Autocomplete = autocomplete;
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
        AddKeyBinding(Key.Enter | Key.CtrlMask, Command.Refresh);


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

        return SqlAutocomplete.keywords.Contains(word, StringComparer.CurrentCultureIgnoreCase);
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