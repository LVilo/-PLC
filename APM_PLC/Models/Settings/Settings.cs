using APM_PLC.Models.DevicesModel;
using APM_PLC.ViewModels;
using APM_PLC.ViewModels.DialogViewModels;
using APM_PLC.Views;
using APM_PLC.Views.DialogViews;
using PortsWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models.Settings
{
    public interface IGetVoltege
    {
        Task GetVolt();
    }
    public class DC :IGetVoltege
    {
        public async Task GetVolt()
        {
            Devices.Instance.multimeter.GetVoltage("DC", 100);
            await Task.Delay(500);
        }
    }
    public class AC : IGetVoltege
    {
        public async Task GetVolt()
        {
            Devices.Instance.multimeter.GetVoltage("AC", 100);
            await Task.Delay(500);
        }
    }
    public class Delay : IGetVoltege
    {
        public async Task GetVolt() => await Task.Delay(2000);
    }

    public static class Settings
    {

        public static IGetVoltege Mult = new Delay();

        public static async Task WhileGetVoltAsync()
        {
            try
            {
                while (true)
                {
                    await Mult.GetVolt();
                }
            }
            catch
            {

            }
        }
        public static async Task SetVoltage( double value, double minvalue, double maxvalue)
        {
            double expectedvalue = minvalue + ((maxvalue - minvalue) / 2);
            Devices.Instance.generator.SetFrequency(79.6);
            Devices.Instance.generator.ChangeSignalType(PortGenerator.SignalType.Sine);
            Devices.Instance.generator.SetVoltage(value);
            double target = value;
            Devices.Instance.multimeter.VoltmeterMode(PortMultimeter.SIGNALTYPE_AC);
            await Task.Delay(500);

            double d = Devices.Instance.multimeter.GetVoltage("AC", 200);
            if (d < minvalue || d > maxvalue)
            {
                value = value * (expectedvalue / d);
                Devices.Instance.generator.SetVoltage(value);
            }
        }
        public static async Task SetOffset( double value, double minvalue, double maxvalue)
        {
            double expectedvalue = minvalue + ((maxvalue - minvalue) / 2);
            if(value >0 && value <4) Devices.Instance.generator.SetOffset(4);
            if (value < 0 && value > -4) Devices.Instance.generator.SetOffset(-4);
            await Task.Delay(1000);
            Devices.Instance.multimeter.GetVoltage("DC", 500);
            Devices.Instance.generator.SetOffset(value);
            double target = value;
            Devices.Instance.multimeter.VoltmeterMode(PortMultimeter.SIGNALTYPE_DC);
            await Task.Delay(500);

            double d = Devices.Instance.multimeter.GetVoltage("DC", 500);
            if (d < minvalue || d > maxvalue)
            {
                value = value * (expectedvalue / d);
                Devices.Instance.generator.SetOffset(value);
            }
        }
        public static async Task SetValueReg( double value, double minvalue, double maxvalue, ushort reg)
        {
            double expectedvalue = minvalue + ((maxvalue - minvalue) / 2);
            Devices.Instance.generator.SetFrequency(79.6);
            Devices.Instance.generator.ChangeSignalType(PortGenerator.SignalType.Sine);
            Devices.Instance.generator.SetVoltage(value);
            double target = value;
            await Task.Delay(500);
            double d = Devices.Instance.controller.ReadSwFloat16(reg, 0x03);
            if (d < minvalue || d > maxvalue)
            {
                value = value * (expectedvalue / d);
                Devices.Instance.generator.SetVoltage(value);
            }
        }
        public static async Task ShowDialogBuild(BuildSchemeViewModel build, string setting)
        {
            build.SetBitmap($"avares://APM_CNV/Assets/{setting}.png");
            build.Show();
            await build.WaitAsync();
            if (build.Confirmed is false) throw new Exception("Отмена");
        }
        public static async Task ShowDialog(ConfirmDialogViewModel dialog, string text, bool YesOrNot,IGetVoltege type)
        {
            if (YesOrNot)
            {
                dialog.ConfirmText = "Да";
                dialog.CancelText = "Нет";
            }
            else
            {
                dialog.ConfirmText = "Ок";
                dialog.CancelText = "Отмена";
            }
            Mult = type;
            dialog.Messege = text;
            dialog.Show();
            await dialog.WaitAsync();
            Mult = new Delay();
            if (dialog.Confirmed is false) throw new Exception("Отмена");
        }
        public static async Task Wait(int delay)
        {
            LogerViewModel.Instance.Write($"Подождите {delay / 1000} секунд");
            await Task.Delay(delay);
        }
        public static async Task CheckInputSignal(ConfirmDialogViewModel CurrentDialog)
        {
            float signal = Devices.Instance.controller.ReadSwFloat16(1005, 0x04);
            if (signal <= 0)
            {
                await Settings.ShowDialog(CurrentDialog, $"Обратите внимание значение регистра 1005 равно {signal}, Продолжить ?", true, new Delay());
            }
        }
    }
}
