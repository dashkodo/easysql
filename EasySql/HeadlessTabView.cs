using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Terminal.Gui;

namespace EasySql;

public class HeadlessTabView : View
{
    private TabView.Tab selectedTab;

    /// <summary>
    /// The default <see cref="MaxTabTextWidth"/> to set on new <see cref="TabView"/> controls
    /// </summary>
    public const uint DefaultMaxTabTextWidth = 30;

    private class TabContentView : View
    {
    }

    /// <summary>
    /// This sub view is the main client area of the current tab.  It hosts the <see cref="View"/> 
    /// of the tab, the <see cref="SelectedTab"/>
    /// </summary>
    TabContentView contentView;

    private List<TabView.Tab> tabs = new List<TabView.Tab>();

    /// <summary>
    /// All tabs currently hosted by the control
    /// </summary>
    /// <value></value>
    public IReadOnlyCollection<TabView.Tab> Tabs
    {
        get => tabs.AsReadOnly();
    }

    /// <summary>
    /// When there are too many tabs to render, this indicates the first
    /// tab to render on the screen.
    /// </summary>
    /// <value></value>
    public int TabScrollOffset { get; set; }

    /// <summary>
    /// The maximum number of characters to render in a Tab header.  This prevents one long tab 
    /// from pushing out all the others.
    /// </summary>
    public uint MaxTabTextWidth { get; set; } = DefaultMaxTabTextWidth;

    /// <summary>
    /// Event for when <see cref="SelectedTab"/> changes
    /// </summary>
    public event EventHandler<TabView.TabChangedEventArgs> SelectedTabChanged;


    /// <summary>
    /// Event fired when a <see cref="TabView.Tab"/> is clicked.  Can be used to cancel navigation,
    /// show context menu (e.g. on right click) etc.
    /// </summary>
    public event EventHandler<TabMouseEventArgs> TabClicked;


    /// <summary>
    /// The currently selected member of <see cref="Tabs"/> chosen by the user
    /// </summary>
    /// <value></value>
    public TabView.Tab SelectedTab
    {
        get => selectedTab;
        set
        {
            var old = selectedTab;

            if (selectedTab != null)
            {
                if (selectedTab.View != null)
                {
                    // remove old content
                    if (selectedTab.View.Subviews.Count == 0)
                    {
                        contentView.Remove(selectedTab.View);
                    }
                    else
                    {
                        foreach (var view in selectedTab.View.Subviews)
                        {
                            contentView.Remove(view);
                        }
                    }
                }
            }

            selectedTab = value;

            if (value != null)
            {
                // add new content
                if (selectedTab.View != null)
                {
                    if (selectedTab.View.Subviews.Count == 0)
                    {
                        contentView.Add(selectedTab.View);
                    }
                    else
                    {
                        foreach (var view in selectedTab.View.Subviews)
                        {
                            contentView.Add(view);
                        }
                    }
                }
            }

            EnsureSelectedTabIsVisible();

            if (old != value)
            {
                OnSelectedTabChanged(old, value);
            }
        }
    }

    /// <summary>
    /// Initializes a <see cref="TabView"/> class using <see cref="LayoutStyle.Computed"/> layout.
    /// </summary>
    public HeadlessTabView() : base()
    {
        CanFocus = true;
        contentView = new TabContentView();
        ApplyStyleChanges();

        base.Add(contentView);

        // Things this view knows how to do
        AddCommand(Command.Left, () =>
        {
            SwitchTabBy(-1);
            return true;
        });
        AddCommand(Command.Right, () =>
        {
            SwitchTabBy(1);
            return true;
        });
        AddCommand(Command.LeftHome, () =>
        {
            SelectedTab = Tabs.FirstOrDefault();
            return true;
        });
        AddCommand(Command.RightEnd, () =>
        {
            SelectedTab = Tabs.LastOrDefault();
            return true;
        });


        // Default keybindings for this view
        AddKeyBinding(Key.CursorLeft, Command.Left);
        AddKeyBinding(Key.CursorRight, Command.Right);
        AddKeyBinding(Key.Home, Command.LeftHome);
        AddKeyBinding(Key.End, Command.RightEnd);
    }

    /// <summary>
    /// Updates the control to use the latest state settings in <see cref="Style"/>.
    /// This can change the size of the client area of the tab (for rendering the 
    /// selected tab's content).  This method includes a call 
    /// to <see cref="View.SetNeedsDisplay()"/>
    /// </summary>
    public void ApplyStyleChanges()
    {
        contentView.X = 0;
        contentView.Width = Dim.Fill(0);

        // Tabs are along the top

        var tabHeight = 0;

        //move content down to make space for tabs
        contentView.Y = tabHeight;

        // Fill client area leaving space at bottom for border
        contentView.Height = Dim.Fill(0);

        // The top tab should be 2 or 3 rows high and on the top


        SetNeedsDisplay();
    }


