﻿using System.ComponentModel;
using EasySql;
using Microsoft.Data.SqlClient;
using Terminal.Gui;

public class Layout : Toplevel
{
    private string[] SpinnerSequence => new[] { "⢹", "⢺", "⢼", "⣸", "⣇", "⡧", "⡗", "⡏" };
    private int SpinnerFrame = 0;

    public Layout()
    {
        var statusBar = new StatusBar()
        {
            Visible = true
        };
        Add(statusBar);

        var tabView = new TabView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
        };
        tabView.Style.ShowTopLine = false;
        tabView.Style.ShowBorder = false;

        var statusItems = new List<StatusItem>()
        {
            new StatusItem(Application.QuitKey, "~^Q~ Quit", () => RequestStop()),
            new StatusItem(Key.F.WithCtrl, "~^+F~ Format",
                () => (tabView.SelectedTab.View.MostFocused as SqlEditor).Format()),
            new StatusItem(Key.E.WithCtrl, "~^+E~ Execute",
                () => ExecuteQuery(tabView.SelectedTab)),
        };

        ConfigManager.Connections
            .Select((arg, idx) =>
            {
                var setFocusAction = CreateTab(tabView, arg, idx == 0);
                var tabHotKey = new Key(KeyCode.F1 + (uint)idx);
                statusItems.Add(new StatusItem(tabHotKey, $"~{tabHotKey}~ {arg.Name}",
                    () =>
                    {
                        tabView.SelectedTab = tabView.Tabs.ElementAt(idx);
                    }));
                return arg;
            })
            .ToArray();

        

        statusBar.Items = statusItems.ToArray();

        Add(tabView);
    }

    private Action CreateTab(TabView tabView, Connection connection, bool setActive)
    {
        var sqlExecutor = new SqlExecutor(
            new SqlConnectionStringBuilder(connection.ConnectionString)
                { TrustServerCertificate = true }.ToString());
        var sqlEditor = new SqlEditor(sqlExecutor, new SqlAutocomplete(sqlExecutor))
        {
            CanFocus = true,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };


        var queryEditorPanel = new FrameView()
        {
            Title = $"Query editor [{connection.Name}]",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(50),
        };

        var resultsPanel = new FrameView()
        {
            Title = "Query results",
            X = 0,
            Y = Pos.Bottom(queryEditorPanel),
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            CanFocus = false
        };
        queryEditorPanel.Add(sqlEditor);
        sqlEditor.Init();

        resultsPanel.Border.BorderStyle = LineStyle.Rounded;
        var resultsTable = new TableView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), };
        sqlEditor.OnExecuteQueryComplete += (result) => { resultsTable.Table = new DataTableSource(result); };
        resultsPanel.Add(resultsTable);

        var container = new View { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), };
        container.Add(queryEditorPanel, resultsPanel);

        var tab = new Tab() { Title = connection.Name, View = container };
        tabView.AddTab(tab, setActive);
        tab.VisibleChanged += (arg, evt) =>
        {
            var sqlEditor = (arg as Tab).View.Subviews.First().Subviews.First();
            sqlEditor.SetFocus();
        };
        return () => sqlEditor.SetFocus();
    }

    private void ExecuteQuery(Tab selectedTab)
    {
        var resultsPanel = selectedTab.View.Subviews.OfType<FrameView>().Last();
        var sqlEditor = selectedTab.View.MostFocused as SqlEditor;
        var title = resultsPanel.Title;
        var worker = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = true };

        worker.DoWork += (s, e) =>
        {
            var task = Task.Run(() => sqlEditor.ExecuteQuery());
            while (!task.IsCompleted)
            {
                (s as BackgroundWorker).ReportProgress(0);
                Thread.Sleep(100);
            }
        };
        worker.RunWorkerCompleted += (sender, args) => resultsPanel.Title = title;

        worker.ProgressChanged += (_, _) =>
        {
            resultsPanel.Title = title + SpinnerSequence[SpinnerFrame];
            SpinnerFrame = ++SpinnerFrame % SpinnerSequence.Length;
        };

        worker.RunWorkerAsync();
    }
}