using Avalonia;
using Avalonia.Controls;

namespace APM_PLC.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void WriteTextCommand(object? sender, TextChangedEventArgs e)
        {
            var len = LogTextBox.Text?.Length ?? 0;
            LogTextBox.CaretIndex = len;
            LogTextBox.Focus();
        }
    }
}