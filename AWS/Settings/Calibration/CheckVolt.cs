using Avalonia.Threading;
using AWS.ViewModels;
using AWS.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.Settings.Calibration
{
    public class CheckVolt : ICalibrationRoutine
    {
        private readonly CalibrationContext _context;

        public CheckVolt(CalibrationContext context)
        {
            _context = context;
        }
        public async Task<bool> RunAsync()
        {

        }
    }
}
