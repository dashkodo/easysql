using EasySql;
using Terminal.Gui;

Console.WriteLine("initializing sql");
await EasySql.ConfigManager.Init();
Console.WriteLine("initialized sql");

Application.Init();
Application.QuitKey = Key.Q | Key.CtrlMask;
//ThemeManager.Instance.Theme = "Dark";

Application.Run<Layout>();

Application.Shutdown ();