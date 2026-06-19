using System;
using Avalonia.Controls;

namespace TimeLedger.Desktop.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        Console.WriteLine("MainView constructor called");
        InitializeComponent();
        this.Loaded += MainView_Loaded;
        Console.WriteLine("MainView Loaded handler attached");
    }

    private void MainView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var scrollViewer = this.FindControl<ScrollViewer>("EventsScrollViewer");
        Console.WriteLine($"ScrollViewer found: {scrollViewer != null}");

        if (scrollViewer != null)
        {
            scrollViewer.ScrollChanged += (s, args) =>
            {
                var sv = (ScrollViewer)s!;
                Console.WriteLine($"Offset={sv.Offset.Y:F0}, Extent={sv.Extent.Height:F0}, Viewport={sv.Viewport.Height:F0}");
            };
        }
    }
}