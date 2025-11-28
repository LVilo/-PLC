using APM_PLC.Models.DevicesModel;
using APM_PLC.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PortsWork;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace APM_PLC.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public DevicesViewModel DevicesViewModel { get; }

        public LogerViewModel LogerViewModel { get; }


        public MainWindowViewModel()
        {
            LogerViewModel = LogerViewModel.Instance;
            DevicesViewModel = new DevicesViewModel();
        }
       
    }
}
