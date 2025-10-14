using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortsWork;
namespace AWS.Devices
{
    public class DeviceManager
    {
        public PortMultimeter Multimeter { get; }
        public PortGenerator Generator { get; }
        public PLC plc { get; }
        public SG004AProtocol SG004 {  get; }


        public DeviceManager(PortMultimeter multimeter, PortGenerator generator, PLC _plc, SG004AProtocol sg004)
        {
            Multimeter = multimeter;
            Generator = generator;
            plc = _plc;
            SG004 = sg004;
        }

        public bool AllDevicesReady => Multimeter.IsOpen && Generator.IsOpen && plc.IsOpen && SG004.IsOpen;
        public bool ReadyForCheckVolt => plc.IsOpen;
        public bool ReadyForSetting4_20 => plc.IsOpen && Multimeter.IsOpen  && SG004.IsOpen;
        public bool ReadyForSettingIEPE => Multimeter.IsOpen && Generator.IsOpen && plc.IsOpen;
        public bool ReadyForSettingRS45 => plc.IsOpen;
    }

}
