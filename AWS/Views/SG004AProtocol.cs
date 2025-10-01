using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;


public class SG004AProtocol
{
    private readonly SerialPort _port;
    public byte slaveAddr { get; set; }
    public int delay { get; set; }
    public bool IsOpen { get { return _port.IsOpen; } }

    public string PortName
    {
        get => _port.PortName;
        set => _port.PortName = value;
    }

    private const byte FUNC_READ_UINT16 = 0x64;  // 100
    private const byte FUNC_WRITE_UINT16 = 0x65; // 101
    private const byte FUNC_READ_FLOAT = 0x66;   // 102
    private const byte FUNC_WRITE_FLOAT = 0x67;  // 103

    public const ushort REG_FIRMWARE_VERSION = 40001;
    public const ushort REG_INPUT_SIGNAL = 40002;
    public const ushort REG_OUTPUT_SIGNAL = 40003;
    public const ushort REG_INPUT_VALUE = 40004;
    public const ushort REG_OUTPUT_VALUE = 40006;
    public const ushort REG_OUTPUT_SWITCH = 40008;
    // Signal types, sensors, modes...

    public const byte SIGNAL_TYPE_CURRENT = 0x01;
    public const byte SIGNAL_TYPE_VOLTAGE = 0x02;
    public const byte SIGNAL_TYPE_FREQUENCY = 0x04;
    public const byte SIGNAL_TYPE_MILLIVOLT = 0x05;
    public const byte SIGNAL_TYPE_RESISTANCE = 0x06;

    public const byte SENSOR_NONE = 0x0;
    public const byte SENSOR_TYPE_S = 0x1;
    public const byte SENSOR_TYPE_B = 0x2;
    public const byte SENSOR_TYPE_E = 0x3;
    public const byte SENSOR_TYPE_K = 0x4;
    public const byte SENSOR_TYPE_R = 0x5;
    public const byte SENSOR_TYPE_J = 0x6;
    public const byte SENSOR_TYPE_T = 0x7;
    public const byte SENSOR_TYPE_N = 0x8;

    public const byte MODE_MV = 0x1;
    public const byte MODE_THERMOCOUPLE = 0x2;
    public const byte MODE_WR_THERMOCOUPLE = 0x3;

