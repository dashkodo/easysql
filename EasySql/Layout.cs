using System.Data;
using EasySql;
using Terminal.Gui;

public class Layout : Toplevel
{
    TableView tableView = new TableView { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

    public Layout()
    {
        var textView = new SqlEditor()
        {
            CanFocus = true,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        
        var statusBar = new StatusBar([
            new(Application.QuitKey, "~^Q~ Quit", () => RequestStop()),
            new(Key.F4, "~F4~ Format", () => textView.Format()),
            
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
        topRight.Border.LineStyle = LineStyle.Rounded;
       
        topRight.Add(textView);
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
        bottomRightPane.Border.LineStyle = LineStyle.Rounded;
        bottomRightPane.Add(tableView);

        Add(bottomRightPane);
    }

    public void SetData(DataTable dataTable)
    {
        tableView.Table = new DataTableSource(dataTable);
    }
}