    ///<inheritdoc/>
    public override void Redraw(Rect bounds)
    {
        Move(0, 0);
        Driver.SetAttribute(GetNormalColor());

        if (false)
        {
            // How much space do we need to leave at the bottom to show the tabs
            int spaceAtBottom = Math.Max(0, 0 - 1);
            int startAtY = Math.Max(0, 0 - 1);

            DrawFrame(new Rect(0, startAtY, bounds.Width,
                Math.Max(bounds.Height - spaceAtBottom - startAtY, 0)), 0, true);
        }

        if (Tabs.Any())
        {
            contentView.SetNeedsDisplay();
            var savedClip = contentView.ClipToBounds();
            contentView.Redraw(contentView.Bounds);
            Driver.Clip = savedClip;
        }
    }

    /// <summary>
    /// Disposes the control and all <see cref="Tabs"/>
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // Manually dispose all tabs
        foreach (var tab in Tabs)
        {
            tab.View?.Dispose();
        }
    }

    /// <summary>
    /// Raises the <see cref="SelectedTabChanged"/> event
    /// </summary>
    protected virtual void OnSelectedTabChanged(TabView.Tab oldTab, TabView.Tab newTab)
    {
        SelectedTabChanged?.Invoke(this, new TabView.TabChangedEventArgs(oldTab, newTab));
    }

    /// <inheritdoc/>
    public override bool ProcessKey(KeyEvent keyEvent)
    {
        if (HasFocus && CanFocus)
        {
            var result = InvokeKeybindings(keyEvent);
            if (result != null)
                return (bool)result;
        }

        return base.ProcessKey(keyEvent);
    }


    /// <summary>
    /// Changes the <see cref="SelectedTab"/> by the given <paramref name="amount"/>.  
    /// Positive for right, negative for left.  If no tab is currently selected then
    /// the first tab will become selected
    /// </summary>
    /// <param name="amount"></param>
    public void SwitchTabBy(int amount)
    {
        if (Tabs.Count == 0)
        {
            return;
        }

        // if there is only one tab anyway or nothing is selected
        if (Tabs.Count == 1 || SelectedTab == null)
        {
            SelectedTab = Tabs.ElementAt(0);
            SetNeedsDisplay();
            return;
        }

        var currentIdx = tabs.IndexOf(SelectedTab);

        // Currently selected tab has vanished!
        if (currentIdx == -1)
        {
            SelectedTab = Tabs.ElementAt(0);
            SetNeedsDisplay();
            return;
        }

        var newIdx = Math.Max(0, Math.Min(currentIdx + amount, Tabs.Count - 1));

        SelectedTab = tabs[newIdx];
        SetNeedsDisplay();

        EnsureSelectedTabIsVisible();
    }


    /// <summary>
    /// Updates <see cref="TabScrollOffset"/> to be a valid index of <see cref="Tabs"/>
    /// </summary>
    /// <remarks>Changes will not be immediately visible in the display until you call <see cref="View.SetNeedsDisplay()"/></remarks>
    public void EnsureValidScrollOffsets()
    {
        TabScrollOffset = Math.Max(Math.Min(TabScrollOffset, Tabs.Count - 1), 0);
    }

    /// <summary>
    /// Updates <see cref="TabScrollOffset"/> to ensure that <see cref="SelectedTab"/> is visible
    /// </summary>
    public void EnsureSelectedTabIsVisible()
    {
        if (SelectedTab == null)
        {
            return;
        }

        // if current viewport does not include the selected tab
        if (!CalculateViewport(Bounds).Any(r => Equals(SelectedTab, r.Tab)))
        {
            // Set scroll offset so the first tab rendered is the
            TabScrollOffset = Math.Max(0, tabs.IndexOf(SelectedTab));
        }
    }


    /// <summary>
    /// Returns which tabs to render at each x location
    /// </summary>
    /// <returns></returns>
    private IEnumerable<TabToRender> CalculateViewport(Rect bounds)
    {
        int i = 1;

        // Starting at the first or scrolled to tab
        foreach (var tab in Tabs.Skip(TabScrollOffset))
        {
            // while there is space for the tab
            var tabTextWidth = tab.Text.Sum(c => Rune.ColumnWidth(c));

            string text = tab.Text.ToString();

            // The maximum number of characters to use for the tab name as specified
            // by the user (MaxTabTextWidth).  But not more than the width of the view
            // or we won't even be able to render a single tab!
            var maxWidth = Math.Max(0, Math.Min(bounds.Width - 3, MaxTabTextWidth));

            // if tab view is width <= 3 don't render any tabs
            if (maxWidth == 0)
            {
                yield return new TabToRender(i, tab, string.Empty, Equals(SelectedTab, tab), 0);
                break;
            }

            if (tabTextWidth > maxWidth)
            {
                text = tab.Text.ToString().Substring(0, (int)maxWidth);
                tabTextWidth = (int)maxWidth;
            }

            // if there is not enough space for this tab
            if (i + tabTextWidth >= bounds.Width)
            {
                break;
            }

            // there is enough space!
            yield return new TabToRender(i, tab, text, Equals(SelectedTab, tab), tabTextWidth);
            i += tabTextWidth + 1;
        }
    }


    /// <summary>
    /// Adds the given <paramref name="tab"/> to <see cref="Tabs"/>
    /// </summary>
    /// <param name="tab"></param>
    /// <param name="andSelect">True to make the newly added Tab the <see cref="SelectedTab"/></param>
    public void AddTab(TabView.Tab tab, bool andSelect)
    {
        if (tabs.Contains(tab))
        {
            return;
        }


        tabs.Add(tab);

        if (SelectedTab == null || andSelect)
        {
            SelectedTab = tab;

            EnsureSelectedTabIsVisible();

            tab.View?.SetFocus();
        }

        SetNeedsDisplay();
    }


    /// <summary>
    /// Removes the given <paramref name="tab"/> from <see cref="Tabs"/>.
    /// Caller is responsible for disposing the tab's hosted <see cref="View"/>
    /// if appropriate.
    /// </summary>
    /// <param name="tab"></param>
    public void RemoveTab(TabView.Tab tab)
    {
        if (tab == null || !tabs.Contains(tab))
        {
            return;
        }

        // what tab was selected before closing
        var idx = tabs.IndexOf(tab);

        tabs.Remove(tab);

        // if the currently selected tab is no longer a member of Tabs
        if (SelectedTab == null || !Tabs.Contains(SelectedTab))
        {
            // select the tab closest to the one that disappeared
            var toSelect = Math.Max(idx - 1, 0);

            if (toSelect < Tabs.Count)
            {
                SelectedTab = Tabs.ElementAt(toSelect);
            }
            else
            {
                SelectedTab = Tabs.LastOrDefault();
            }
        }

        EnsureSelectedTabIsVisible();
        SetNeedsDisplay();
    }

    private class TabToRender
    {
        public int X { get; set; }
        public TabView.Tab Tab { get; set; }

        /// <summary>
        /// True if the tab that is being rendered is the selected one
        /// </summary>
        /// <value></value>
        public bool IsSelected { get; set; }

        public int Width { get; }
        public string TextToRender { get; }

        public TabToRender(int x, TabView.Tab tab, string textToRender, bool isSelected, int width)
        {
            X = x;
            Tab = tab;
            IsSelected = isSelected;
            Width = width;
            TextToRender = textToRender;
        }
    }

    /// <summary>
    /// Raises the <see cref="TabClicked"/> event.
    /// </summary>
    /// <param name="tabMouseEventArgs"></param>
    protected virtual private void OnTabClicked(TabMouseEventArgs tabMouseEventArgs)
    {
        TabClicked?.Invoke(this, tabMouseEventArgs);
    }

    /// <summary>
    /// Describes a mouse event over a specific <see cref="TabView.Tab"/> in a <see cref="TabView"/>.
    /// </summary>
    public class TabMouseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="TabView.Tab"/> (if any) that the mouse
        /// was over when the <see cref="MouseEvent"/> occurred.
        /// </summary>
        /// <remarks>This will be null if the click is after last tab
        /// or before first.</remarks>
        public TabView.Tab Tab { get; }

        /// <summary>
        /// Gets the actual mouse event.  Use <see cref="MouseEvent.Handled"/> to cancel this event
        /// and perform custom behavior (e.g. show a context menu).
        /// </summary>
        public MouseEvent MouseEvent { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="TabMouseEventArgs"/> class.
        /// </summary>
        /// <param name="tab"><see cref="TabView.Tab"/> that the mouse was over when the event occurred.</param>
        /// <param name="mouseEvent">The mouse activity being reported</param>
        public TabMouseEventArgs(TabView.Tab tab, MouseEvent mouseEvent)
        {
            Tab = tab;
            MouseEvent = mouseEvent;
        }
    }
}