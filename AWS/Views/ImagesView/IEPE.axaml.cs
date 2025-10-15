using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AWS.Views.ImagesView;

namespace AWS;

public partial class IEPE : Window
{
    public IEPE()
    {
        InitializeComponent();
        Image_Panel.Source = ImagesViewWin.LoadEmbeddedImage("AWS.Images.IEPE.png");
    }
}