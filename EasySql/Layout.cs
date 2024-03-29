using System.Data;
using EasySql;
using Terminal.Gui;

public class Layout : Toplevel
{
    TableView tableView = new TableView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

    public Layout()
    {
        var statusBar = new StatusBar([new(Application.QuitKey, "~^Q~ Quit", () => RequestStop())])
        {
            Visible = true
        };
        Add(statusBar);

        // Create a left pane
        var leftPane = new FrameView()
        {
            Title = "Catalogs",
            X = 0,
            Y = 0,
            Width = Dim.Percent(0),
            Height = Dim.Fill(1),
            CanFocus = true,
        };
        leftPane.Border.LineStyle = LineStyle.Rounded;
        Add(leftPane);

        // Create a right pane
        var topRight = new FrameView()
        {
            Title = "Query editor",
            X = Pos.Right(leftPane),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(50),
        };
        topRight.Border.LineStyle = LineStyle.Rounded;
        var textView = new SqlEditor()
        {
            Text = "select * from tests",
            CanFocus = true,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        topRight.Add(textView);
        Add(topRight);

        var bottomRightPane = new FrameView()
        {
            Title = "Query results",
            X = Pos.Right(leftPane),
            Y = Pos.Bottom(topRight),
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            CanFocus = true
        };
        bottomRightPane.Border.LineStyle = LineStyle.Rounded;
        bottomRightPane.Add(tableView);

        Add(bottomRightPane);
    }

    public void SetData(DataTable dataTable)
    {
        tableView.Table = new DataTableSource(dataTable);
    }
}