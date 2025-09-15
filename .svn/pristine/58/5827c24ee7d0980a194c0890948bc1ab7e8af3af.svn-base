using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace AWS;

public partial class Dialog : Window
{
    public Dialog(string Text, string Setting)
    {
        InitializeComponent();
        Border_Name.IsVisible = true;
        switch (Setting)
        {
            case "IEPE": Image_Panel.Source = new Bitmap("Images\\IEPE.png") ; Label_Text.Content = Text; break;
            case "4-20 входное": Image_Panel.Source = new Bitmap("Images\\4-20 Вход.png"); Label_Text.Content = Text; break;
            case "4-20 выходное": Image_Panel.Source = new Bitmap("Images\\4-20 Выход.png"); Label_Text.Content = Text; break;
           // case "RS485": Image_Panel.Source = new Bitmap("Images\\4-20 Вход.png"); Label_Text.Content = Text; break;
        }
        
    }
    public Dialog(string Text)
    {
        InitializeComponent();
        Border_Name.IsVisible = false;
        Label_Text.Content = Text;

    }
    public bool IS_Clicked = false;
    private async void OK_Click(object? sender, RoutedEventArgs e)
    {
        IS_Clicked = true;
        Close();
    }
}