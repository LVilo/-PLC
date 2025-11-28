using APM_PLC.Models.Settings;
using APM_PLC.ViewModels;
using PortsWork;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models.DevicesModel
{
    public sealed class Devices : Port
    {

        private static readonly Lazy<Devices> _instance = new Lazy<Devices>(() => new Devices());
        public static Devices Instance => _instance.Value;
        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;

        private List<VisaDeviceInformation> usbDevicesInfo;


        public PortGenerator generator = new PortGenerator();
        public PortMultimeter multimeter = new PortMultimeter();
        public Сontroller controller = new Сontroller();
        public SG004AProtocol sg004 = new SG004AProtocol();

        public string[]? Ports { get; set; }
        public string? PortItem { get; set; }
        public string? PortItemGen { get; set; }
        public string? PortItemAgil { get; set; }
        public string? PortItemCNV { get; set; }


        public async Task<bool?> OpenPort(Port port)
        {
            return await Task.Run<bool?>(() =>
            {
                if (port.IsOpen) return null;
                return port.OpenPort();
            });

        }
        public void ClosePort(Port device) => device.ClosePort();
        public Port SetMeasureDeviceName(Port device, string name)
        {

            if (name.Contains("COM") || name.Contains("/dev/ttyUSB") || name.Contains("/dev/usbtmc"))
            {
                device.SetName(name);
            }
            else
            {
                VisaDeviceInformation info = usbDevicesInfo.Find(t => name.Contains(t.devType));
                device.usbInfo = info;
                device.SetName(info.description);
            }
            return device.IdentifyDeviceType();
        }
        public string[] GetAllPorts()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                usbDevicesInfo = Port.FindVisaDevicesInfo();
                List<string> usbInfo = new List<string>();
                usbDevicesInfo.ForEach(t => usbInfo.Add(t.GetInfo()));
                return usbInfo.Concat(SerialPort.GetPortNames()).ToArray();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Linux.FindDevicesLinux().ToArray();
            }
            else
            {
                throw new PlatformNotSupportedException("Неподдерживаемая ОС");
            }
        }
        //public void SetSetting(string model)
        //{
        //    switch (model)
        //    {
        //        case "CNV1171": controller.settings = new SettingCNV117(); break;
        //        case "CNV1176": controller.settings = new SettingCNV117(); break;
        //        case "CNV127": controller.settings = new SettingCNV127(); break;
        //        case "CNV1371": controller.settings = new SettingCNV137(); break;
        //        case "CNV1376": controller.settings = new SettingCNV137(); break;
        //        case "CNV1471": controller.settings = new SettingCNV147(); break;
        //        case "CNV1476": controller.settings = new SettingCNV147(); break;
        //        case "CNV1571": controller.settings = new SettingCNV157(); break;
        //        case "CNV1576": controller.settings = new SettingCNV157(); break;
        //    }
        //    devices.cnv.settings.SetType(model);
        //}
        //public async Task<ISetting> IdentifySetting()
        //{
        //    try
        //    {
        //        Model = await Task.Run(() => ReadUint16(2171, 0x03));
        //        ISetting setting = Model switch
        //        {
        //            10 => new SettingCNV117(),
        //            20 => new SettingCNV127(),
        //            21 => new SettingCNV127(),
        //            30 => new SettingCNV137(),
        //            40 => new SettingCNV147(),
        //            50 => new SettingCNV157(),
        //            _ => new SettingsALL()
        //        };
        //        if (setting is SettingsALL)
        //        {
        //            LogerViewModel.Write("Тип CNV не определен. Выберите нужный тип из предложенного");
        //        }
        //        return setting;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogerViewModel.Write(ex.Message);
        //        LogerViewModel.Write("Тип CNV не определен. Выберите нужный тип из предложенного");
        //        return new SettingsALL();
        //    }
        //}
    }
}
