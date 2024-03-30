using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace EasySql;

public class Theme
{
    private static Color backgroundColor = Color.Black;

    public static Attribute KeywordColor => new Attribute(Color.BrightMagenta, backgroundColor);

    public static Attribute CommentColor => new Attribute(Color.Green, backgroundColor);
    public static Attribute StringColor => new Attribute(Color.BrightCyan, backgroundColor);
    public static Attribute TextColor => new Attribute(Color.White, backgroundColor);
}