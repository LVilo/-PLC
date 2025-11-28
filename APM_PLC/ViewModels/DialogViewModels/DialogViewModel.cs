using APM_PLC.Models.DevicesModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.ViewModels.DialogViewModels
{
    public partial class DialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isDialogOpen = false;

        protected TaskCompletionSource closeTask = new TaskCompletionSource();

        public async Task WaitAsync()
        {
            await closeTask.Task;
        }

        public void Show()
        {
            if(closeTask.Task.IsCompleted) closeTask = new TaskCompletionSource();
            IsDialogOpen = true;
        }

        public void Close()
        {
            IsDialogOpen = false;
            closeTask.TrySetResult();
        }
    }
}
