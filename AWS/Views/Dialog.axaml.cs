using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AWS.ViewModels;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AWS;

public partial class Dialog : Window
{
    public Dialog(string Text, string path)
    {
        InitializeComponent();
        Border_Name.IsVisible = true;
        
        Image_Panel.Source = LoadEmbeddedImage(path);
        Label_Text.Text = Text;
        Title = "Настройка";
    }
    public Dialog(string Text)
    {
        InitializeComponent();
        Border_Name.IsVisible = false;
        Label_Text.Text = Text;
        Title = Text;
        Height = 124;
    }
    public bool Dialog_result { get; private set; }
    public bool Dialog_Cancel { get; private set; }
    private static Bitmap LoadEmbeddedImage(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resourceName);
        foreach (var name in assembly.GetManifestResourceNames())
            Console.WriteLine(name);
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
        Dialog_Cancel = true;
        Close();
    }
    
}