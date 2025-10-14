using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortsWork;
namespace AWS.Settings
{
    public class DeviceManager
    {
        public PortMultimeter Multimeter { get; }
        public PortGenerator Generator { get; }
        public ModbusRTU PLC { get; }
        public SG004AProtocol SG004 {  get; }


        public DeviceManager(PortMultimeter multimeter, PortGenerator generator, ModbusRTU plc, SG004AProtocol sg004)
        {
            Multimeter = multimeter;
            Generator = generator;
            PLC = plc;
            SG004 = sg004;
        }

        public bool AllDevicesReady => Multimeter.IsOpen && Generator.IsOpen && PLC.IsOpen && SG004.IsOpen;
        public bool ReadyForCheckVolt => PLC.IsOpen;
        public bool ReadyForSetting4_20 => PLC.IsOpen && Multimeter.IsOpen  && SG004.IsOpen;
        public bool ReadyForSettingIEPE => Multimeter.IsOpen && Generator.IsOpen && PLC.IsOpen;
        public bool ReadyForSettingRS45 => PLC.IsOpen;
    }

}
