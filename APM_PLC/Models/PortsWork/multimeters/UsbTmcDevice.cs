using PortsWork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

 public class UsbTmcDevice : PortMultimeter
{
    private FileStream _stream;
    private readonly object _lock = new object(); // для потокобезопасности

    public UsbTmcDevice()
    {
    }
    public override string GetName()
    {
        return PortName;
    }
    public override bool SetName(string name)
    {

        if (string.IsNullOrEmpty(name))
            return false;

        PortName = name;
        return true;
    }
    public override bool OpenPort()
    {
        Console.WriteLine(PortName);
        Linux.Acsessusb(PortName);
        if (!File.Exists(PortName))
        {
            Console.WriteLine($"Устройство {PortName} не найдено.");
            return false;
        }

        try
        {
            _stream = new FileStream(PortName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            Console.WriteLine($"Открыто устройство {PortName}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при открытии {PortName}: {ex.Message}");
            return false;
        }
    }
    protected override void WriteMessage(string message)
    {
        if (_stream == null)
            throw new InvalidOperationException("Устройство не открыто.");

        byte[] buffer = Encoding.ASCII.GetBytes(message + "\n");

        lock (_lock)
        {
            _stream.Write(buffer, 0, buffer.Length);
            _stream.Flush();
        }
    }
    public override string ReadMessage(string message)
    {
        WriteMessage(message);
        WaitPortAnswer(300);
        return ReadLine();
    }
    public override double GetVoltage(string type, int time)
    {
        SetWorkType(DEVICE_VOLTMETER, type, true);
        try
        {
            return ConvertString(ReadMessage(MESSAGE_READ).Replace(".", ","));
        }
        catch (TimeoutException)
        {
            return DOUBLE_FALSEVALUE;
        }
    }
    private new void WaitPortAnswer(int timeoutMs)
    {
        if (_stream == null)
            throw new InvalidOperationException("Устройство не открыто.");

        int waited = 0;
        while (_stream.Length == 0 && waited < timeoutMs)
        {
            Thread.Sleep(10);
            waited += 10;
        }
    }
    private new string ReadLine()
    {
        if (_stream == null)
            throw new InvalidOperationException("Устройство не открыто.");

        byte[] buffer = new byte[4096];
        int bytesRead = _stream.Read(buffer, 0, buffer.Length);

        return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
    }

    public override void ClosePort()
    {
        if (_stream != null)
        {
            WriteRemoteMode(false);
            _stream.Close();
            _stream.Dispose();
            _stream = null;
            Console.WriteLine("Порт закрыт");
        }
    }
    
}
