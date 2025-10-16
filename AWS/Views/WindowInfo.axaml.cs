using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AWS;

public partial class WindowInfo : Window
{
    public WindowInfo(string text,string title)
    {
        InitializeComponent();
        Info.Text = text;
        Title = title;
    }
    private void Click_OK(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}