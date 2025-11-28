using APM_PLC.Models.DevicesModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Input;
using APM_PLC.ViewModels.DevicesViewModels;
using System.Diagnostics;
using System;

namespace APM_PLC.ViewModels
{
    public partial class DevicesViewModel : ViewModelBase
    {

        public IAsyncRelayCommand OpenGeneratorCommand { get; }
        public IAsyncRelayCommand OpenAgilentCommand { get; }
        public IAsyncRelayCommand OpenPLCCommand { get; }
        public IAsyncRelayCommand OpenSG004Command { get; }


        public IAsyncRelayCommand CloseGeneratorCommand { get; }
        public IAsyncRelayCommand CloseAgilentCommand { get; }
        public IAsyncRelayCommand ClosePLCCommand { get; }
        public IAsyncRelayCommand CloseSG004Command { get; }



        //public AgilentViewModel Agilent { get; }
        //public PLCViewModel PLC { get; }
        //public SG004ViewModel SG004 { get; }
        //public GeneratorViewModel Generator { get; }
        public AgilentViewModel Agilent { get; } = new AgilentViewModel();
        public ControllerViewModel Controller { get; } = new ControllerViewModel();
        public GeneratorViewModel Generator { get; } = new GeneratorViewModel();
        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;


        public DevicesViewModel()
        {

            OpenGeneratorCommand = new AsyncRelayCommand(() => OpenPort(Generator));
            OpenAgilentCommand = new AsyncRelayCommand(() => OpenPort(Agilent));
            OpenPLCCommand = new AsyncRelayCommand(() => OpenPort(Controller));

            CloseGeneratorCommand = new AsyncRelayCommand(() => ClosePort(Generator));
            CloseAgilentCommand = new AsyncRelayCommand(() => ClosePort(Agilent));
            ClosePLCCommand = new AsyncRelayCommand(() => ClosePort(Controller));

            UpdatesPorts();
        }

        //[RelayCommand]
        private async Task OpenPort(DevicesContext viewmodel/*,Port device*/)
        {
            try
            {
                if (await viewmodel.OpenPort() is true)
                {
                    viewmodel.SelectedColor = "#FF1DEC1D";
                    LogerViewModel.Write($"{viewmodel.PortItem} подключен");
                }
                else
                {
                    viewmodel.SelectedColor = "#FFD3D3D3";
                    LogerViewModel.Write($"{viewmodel.PortItem} не  подключен");
                }
            }
            catch (Exception ex)
            {
                LogerViewModel.Write(ex.Message);
            }
        }
        //[RelayCommand]
        public async Task ClosePort(DevicesContext viewmodel/*, Port device*/)
        {
            await viewmodel.ClosePort();
            viewmodel.SelectedColor = "#FFD3D3D3";
        }
        [ObservableProperty]
        private string?[] _Ports;

        [RelayCommand]
        public void UpdatesPorts()
        {
            try
            {
                Ports = Devices.Instance.GetAllPorts();
                if (Devices.Instance.multimeter.IsOpened() is false) Agilent.PortItem = Ports[0];
                if (Devices.Instance.generator.IsOpened() is false) Generator.PortItem = Ports[0];
                if (Devices.Instance.controller.IsOpened() is false) Controller.PortItem = Ports[0];
            }
            catch (Exception ex)
            {
                LogerViewModel.Write(ex.Message);
            }
        }

    }
}

