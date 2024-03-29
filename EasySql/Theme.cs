using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace EasySql;

public class Theme
{
    private static Color backgroundColor = Color.Black;

    public static ColorScheme KeywordColor => new ColorScheme(new Attribute(Color.BrightMagenta, backgroundColor));

    public static ColorScheme CommentColor => new ColorScheme(new Attribute(Color.Green, backgroundColor));
    public static ColorScheme StringColor => new ColorScheme(new Attribute(Color.BrightCyan, backgroundColor));
    public static ColorScheme TextColor => new ColorScheme(new Attribute(Color.White, backgroundColor));
}