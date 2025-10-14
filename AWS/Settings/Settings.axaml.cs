using Avalonia.Controls;
using Avalonia.Threading;
using AWS.Settings.Calibration;
using AWS.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Settings
{
    public static class Setting
    {
        public async void Do_Work(int code)
        {
            DeviceManager devices = new DeviceManager();
            CalibrationContext context = new CalibrationContext(devices);
            CheckVolt checkvolt = new CheckVolt(context);
            CheckVolt.RunAsync();
        }
    }
}
