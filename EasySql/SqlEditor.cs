using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace EasySql;

class SqlEditor : TextView
{
    public event Action<DataTable> OnExecuteQueryComplete;
    private readonly SqlExecutor _sqlExecutor;

    private Attribute blue;
    private Attribute white;
    private Attribute magenta;


    public SqlEditor(SqlExecutor sqlExecutor, IAutocomplete autocomplete) : base()
    {
        _sqlExecutor = sqlExecutor;
        this.Autocomplete = autocomplete;
    }

    public void Init()
    {
        //ReplaceKeyBinding(Key.Enter, Key.Enter | Key.AltMask);
        AddCommand(Command.Refresh, () =>
        {
            ExecuteQuery();
            return true;
        });
        AddCommand(Command.Collapse, () =>
        {
            Format();
            return true;
        });


        magenta = Theme.KeywordColor;
        blue = Theme.KeywordColor;
        white = Theme.TextColor;
    }

    protected override void SetNormalColor()
    {
        Driver.SetAttribute(white);
    }

    // protected override void SetNormalColor(List<System.Text.Rune> line, int idx)
    // {
    //     if (IsInStringLiteral(line, idx))
    //     {
    //         Driver.SetAttribute(magenta);
    //     }
    //     else if (IsKeyword(line, idx))
    //     {
    //         Driver.SetAttribute(blue);
    //     }
    //     else
    //     {
    //         Driver.SetAttribute(white);
    //     }
    // }

    

    public void Format()
    {
        Text = new Formatter(Text.ToString()).Format();
    }


    public void ExecuteQuery()
    {
        var result = _sqlExecutor.ExecuteQuery(Text.ToString());
        OnExecuteQueryComplete(result);
    }
}