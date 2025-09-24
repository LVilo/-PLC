using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using AWS.ViewModels;
using System;
using System.IO;
using System.Reflection;
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
            case "IEPE": Image_Panel.Source =  LoadEmbeddedImage("AWS.Images.IEPE.png")  ; break; 
            case "4-20 входное": Image_Panel.Source =  LoadEmbeddedImage("AWS.Images.4-20 Вход.png"); break;
            case "4-20 выходное": Image_Panel.Source =  LoadEmbeddedImage("AWS.Images.4-20 Выход.png"); break;
        }
        Label_Text.Content = Text;
        Title = Setting;
    }
    public Dialog(string Text)
    {
        InitializeComponent();
        Border_Name.IsVisible = false;
        Label_Text.Content = Text;
        Title = Text;
    }
    public bool Dialog_result { get; private set; }
    public bool Dialog_skip { get; private set; }
    private static Bitmap LoadEmbeddedImage(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
            throw new FileNotFoundException($"Ресурс не найден: {resourceName}");

        return new Bitmap(stream);
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
        Dialog_skip = true;
        Close();
    }
    //internal async Task ShowDialog()
    //{
    //    throw new NotImplementedException();
    //}
}
//public static class DialogExtensions
//{
//    public static async Task<bool> ShowDialogWithResultAsync(this Dialog dialog, Window owner)
//    {
//        await dialog.ShowDialog(owner);
//        return dialog.GetResult();
//    }

//    public static bool GetResult(this Window window)
//    {
//        // В Avalonia результат хранится в Window.Result
//        return window. is bool result && result;
//    }
//}