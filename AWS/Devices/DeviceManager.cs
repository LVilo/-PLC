using PortsWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
namespace AWS.Devices
{
    public class DeviceManager
    {
        public PortMultimeter Multimeter;
        public PortGenerator Generator;
        public PLC plc;
        public SG004AProtocol SG004;


        public DeviceManager()
        {
            Multimeter = new PortMultimeter();
            Generator = new PortGenerator();
            plc = new PLC();
            SG004 = new SG004AProtocol();
        }
        public bool OpenPort(Port device)
        {
            if(device.OpenPort())
            {

                return true;
            }
            return false;
        }
        public void CloseConnection()
        {
            Multimeter.ClosePort();
            Generator.ClosePort();
            plc.ClosePort();
            SG004.ClosePort();
        }
        public bool AllDevicesReady => Multimeter.IsOpen && Generator.IsOpen && plc.IsOpen && SG004.IsOpen;
        public bool ReadyForCheckVolt => plc.IsOpen;
        public bool ReadyForSetting4_20 => plc.IsOpen && Multimeter.IsOpen  && SG004.IsOpen;
        public bool ReadyForSettingIEPE => Multimeter.IsOpen && Generator.IsOpen && plc.IsOpen;
        public bool ReadyForSettingRS45 => plc.IsOpen;
    }

}
