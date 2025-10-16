using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using AWS.Settings;
using AWS.Views;
using DocumentFormat.OpenXml.Spreadsheet;
using PortsWork;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Devices;

public partial class DevicesWindow : Window
{
   public DevicesCommunication devices;
   public bool Work_DO = true;
   public bool _reallyClose = false;
    public DevicesWindow()
    {
        InitializeComponent();
        devices = new DevicesCommunication();
        devices.address = 10;
        devices.TimeSleep = 2;
        PortsListReload();
        this.Closing += Devices_Closing;
    }
    private void Devices_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_reallyClose)
        {
            return;
        }
        e.Cancel = true;  // отменяем стандартное закрытие
        this.Hide();      // просто прячем окно
    }
    protected void PortsListReload()
    {
        Console.WriteLine("PortsListReload---------------");
        InitializeAllComboBoxes(devices.GetAllPorts());
        DevicesCommunication.CreateMessege("Порты обновлены");
    }
    private void InitializeAllComboBoxes(IEnumerable<string> portItems)
    {
        Port_Name_Agiletn.ItemsSource = portItems;
        if (!devices.mult_is_open) Port_Name_Agiletn.SelectedIndex = 0;

        Port_Name_Generator.ItemsSource = portItems;
        if (!devices.gen_is_open) Port_Name_Generator.SelectedIndex = 0;

        Port_Name_PLC.ItemsSource = portItems;
        if (!devices.PLC.IsOpen) Port_Name_PLC.SelectedIndex = 0;

        Port_Name_SG004.ItemsSource = portItems;
        if (!devices.sg004.IsOpen) Port_Name_SG004.SelectedIndex = 0;
    }
    private void Button_Open_Port_SG004(object? sender, RoutedEventArgs e)
    {
        //OpenPorts(devices.PLC, Port_Name_PLC.SelectedItem.ToString());
        try
        {
            if (devices.sg004.IsOpen) return;
            devices.sg004.PortName = Port_Name_SG004.SelectedItem.ToString();
            Task.Run(async () =>
            {
                if (devices.sg004.OpenPort())
                {
                    DevicesCommunication.CreateMessege(devices.info[104]);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Port_Name_SG004.IsEnabled = false;
                        Panel_SG004.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#1DEC1D"));
                    });
                }
                else DevicesCommunication.CreateMessege(devices.info[114]);
            });
        }
        catch (Exception ex)
        {
            DevicesCommunication.CreateMessege($"Ошибка: {ex.Message}");
        }
    }
    private void Button_Open_Port_PLC(object? sender, RoutedEventArgs e)
    {
        //OpenPorts(devices.PLC, Port_Name_PLC.SelectedItem.ToString());
        try
        {
            if (devices.PLC.IsOpen) return;
            //

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
                devices.PLC = (ModbusRTU)devices.SetMeasureDeviceName(devices.PLC, Port_Name_PLC.SelectedItem.ToString());
            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) devices.PLC.SetName(Port_Name_PLC.SelectedItem.ToString());
            //else throw new PlatformNotSupportedException("Unsupported OS");

            devices.PLC.SetParameters(115200, (StopBits)1);
            Task.Run(async () =>
            {
                if (devices.PLC.OpenPort())
                {
                    DevicesCommunication.CreateMessege(devices.info[103]);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Port_Name_PLC.IsEnabled = false;
                        Panel_PLC.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#1DEC1D"));
                    });
                }
                else DevicesCommunication.CreateMessege(devices.info[113]);
            });
        }
        catch (Exception ex)
        {
            DevicesCommunication.CreateMessege($"Ошибка: {ex.Message}");
        }
    }
    private void Button_Open_Port_Generator(object? sender, RoutedEventArgs e)
    {
        //OpenPorts(devices.generator, Port_Name_Generator.SelectedItem.ToString());

        try
        {
            if (devices.gen_is_open) return;

            // 


            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
                devices.generator = (PortGenerator)devices.SetMeasureDeviceName(devices.generator, Port_Name_Generator.SelectedItem.ToString());
            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) devices.generator.SetName(Port_Name_Generator.SelectedItem.ToString());
            //else throw new PlatformNotSupportedException("Unsupported OS");
            Task.Run(async () =>
            {
                if (devices.generator.OpenPort())
                {
                    DevicesCommunication.CreateMessege(devices.info[101]);
                    devices.gen_is_open = true;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Panel_Generator.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#1DEC1D"));
                        Port_Name_Generator.IsEnabled = false;

                        if (Option1.IsChecked == true)
                        {
                            devices.generator.SetChannel(1);
                        }
                        if (Option2.IsChecked == true)
                        {
                            devices.generator.SetChannel(2);
                        }
                    });

                }
                else DevicesCommunication.CreateMessege(devices.info[111]);
            });
        }
        catch (Exception ex)
        {
            DevicesCommunication.CreateMessege($"Ошибка: {ex.Message}");
        }

    }
    private void Button_Open_Port_Agilent(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("----------Button_Open_Port_Agilent");
        // OpenPorts(devices.multimeter, Port_Name_Agiletn.SelectedItem.ToString());
        try
        {
            if (devices.mult_is_open)
            {
                return;
            }
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
                devices.multimeter = (PortMultimeter)devices.SetMeasureDeviceName(devices.multimeter, Port_Name_Agiletn.SelectedItem.ToString());
                Console.WriteLine("yes");
            //}
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) devices.multimeter.SetName(Port_Name_Agiletn.SelectedItem.ToString());
            //else throw new PlatformNotSupportedException("Unsupported OS");
            Task.Run(async () =>
            {
                Console.WriteLine("async");
                Console.WriteLine(devices.multimeter.GetName());
                if (devices.multimeter.OpenPort())
                {
                    DevicesCommunication.CreateMessege(devices.info[102]);
                    devices.mult_is_open = true;
                    Start_DC_Read_Work();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Panel_Agilent.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#1DEC1D"));
                        Port_Name_Agiletn.IsEnabled = false;
                    });
                }
                else DevicesCommunication.CreateMessege(devices.info[112]);
            });


        }
        catch (Exception ex)
        {
            DevicesCommunication.CreateMessege($"Ошибка: {ex.Message}");
        }
    }
    private async void OpenPorts(Port device, string port)
    {
        await Task.Run(async () =>
        {
            try
            {
                device.SetName(port);
                device = (Port)devices.SetMeasureDeviceName(device, port);
                if (device == devices.PLC) devices.PLC.SetParameters(115200, (StopBits)1);
                if (device.OpenPort())
                {
                    DevicesCommunication.CreateMessege(port);
                }
            }
            catch (Exception)
            {
                DevicesCommunication.CreateMessege(devices.info[110]);
            }
        });
    }
    private void Button_Close_Port_SG004(object? sender, RoutedEventArgs e)
    {
        if (devices.sg004.IsOpen)
        {
            devices.sg004.ClosePort();
            DevicesCommunication.CreateMessege(devices.info[134]);
            Panel_SG004.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
            Port_Name_SG004.IsEnabled = true;
        }
    }
    private void Button_Close_Port_PLC(object? sender, RoutedEventArgs e)
    {
        if (devices.PLC.IsOpen)
        {
            devices.PLC.ClosePort();
            DevicesCommunication.CreateMessege(devices.info[133]);
            Panel_PLC.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
            Port_Name_PLC.IsEnabled = true;
        }
    }

    private void Button_Close_Port_Generator(object? sender, RoutedEventArgs e)
    {
        if (devices.gen_is_open)
        {
            devices.generator.ClosePort();
            devices.gen_is_open = false;
            DevicesCommunication.CreateMessege(devices.info[131]);
            Panel_Generator.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
            Port_Name_Generator.IsEnabled = true;
        }
    }
    private void Button_Close_Port_Agilent(object? sender, RoutedEventArgs e)
    {
        if (devices.mult_is_open)
        {
            devices.multimeter.ClosePort();
            devices.mult_is_open = false;
            DevicesCommunication.CreateMessege(devices.info[132]);
            Panel_Agilent.Background = new SolidColorBrush(Avalonia.Media.Colors.LightGray);
            Port_Name_Agiletn.IsEnabled = true;
        }
    }

    private void Button_Update_Ports(object? sender, RoutedEventArgs e)
    {
        PortsListReload();
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
                            DevicesCommunication.CreateMessege((ex.Message));
                        }
                    }
                    Thread.Sleep(300);
                }
            });

        }
        catch (Exception ex)
        {
            DevicesCommunication.CreateMessege((ex.Message));
        }

    }
}