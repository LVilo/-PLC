using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AWS.Views.ImagesView;

namespace AWS;

public partial class Output_4_20 : Window
{
    public Output_4_20()
    {
        InitializeComponent();
        Image_Panel.Source = ImagesViewWin.LoadEmbeddedImage("AWS.Images.4-20Output.png");
    }
}