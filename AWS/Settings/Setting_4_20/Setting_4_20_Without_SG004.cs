using Avalonia.Controls;
using Avalonia.Threading;
using AWS.Devices;
using AWS.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Settings.Setting_4_20
{
    public class Setting_4_20_Without_SG004 : ISettting_4_20
    {
        DevicesCommunication devices = DevicesCommunication.Instance;
       public string ImageSettingOutput { get; } = "AWS.Images.4_20OutputWithoutSG004.png";
       public string ImageSettingInput { get; } = "AWS.Images.4_20InputWithoutSG004.png";
        private async Task<bool> ShowConfirmationDialogAsync(string message,Window owner)
        {
            bool result = await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                devices.DC_Read = true;
                var dialog = new Dialoginfo();
                dialog.Label_Text.Text = message;
                await dialog.ShowDialog(owner);
                devices.DC_Read = false;
                if (dialog.Dialog_Cancel == true) throw new Exception(devices.info[220]);
                return dialog.Dialog_result;
            });
            return result;
        }
        public async Task<bool> SetCurrent(float f,Window owner)
        {
            if (await ShowConfirmationDialogAsync($"Задайте сопротивление, чтобы на вольтметра было {f / 10} В", owner ) is true)
            {
                return true;
            }
            else return false;
        }
        public void SetOutputSwtich(bool swtich)
        {

        }
        public float ReadCurrent()
        {
            return Convert.ToSingle(devices.multimeter.GetVoltage("DC",200)) * 10f;
        }
    }
}
