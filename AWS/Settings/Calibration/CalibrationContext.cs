using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Settings.Calibration
{
    public class CalibrationContext
    {
        public DeviceManager Devices { get; }

        public CalibrationContext(DeviceManager devices)
        {
            Devices = devices;
        }
    }

}
