using System.Threading.Tasks;

namespace AWS.Settings.Calibration
{
    internal interface ICalibrationRoutine
    {
        Task<bool> RunAsync();
    }
}
