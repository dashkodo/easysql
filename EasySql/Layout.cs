using System.Data;
using EasySql;
using Microsoft.Data.SqlClient;
using Terminal.Gui;
using Testcontainers.MsSql;

public class Layout : Toplevel
{
    TableView tableView = new TableView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), };
    private readonly SqlEditor sqlEditor;

    private SqlExecutor sqlExecutor =
        new SqlExecutor(new SqlConnectionStringBuilder(PlaygroundSqlServer.ConnectionString)
            { TrustServerCertificate = true }.ToString());

    public Layout()
    {
        sqlEditor = new SqlEditor(new SqlAutocomplete(sqlExecutor))
        {
            CanFocus = true,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        

        sqlEditor.Init(ExecuteQuery);
        var statusBar = new StatusBar([
            new(Application.QuitKey, "~^Q~ Quit", () => RequestStop()),
            new(Key.CtrlMask | Key.f, "~^+F~ Format", () => sqlEditor.Format()),
            new(Key.CtrlMask | Key.e, "~^+E~ Execute", () => ExecuteQuery()),
        ])
        {
            Visible = true
        };
        Add(statusBar);

        // Create a left pane
        // var leftPane = new FrameView()
        // {
        //     Title = "Catalogs",
        //     X = 0,
        //     Y = 0,
        //     Width = Dim.Percent(0),
        //     Height = Dim.Fill(1),
        //     CanFocus = true,
        // };
        // leftPane.Border.LineStyle = LineStyle.Rounded;
        // Add(leftPane);

        // Create a right pane
        var topRight = new FrameView()
        {
            Title = "Query editor",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(50),
        };
        topRight.Border.BorderStyle = BorderStyle.Rounded;
        topRight.Add(sqlEditor);
        Add(topRight);

        var bottomRightPane = new FrameView()
        {
            Title = "Query results",
            X = 0,
            Y = Pos.Bottom(topRight),
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            CanFocus = false
        };
        bottomRightPane.Border.BorderStyle = BorderStyle.Rounded;

        tableView.Style.ShowHorizontalHeaderOverline = true;
        tableView.Style.ShowVerticalHeaderLines = true;
        tableView.Style.ShowHorizontalHeaderUnderline = true;
        tableView.Style.ShowVerticalCellLines = true;
        tableView.Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded
        };
        bottomRightPane.Add(tableView);

        Add(bottomRightPane);
    }

    private void ExecuteQuery()
    {
        SetData(sqlExecutor.ExecuteQuery(sqlEditor.Text.ToString()));
    }

    public void SetData(DataTable dataTable)
    {
        tableView.Table = dataTable;
    }
}