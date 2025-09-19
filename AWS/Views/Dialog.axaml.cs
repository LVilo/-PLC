using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.Threading.Tasks;

namespace AWS;

public partial class Dialog : Window
{
    public Dialog(string Text, string Setting)
    {
        InitializeComponent();
        Border_Name.IsVisible = true;
        switch (Setting)
        {
            case "IEPE": Image_Panel.Source = new Bitmap("Images\\IEPE.png") ; Label_Text.Content = Text; Title = Text; break; 
            case "4-20 входное": Image_Panel.Source = new Bitmap("Images\\4-20 Вход.png"); Label_Text.Content = Text; Title = Text; break;
            case "4-20 выходное": Image_Panel.Source = new Bitmap("Images\\4-20 Выход.png"); Label_Text.Content = Text; Title = Text; break;
           // case "RS485": Image_Panel.Source = new Bitmap("Images\\4-20 Вход.png"); Label_Text.Content = Text; break;
        }
        
    }
    public Dialog(string Text)
    {
        InitializeComponent();
        Border_Name.IsVisible = false;
        Label_Text.Content = Text;
    }
    public bool Dialog_result { get; private set; }
    private async void OK_Click(object? sender, RoutedEventArgs e)
    {
        Dialog_result = true;
        Close();
    }

    private async void Canel_Click(object? sender, RoutedEventArgs e)
    {
        Dialog_result = false;
        Close();
    }
    internal async Task ShowDialog()
    {
        throw new NotImplementedException();
    }
}
public static class DialogExtensions
{
    public static async Task<bool> ShowDialogWithResultAsync(this Dialog dialog, Window owner)
    {
        await dialog.ShowDialog(owner);
        return dialog.GetResult();
    }

    public static bool GetResult(this Window window)
    {
        // В Avalonia результат хранится в Window.Result
        return window. is bool result && result;
    }
}