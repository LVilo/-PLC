using APM_PLC.Models;
using APM_PLC.Models.DevicesModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;


namespace APM_PLC.ViewModels
{
    public partial class LogerViewModel : ViewModelBase
    {
        private static readonly Lazy<LogerViewModel> _instance = new Lazy<LogerViewModel>(() => new LogerViewModel());

        public static LogerViewModel Instance => _instance.Value;

        [ObservableProperty] private string? _LogText;
        LogModel Log = new LogModel();

        public void Write(string msg)
        {
            LogText += $"{DateTime.Now:HH:mm:ss} {msg}\r\n";
            WriteDebug(msg);
        }
        public void WriteDebug(string msg)
        {
            Debug.WriteLine(msg);
            Console.WriteLine(msg);
            Log.Write(msg);
        }
    }
}
