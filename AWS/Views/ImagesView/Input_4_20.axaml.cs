using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AWS.Views.ImagesView;
namespace AWS;

public partial class Input_4_20 : Window
{
    public Input_4_20()
    {
        InitializeComponent();
        Image_Panel.Source = ImagesViewWin.LoadEmbeddedImage("AWS.Images.4-20Input.png");
    }
}