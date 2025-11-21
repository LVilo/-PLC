using Avalonia.Controls;
using Avalonia.Threading;
using AWS.Devices;
using AWS.Views;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Settings.Setting_4_20
{
    public class Setting_4_20_With_SG004 : ISettting_4_20
    {
        DevicesCommunication devices = DevicesCommunication.Instance;
        public string ImageSettingOutput { get; } = "AWS.Images.4_20Input.png";
        public string ImageSettingInput { get; } = "AWS.Images.4_20Output.png";
        public async Task<bool> SetCurrent(float f, Window owner)
        {
            devices.sg004.WriteOutputCurrent(f);
            return true;
        }
        public void SetOutputSwtich(bool swtich) => devices.sg004.WriteOutputSwitch(swtich);
        public float ReadCurrent()
        {
            return devices.sg004.ReadInputCurrent();
        }
    }
}
