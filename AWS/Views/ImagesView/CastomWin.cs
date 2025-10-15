using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;

namespace AWS
{
    public class CasomWin : Window
    {
        private Image _imagePanel;
        private Label _label;
        private Border _borderName;
        private Button _stopButton;
        private Button _okButton;
        private Button _skipButton;

        public CasomWin(string title, string path)
        {
            Title = title;
            Width = 400;
            Height = 300;

            // Основная панель
            var mainPanel = new StackPanel
            {
                Name = "Dialog_Panel"
            };

            // Рамка с изображением
            _borderName = new Border
            {
                Name = "Border_Name",
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                IsVisible = true,
                Margin = new Thickness(5)
            };

            _imagePanel = new Image
            {
                Name = "Image_Panel",
                Stretch = Avalonia.Media.Stretch.Uniform
            };

            _borderName.Child = _imagePanel;
            mainPanel.Children.Add(_borderName);

            // Панель с кнопками
            var buttonPanel = new StackPanel
            {
                Margin = new Thickness(5)
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("150,150,150"),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            _stopButton = new Button
            {
                Content = "Остановить",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(_stopButton, 0);

            _okButton = new Button
            {
                Content = "ОК",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(_okButton, 1);

            _skipButton = new Button
            {
                Content = "Пропустить",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Right,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(_skipButton, 2);

            grid.Children.Add(_stopButton);
            grid.Children.Add(_okButton);
            grid.Children.Add(_skipButton);

            buttonPanel.Children.Add(grid);
            mainPanel.Children.Add(buttonPanel);

            Content = mainPanel;
        }
    }
}
