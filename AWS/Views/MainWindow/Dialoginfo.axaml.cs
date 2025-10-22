using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AWS.Devices;
using AWS.Settings;
using AWS.ViewModels;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using PortsWork;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace AWS;

public partial class Dialoginfo : Window
{
    public bool Dialog_result { get; private set; }
    public bool Dialog_Cancel { get; private set; }
    public Dialoginfo()
    {
        InitializeComponent();
        OK.Content = "ОК";
        Skip.Content = "Пропустить";
        Cancel.Content = "Отмена";
        Title = "Информация";
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