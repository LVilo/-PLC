using Avalonia.Controls;
using Avalonia.Interactivity;


namespace AWS;

public partial class Dialoginfo : Window
{
    public bool Dialog_result { get; private set; }
    public bool Dialog_Cancel { get; private set; }
    public Dialoginfo()
    {
        InitializeComponent();
       
    }
    private async void OK_Click(object? sender, RoutedEventArgs e)
    {
        Dialog_result = true;
        Close();
    }

    private async void Skip_Click(object? sender, RoutedEventArgs e)
    {
        Dialog_result = false;
        Close();
    }
    private async void Canel_Click(object? sender, RoutedEventArgs e)
    {
        Dialog_Cancel = true;
        Close();
    }
}