    public const byte OUTPUT_OFF = 0;
    public const byte OUTPUT_ON = 1;
    public SG004AProtocol()
    {
        _port = new SerialPort()
        {
            ReadTimeout = 1000,
            WriteTimeout = 1000,
            BaudRate = 9600,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
        };

    }
    public bool Open()
    {
        try
        {
            _port.Open();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Порт не найден ");
            return false;
        }
        return IsOpen;
    }
    // CRC16 Modbus
    private static ushort ComputeCRC(byte[] data, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = 0; i < length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
        }
        return crc;
    }
    private byte[] BuildReadPacket(byte slaveAddr, byte function, ushort regAddr, ushort count)
    {
        byte[] buffer = new byte[6];
        buffer[0] = slaveAddr;
        buffer[1] = function;
        buffer[2] = (byte)(regAddr >> 8);
        buffer[3] = (byte)(regAddr & 0xFF);
        buffer[4] = (byte)(count >> 8);
        buffer[5] = (byte)(count & 0xFF);

        ushort crc = ComputeCRC(buffer, buffer.Length);
        return buffer.Concat(new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) }).ToArray();
    }

    private ushort ReadUint16(ushort regAddr)
    {
        byte[] packet = BuildReadPacket(slaveAddr, 0x64, regAddr, 1);
        _port.Write(packet, 0, packet.Length);

        byte[] resp = new byte[7]; // slave + func + count + data(2) + CRC(2)
        _port.Read(resp, 0, resp.Length);
        Thread.Sleep(delay);
        return (ushort)((resp[3] << 8) | resp[4]);
    }

    private float ReadFloat(ushort regAddr)
    {
        byte[] packet = BuildReadPacket(slaveAddr, 0x66, regAddr, 2);
        _port.Write(packet, 0, packet.Length);

        byte[] resp = new byte[9]; // slave + func + count + data(4) + CRC(2)
        _port.Read(resp, 0, resp.Length);
        Thread.Sleep(delay);
        _port.Read(resp, 0, resp.Length);
        byte[] floatBytes = { resp[3], resp[4], resp[5], resp[6] };

        if (BitConverter.IsLittleEndian)
            Array.Reverse(floatBytes);

        return BitConverter.ToSingle(floatBytes, 0) / 1000;
    }
    private void WriteUint16(ushort regAddr, ushort value)
    {
        byte[] payload = { (byte)(value >> 8), (byte)(value & 0xFF) };
        byte[] buffer = new byte[4 + payload.Length];
        buffer[0] = slaveAddr;
        buffer[1] = 0x65;
        buffer[2] = (byte)(regAddr >> 8);
        buffer[3] = (byte)(regAddr & 0xFF);
        Array.Copy(payload, 0, buffer, 4, payload.Length);

        ushort crc = ComputeCRC(buffer, buffer.Length);
        byte[] packet = buffer.Concat(new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) }).ToArray();

        _port.Write(packet, 0, packet.Length);
        Thread.Sleep(delay);
    }
    private void WriteFloat(ushort register, float value)
    {
        // --- 1. Разбиваем float на 2 регистра ---
        byte[] floatBytes = BitConverter.GetBytes(value * 1000);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(floatBytes); // Big-endian для Modbus

        ushort high = (ushort)((floatBytes[0] << 8) | floatBytes[1]);
        ushort low = (ushort)((floatBytes[2] << 8) | floatBytes[3]);

        // --- 2. Формируем payload ---
        ushort[] values = new ushort[] { high, low };
        byte[] payload = new byte[5 + values.Length * 2];
        payload[0] = (byte)(register >> 8);
        payload[1] = (byte)(register & 0xFF);
        payload[2] = (byte)(values.Length >> 8);
        payload[3] = (byte)(values.Length & 0xFF);
        payload[4] = (byte)(values.Length * 2);

        for (int i = 0; i < values.Length; i++)
        {
            payload[5 + i * 2] = (byte)(values[i] >> 8);
            payload[6 + i * 2] = (byte)(values[i] & 0xFF);
        }

        // --- 3. Формируем команду (slave + func + payload) ---
        byte functionCode = 0x67; // твой кастомный код
        byte[] command = new byte[2 + payload.Length];
        command[0] = slaveAddr;
        command[1] = functionCode;
        Array.Copy(payload, 0, command, 2, payload.Length);

        // --- 4. CRC ---
        ushort crc = ComputeCRC(command, command.Length);
        byte[] commandWithCrc = command.Concat(new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) }).ToArray();

        // --- 5. Отправка ---
        _port.Write(commandWithCrc, 0, commandWithCrc.Length);
        Thread.Sleep(delay);
        // ⚠️ Если нужно — можно раскомментировать обработку ответа
        // byte[] response = new byte[8];
        // int read = _port.Read(response, 0, response.Length);
        // if (read < 8) throw new Exception("Response too short");
    }
    public void WriteOutputSwitch(bool enabled)
    {
        ushort sv = (ushort)((enabled ? 1 : 0) + 0x0101);
        WriteUint16(REG_OUTPUT_SWITCH, sv);
    }
    public void WriteOutputCurrent(float value)
    {
        WriteFloat(REG_OUTPUT_VALUE, value);
    }
    public void ChangeOutputSignal(ushort signal)
    {
        WriteUint16(REG_OUTPUT_SIGNAL, signal);
    }
    public float ReadInputCurrent()
    {
        ChangeInputSignal(0x0101);
        return ReadFloat(REG_INPUT_VALUE);
    }

    public void ChangeInputSignal(ushort signal)
    {
        WriteUint16(REG_INPUT_SIGNAL, signal);
    }
    public void Close() => _port.Close();
}