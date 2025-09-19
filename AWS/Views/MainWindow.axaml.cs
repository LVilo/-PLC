using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AWS.ViewModels;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using PortsWork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Views;

public partial class MainWindow : Window
{
    DevicesCommunication devices;
    private bool Work_DO = true;
    private bool _showDriverError = false;
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            devices = new DevicesCommunication();
            
            PortsListReload();
            this.Closing += MainWindow_Closing;
            devices.address = 10;
            devices.TimeSleep = 2;
            StartBackgroundWork();
        }
        catch (DllNotFoundException)
        {
            // Отложим показ ошибки до момента, когда окно уже будет открыто
            _showDriverError = true;
        }
    }
    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (_showDriverError)
        {
            await ShowDriverErrorDialog();
            Close();
        }
    }
    private async Task ShowDriverErrorDialog()
    {
        var textBlock = new TextBlock
        {
            Text = "Для работы приложения требуется драйвер RS VISA 5.5.5.\n\n" +
                    "Пожалуйста, установите RS_VISA_Setup_Win_5_5_5 и перезапустите приложение.\n\n" +
                    "Драйвер можно установить с папки приложения.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(20)
        };

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        var dialog = new Window
        {
            Title = "Ошибка",
            Width = 400,
            Height = 150,
            Content = new StackPanel
            {
                Children =
            {
                textBlock,
                new Separator { Margin = new Thickness(0, 10) },
                okButton
            },
                Margin = new Thickness(10)
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        okButton.Click += (_, __) => dialog.Close();

        await dialog.ShowDialog(this);
    }
    protected async void Start_DC_Read_Work()
    {
        try
        {
            await Task.Run(() =>
            {
                while (Work_DO)
                {
                    if (devices.DC_Read && devices.mult_is_open)
                    {
                        try
                        {
                            devices.currentVolt = devices.multimeter.GetVoltage(PortMultimeter.SIGNALTYPE_DC, 100);
                        }
                        catch (InvalidOperationException ex)
                        {
                            devices.CreateMessege((ex.Message));
                        }
                    }
                    Thread.Sleep(300);
                }
            });

        }
        catch (Exception ex)
        {
            devices.CreateMessege((ex.Message));
        }

    }
    private async void StartBackgroundWork()
    {
        // Показываем индикатор загрузки


        try
        {
            // Запускаем фоновую задачу
            await Task.Run(() =>
            {
                while (true)
                {
                    if (devices.messege.Count > 0)
                    {
                        LogWrite(devices.messege.Dequeue());
                    }
                    Thread.Sleep(1000);
                }
            });

            // Этот код выполнится после завершения задачи в UI потоке
        }
        catch (Exception ex)
        {
            LogWrite($"Ошибка: {ex.Message}");
        }
    }
  

    protected async void Do_Work(int code)
    {
        Set_Enabled(false);
        await Task.Run(async () =>
        {
            try
            {
                switch (code)
                {
                    case 0://настройка напряжения
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage();
                        break;
                    case 1: // IEPE
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Seting_IEPE();
                        break;

                    case 2:// 4-20
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Setting_4_20_Input();
                        break;

                    case 3:
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Setting_4_20_Output();
                        break;
                    case 4:
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await Settig_485();
                        break;
                    case 5:
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await MakeReportAsync(Name_PLC.SelectionBoxItem.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                devices.CreateMessege(ex.Message);
            }
        });
        Set_Enabled(true);
    }
    
    protected async void Do_Work(string PLC)
    {
        Set_Enabled(false);
        await (Task.Run(async () =>
        {
            try
            {
                switch (PLC)
                {
                    case "PLC 112":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage();
                        await Setting_4_20_Input();
                        await Setting_4_20_Output();
                        break;
                    case "PLC 121":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage();
                        await Seting_IEPE(); break;

                    case "PLC 481":
                        if (!devices.mult_is_open) throw new Exception(devices.info[122]);
                        if (!devices.generator.IsOpen) throw new Exception(devices.info[121]);
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage();
                        await Seting_IEPE();
                        await Setting_4_20_Input();
                        await Setting_4_20_Output();
                        break;

                    case "PLC 991":
                        if (!devices.PLC.IsOpen) throw new Exception(devices.info[123]);
                        await CheckVoltage();
                        await Settig_485();
                        break;
                }
                await MakeReportAsync(PLC);
            }
            catch (Exception ex)
            {
                devices.CreateMessege(ex.Message);
            }
        }));
        Set_Enabled(true);
    }
    private void Set_Enabled(bool BOOL)
    {
        Setting_PLC.IsEnabled = BOOL;
        Setting_Volt.IsEnabled = BOOL;
        Setting_IEPE.IsEnabled = BOOL;
        Setting_4_20_Input_but.IsEnabled = BOOL;
        Setting_4_20_Output_but.IsEnabled = BOOL;
        Setting_Rs_485.IsEnabled = BOOL;
        Save_Reg.IsEnabled = BOOL;
    }

    protected void PortsListReload()
    {
        InitializeAllComboBoxes(devices.GetAllPorts());
        devices.CreateMessege("Порты обновлены");
    }
    private void InitializeAllComboBoxes(IEnumerable<string> portItems)
    {
        Port_Name_Agiletn.ItemsSource = portItems;
        if (!devices.mult_is_open) Port_Name_Agiletn.SelectedIndex = 0;

        Port_Name_Generator.ItemsSource = portItems;
        if (!devices.gen_is_open) Port_Name_Generator.SelectedIndex = 0;

        Port_Name_PLC.ItemsSource = portItems;
        if (!devices.PLC.IsOpen) Port_Name_PLC.SelectedIndex = 0;

    }

    private void LogWrite(string message)
    {
        var formattedMessage = $"{DateTime.Now:HH:mm:ss} {message}\r\n";

        Dispatcher.UIThread.Post(() =>
        {
            LogTextBox.Text += formattedMessage;
            LogTextBox.CaretIndex = int.MaxValue; // Прокрутка вниз
        });
    }
    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        LogWrite("Приложение закрывается");
        Work_DO = false;


        Thread.Sleep(1000);
        devices.CloseConnection();
        
    }
    private void Serial_Number_PreviewTextInput(object sender, TextChangedEventArgs e)
    {

    }
    private void Order_Number_PreviewTextInput(object sender, TextChangedEventArgs e)
    {

    }
}

