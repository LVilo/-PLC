using Avalonia.Media;
using APM_PLC.Models.DevicesModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PortsWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.ViewModels.DevicesViewModels
{
    public abstract partial class DevicesContext : ViewModelBase
    {

        [ObservableProperty]
        protected string _selectedColor = "#FFD3D3D3"; // СЕРЫЙ

        [ObservableProperty]
        protected string? _PortItem;

        public abstract Task<bool?> OpenPort();
        public abstract Task ClosePort();
    }
}
