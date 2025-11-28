using Avalonia.Media;
using APM_PLC.Models.DevicesModel;
using CommunityToolkit.Mvvm.Input;
using PortsWork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace APM_PLC.ViewModels.DevicesViewModels
{
    public partial class GeneratorViewModel : DevicesContext
    {
        [ObservableProperty] private bool _chanel_1 = true;
        [ObservableProperty] private bool _chanel_2 = false;

        public override async Task<bool?> OpenPort()
        {
            Devices.Instance.generator = new PortGenerator();
            Devices.Instance.generator = (PortGenerator)Devices.Instance.SetMeasureDeviceName(Devices.Instance.generator,  PortItem);
            if( await Devices.Instance.OpenPort(Devices.Instance.generator) is true)
            {
                int chanel = Chanel_1 is true ? 1 : 2;
                Devices.Instance.generator.SetChannel(chanel);
                return Devices.Instance.generator.IsOpened();
            }
            return false;
        }
        
        public override async Task ClosePort()
        {
            Devices.Instance.ClosePort(Devices.Instance.generator);
        }
    }
}
