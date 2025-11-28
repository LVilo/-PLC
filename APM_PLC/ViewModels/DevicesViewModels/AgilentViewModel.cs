using Avalonia.Media;
using APM_PLC.Models.DevicesModel;
using CommunityToolkit.Mvvm.Input;
using PortsWork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using APM_PLC.Models.Settings;

namespace APM_PLC.ViewModels.DevicesViewModels
{
    public partial class AgilentViewModel : DevicesContext
    {

        public override async Task<bool?> OpenPort()
        {
            Devices.Instance.multimeter = new PortMultimeter();
            Devices.Instance.multimeter = (PortMultimeter)Devices.Instance.SetMeasureDeviceName(Devices.Instance.multimeter, PortItem);

           if( await Devices.Instance.OpenPort(Devices.Instance.multimeter) is true)
            {
                Settings.Mult = new Delay();
                Settings.WhileGetVoltAsync();
                return Devices.Instance.multimeter.IsOpened();
                
            }
            return false;
        }
       
        public override async Task ClosePort()
        {
            Devices.Instance.ClosePort(Devices.Instance.multimeter);

        }
    }
}

