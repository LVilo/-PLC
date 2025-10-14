using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AWS.ViewModels;
using PortsWork;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Views
{
    public partial class MainWindow : Window
    {
        #region Кнопки
        private async void Button_Show_Diveces(object? sender, RoutedEventArgs e)
        {
            Show.Show();       
        }
        private async void Button_Setting_Volt(object? sender, RoutedEventArgs e)
        {
            Do_Work(0);
        }
        private async void Button_Setting_IEPE(object? sender, RoutedEventArgs e)
        {
            Do_Work(1);
        }

        private async void Button_Setting_4_20_Input(object? sender, RoutedEventArgs e)
        {
            Do_Work(2);
        }
        private async void Button_Setting_4_20_Output(object? sender, RoutedEventArgs e)
        {
            Do_Work(3);
        }
        private void Setting_Rs_485_Click(object? sender, RoutedEventArgs e)
        {
            Do_Work(4);
        }
        private async void Save_Reg_Button(object? sender, RoutedEventArgs e)
        {
            Do_Work(5);
        }
        private async void Button_Start(object? sender, RoutedEventArgs e)
        {
            Do_Work(Name_PLC.SelectionBoxItem.ToString());
        }

        #endregion
    }
}
