using Terminal.Gui;
Application.Init();
Application.QuitKey = Key.Q.WithCtrl;
//ThemeManager.Instance.Theme = "Dark";
ConfigurationManager.Apply();

Application.Run<Layout>().Dispose();

Application.Shutdown ();