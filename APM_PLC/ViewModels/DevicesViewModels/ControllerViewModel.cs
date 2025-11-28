using APM_PLC.Models;
using APM_PLC.Models.DevicesModel;
using APM_PLC.Models.Settings;
using APM_PLC.ViewModels.DialogViewModels;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PortsWork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace APM_PLC.ViewModels.DevicesViewModels
{
    public partial class ControllerViewModel : DevicesContext
    {

        [ObservableProperty] private string? _settingtext0;
        [ObservableProperty] private string? _settingtext1 = "Авто настройка";
        [ObservableProperty] private string? _settingtext2 = "Настройка DC";
        [ObservableProperty] private string? _settingtext3 = "Настройка AC";
        [ObservableProperty] private string? _settingtext4 = "Проверка";
        [ObservableProperty] private string? _settingtext5 = "Сохранить в файл";

        [ObservableProperty] private string? _orderNumber = "0";
        [ObservableProperty] private string? _serialNumber = "0";

        [ObservableProperty] private string? _addressController = Devices.Instance.controller.address.ToString();

        [ObservableProperty] private string[] _modelController = ["CNV117","CNV127","CNV137","CNV147","CNV157"];
        [ObservableProperty] private string _selectedModel = "CNV117";

        [ObservableProperty] private bool _isWait= true;

        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;

        [ObservableProperty]
        private DialogViewModel? _dialog;

        [ObservableProperty]
        private DialogViewModel? _build;

        [ObservableProperty]
        private DialogViewModel? _paramOther; 

        [ObservableProperty]
        private DialogViewModel? _paramCNV127;

        [ObservableProperty]
        private DialogViewModel? _paramCNV157;

        [ObservableProperty]
        private DialogViewModel? _paramCapacity;

        public IAsyncRelayCommand SettingALL_Command { get; }
        public IAsyncRelayCommand Setting_ParamCommand { get; }
        public IAsyncRelayCommand Setting_1Command { get; }
        public IAsyncRelayCommand Setting_2Command { get; }
        public IAsyncRelayCommand CheckSettingCommand { get; }
        public IAsyncRelayCommand WriteFileCommand { get; }
        Stopwatch stopwatch = new Stopwatch();
        public ControllerViewModel()
        {
            SettingALL_Command = new AsyncRelayCommand(Setting);
            Setting_ParamCommand = new AsyncRelayCommand(Setting_Param);
            Setting_1Command = new AsyncRelayCommand(Setting_1);
            Setting_2Command = new AsyncRelayCommand(Setting_2);
            CheckSettingCommand = new AsyncRelayCommand(CheckSetting);
            WriteFileCommand = new AsyncRelayCommand(WriteFile);

            ItemChangedCommand = new RelayCommand<string?>(OnItemChanged);

            //devices.cnv.settings = await devices.cnv.IdentifySetting();
            //Devices.Instance.controller.settings = new SettingsALL();
            SetText();
        }
        public override async Task<bool?> OpenPort()
        {
            Devices.Instance.controller = new Сontroller();
            Devices.Instance.controller.address = Convert.ToByte(AddressController);
            Devices.Instance.controller.TimeSleep = 2;
            Devices.Instance.controller.ReadTimeout = 3000;
            Devices.Instance.controller.WriteTimeout = 3000;
            Devices.Instance.controller.SetParameters(115200, StopBits.One);
            Devices.Instance.controller = (Сontroller)Devices.Instance.SetMeasureDeviceName(Devices.Instance.controller, PortItem);
            if (await Devices.Instance.OpenPort(Devices.Instance.controller) is true)
            {
               // Devices.Instance.controller.settings = await Devices.Instance.controller.IdentifySetting();
                SetText();
                return true;
            }
            return false;
        }
        private void SetText()
        {
            ModelController = Devices.Instance.controller.settings.TypeItems;
            SelectedModel = Devices.Instance.controller.settings.selectedText;
            Settingtext0 = Devices.Instance.controller.settings.textsetting_0;
            Settingtext2 = Devices.Instance.controller.settings.textsetting_2;
            Settingtext3 = Devices.Instance.controller.settings.textsetting_3;
        }
        partial void OnSelectedModelChanged(string? value)
        {
            ItemChangedCommand.Execute(value);
        }
        public IRelayCommand<string?> ItemChangedCommand { get; }
        private void OnItemChanged(string? newModel)
        {
           // Devices.Instance.controller.SetSetting(newModel);
            Settingtext0 = Devices.Instance.controller.settings.textsetting_0;
            Settingtext2 = Devices.Instance.controller.settings.textsetting_2;
            Settingtext3 = Devices.Instance.controller.settings.textsetting_3;
        }

        public override async Task ClosePort()
        {
            Devices.Instance.ClosePort(Devices.Instance.controller);
        }
        public async Task Setting()
        {
            try
            {

                if (OrderNumber is "0") throw new Exception("Заполните номер заказа");
                if (SerialNumber is "0") throw new Exception("Заполните серийный номер");
                IsWait = false;
                var ConfirmDialogViewModel = new ConfirmDialogViewModel();
                var BuildSchemeViewModel = new BuildSchemeViewModel();

                var ParamOtherDialogViewModel = new ParamCNVOtherDialogViewModel();

                var ParamCNV127DialogViewModel = new ParamCNV127DialogViewModel();
                var ParamCNV157DialogViewModel = new ParamCNV157DialogViewModel();
                var ParamCapacityDialogViewModel = new ParamCapacityDialogViewModel();

                Dialog = ConfirmDialogViewModel;
                Build = BuildSchemeViewModel;
                ParamOther = ParamOtherDialogViewModel;
                ParamCNV127 = ParamCNV127DialogViewModel;
                ParamCNV157 = ParamCNV157DialogViewModel;
                ParamCapacity = ParamCapacityDialogViewModel;

                Devices.Instance.controller.settings.SetType(SelectedModel);
                string starttime = String.Format($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                stopwatch.Restart();
                LogerViewModel.Write($"Начата настройка {SelectedModel}");
                await Devices.Instance.controller.settings.ALLSetting(
                    BuildSchemeViewModel,
                    ConfirmDialogViewModel,
                    ParamOtherDialogViewModel,
                    ParamCNV127DialogViewModel,
                    ParamCNV157DialogViewModel,
                    ParamCapacityDialogViewModel);
                stopwatch.Stop();
                string endtime = String.Format($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                string result = await SaveRegistersModel.MakeReportAsync(SelectedModel, OrderNumber, SerialNumber,"Полная", starttime, endtime, stopwatch.Elapsed, ConfirmDialogViewModel);
                LogerViewModel.Write(result);
                LogerViewModel.Write($"Настройка заняла {stopwatch.Elapsed:mm\\ss}");
                IsWait = true;
            }
            catch (Exception ex)
            {
                LogerViewModel.Write(ex.Message);
                IsWait = true;
            }

        }
        private async Task Setting_Param()
        {
            try
            {
                IsWait = false;
                if (Devices.Instance.controller.IsOpened() is false) return;
                var dialog = new ConfirmDialogViewModel();
                var build = new BuildSchemeViewModel();

                var paramother = new ParamCNVOtherDialogViewModel();

                var paramcnv127 = new ParamCNV127DialogViewModel();
                var paramcnv157 = new ParamCNV157DialogViewModel();
                var paramcapacity = new ParamCapacityDialogViewModel();

                Dialog = dialog;
                Build = build;
                ParamOther = paramother;
                ParamCNV127 = paramcnv127;
                ParamCNV157 = paramcnv157;
                ParamCapacity = paramcapacity;

                Devices.Instance.controller.settings.SetType(SelectedModel);
                stopwatch.Restart();
                await Devices.Instance.controller.settings.SettingParam(paramother, paramcnv127, paramcnv157, paramcapacity, dialog);
                stopwatch.Stop();
                LogerViewModel.Write($"Настройка заняла {stopwatch.Elapsed:mm\\ss}");
                IsWait = true;
            }
            catch (Exception ex)
            {
                LogerViewModel.Write(ex.Message);
                IsWait = true;
            }
            // await Settings.SetVoltage(devices.generator, devices.multimeter, 5, 0, 0.0072);
        }
        private async Task Setting_1()
        {
            try
            {
                if (OrderNumber is "0") throw new Exception("Заполните номер заказа");
                if (SerialNumber is "0") throw new Exception("Заполните серийный номер");
                IsWait = false;
                var dialog = new ConfirmDialogViewModel();
                var build = new BuildSchemeViewModel();

                Dialog = dialog;
                Build = build;

                Devices.Instance.controller.settings.SetType(SelectedModel);
                stopwatch.Restart();
                string starttime = String.Format($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                await Devices.Instance.controller.settings.Setting1(build, dialog);
                stopwatch.Stop();
                string endtime = String.Format($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                string result = await SaveRegistersModel.MakeReportAsync(SelectedModel, OrderNumber, SerialNumber, "DC", starttime, endtime, stopwatch.Elapsed, dialog);
                LogerViewModel.Write(result);
                LogerViewModel.Write($"Настройка заняла {stopwatch.Elapsed:mm\\ss}");
                IsWait = true;
            }
            catch (Exception ex)
            {
                LogerViewModel.Write(ex.Message);
                IsWait = true;
            }
            
        }
        private async Task Setting_2()
        {
            try
            {
                if (OrderNumber is "0") throw new Exception("Заполните номер заказа");
                if (SerialNumber is "0") throw new Exception("Заполните серийный номер");
                IsWait = false;
                var dialog = new ConfirmDialogViewModel();
                var build = new BuildSchemeViewModel();

                Dialog = dialog;
                Build = build;

                Devices.Instance.controller.settings.SetType(SelectedModel);
                stopwatch.Restart();
                string starttime = String.Format($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                await Devices.Instance.controller.settings.Setting2(build, dialog);
                stopwatch.Stop();
                string endtime = String.Format($"{DateTime.Now.Hour}.{DateTime.Now.Minute}");
                string result = await SaveRegistersModel.MakeReportAsync(SelectedModel, OrderNumber, SerialNumber, "AC", starttime, endtime, stopwatch.Elapsed, dialog);

                LogerViewModel.Write(result);
                LogerViewModel.Write($"Настройка заняла {stopwatch.Elapsed:mm\\ss}");
                IsWait = true;
            }
            catch (Exception ex)
            {
                LogerViewModel.Write(ex.Message);
                IsWait = true;
            }
            
        }
        private async Task CheckSetting()
        {
            try
            {
                IsWait = false;
                var dialog = new ConfirmDialogViewModel();
                var build = new BuildSchemeViewModel();

                Dialog = dialog;
                Build = build;

                Devices.Instance.controller.settings.SetType(SelectedModel);
                stopwatch.Restart();
                await Devices.Instance.controller.settings.CheckSetting(build, dialog);
                stopwatch.Stop();
                LogerViewModel.Write($"Проверка заняла {stopwatch.Elapsed:mm\\ss}");
            }
            catch (Exception ex)
            {
                LogerViewModel.Write(ex.Message);
            }
            
        }
        private async Task WriteFile()
        {
            try
            {
                IsWait = false;
                if (Devices.Instance.controller.IsOpened() is false) throw new Exception("RS485 не подключен");
                if (OrderNumber is "0") throw new Exception("Заполните номер заказа");
                if (SerialNumber is "0") throw new Exception("Заполните серийный номер");
                var dialog = new ConfirmDialogViewModel();
                Dialog = dialog;
                Devices.Instance.controller.settings.SetType(SelectedModel);
                LogerViewModel.Write($"Запись регистров {SelectedModel} в файл");
                string result = await SaveRegistersModel.MakeReportAsync(SelectedModel, OrderNumber, SerialNumber, "","", "", TimeSpan.Zero, dialog);
                LogerViewModel.Write(result);
                IsWait = true;
            }
            catch (Exception ex)
            {
                LogerViewModel.Write(ex.Message);
                IsWait = true;
            }
           
        }
    }
}
