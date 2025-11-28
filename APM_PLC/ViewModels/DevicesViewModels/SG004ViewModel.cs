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

namespace APM_PLC.ViewModels.DevicesViewModels
{
    public partial class SG004ViewModel : DevicesContext
    {

        public override async Task<bool?> OpenPort()
        {
            Devices.Instance.sg004 = new SG004AProtocol();
            Devices.Instance.sg004 = (SG004AProtocol)Devices.Instance.SetMeasureDeviceName(Devices.Instance.sg004, PortItem);

           if( await Devices.Instance.OpenPort(Devices.Instance.sg004) is true)
            {
                return Devices.Instance.sg004.IsOpened();
            }
            return false;
        }
       
        public override async Task ClosePort()
        {
            Devices.Instance.ClosePort(Devices.Instance.sg004);

        }
    }
}

