using APM_PLC.Models.Settings;
using APM_PLC.ViewModels;
using Avalonia.Media;
using Microsoft.Win32;
using PortsWork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace APM_PLC.Models.DevicesModel
{
    public class Сontroller : ModbusRTU
    {
        LogerViewModel LogerViewModel { get; } = LogerViewModel.Instance;
        public ISetting settings { get; set; }

        public ushort Model
        {
            get;
            set;
        }

        public ushort access { get; } = 0xABCD;
        public byte address { get; set; } = 10;
        public int TimeSleep { get; set; }
        private ushort TimeErrorSleep = 200;

        private byte[] CreateMessage(byte address, byte funct, byte[] register, byte[] value)
        {
            byte[] result = new byte[8];
            byte[] crc = new byte[2];
            result[0] = address;
            result[1] = funct;
            result[2] = register[1];
            result[3] = register[0];
            result[4] = value[1];
            result[5] = value[0];
            GetCRC(result, ref crc);
            result[6] = crc[0];
            result[7] = crc[1];
            return result;
        }
        protected byte[] CreateMessageMultipleWrite(byte address, byte[] register, byte[] values)
        {
            byte[] result = new byte[8 + 1 + values.Length];
            byte[] tmp;
            result[0] = address;
            result[1] = 0x10;
            result[2] = register[1];
            result[3] = register[0];
            tmp = BitConverter.GetBytes(values.Length / 2);
            result[4] = tmp[1];
            result[5] = tmp[0];
            result[6] = (byte)values.Length;

            for (int i = 0; i < values.Length / 2; i++)
            {
                result[7 + 2 * i] = values[2 * i + 1];
                result[7 + 2 * i + 1] = values[2 * i];
            }

            GetCRC(result, ref tmp);
            result[result.Length - 2] = tmp[0];
            result[result.Length - 1] = tmp[1];
            return result;
        }
        public void WriteSwFloat16(ushort reg, float value)
        {
            LogerViewModel.WriteDebug($"Запись в регистр {reg} значение {value}");
            byte[] b = BitConverter.GetBytes(value); //ABCD
            byte[] floatBytes = new byte[4];
            floatBytes[0] = b[0];//A
            floatBytes[1] = b[1];//B
            floatBytes[2] = b[2];//C
            floatBytes[3] = b[3];//D
            TryWriteFloat(reg, floatBytes, value);

        }

        public float ReadSwFloat16(ushort reg, byte type)
        {
            byte[] floatBytes = new byte[4];
            for (int i = 0; i < 10; i++)
            {
                byte[]? b = Read(reg, type, 2);
                if (b != null)
                {
                    floatBytes[0] = b[4];
                    floatBytes[1] = b[3];
                    floatBytes[2] = b[6];
                    floatBytes[3] = b[5];
                    float result = BitConverter.ToSingle(floatBytes);
                    if ((Math.Abs(result) <= 1E-7 || Math.Abs(result) >= 1E+10) && result != 0)
                    {
                        Thread.Sleep(TimeErrorSleep);
                        continue;
                    }
                    LogerViewModel.WriteDebug($"Прочтено с регистра {reg} значение {result}");
                    return result;
                }
                Thread.Sleep(TimeErrorSleep);
            }
            return 0;
        }
        public void WriteFloat16(ushort reg, float value)
        {
            LogerViewModel.WriteDebug($"Запись в регистр {reg} значение {value}");
            byte[] b = BitConverter.GetBytes(value); //ABCD
            byte[] floatBytes = new byte[4];
            floatBytes[0] = b[2];//C
            floatBytes[1] = b[3];//D
            floatBytes[2] = b[0];//A
            floatBytes[3] = b[1];//B
            TryWriteFloat(reg, floatBytes, value);
        }
        public float ReadFloat16(ushort reg, byte type)
        {
            byte[] floatBytes = new byte[4];
            for (int i = 0; i < 10; i++)
            {
                byte[]? b = Read(reg, type, 2);
                if (b != null)
                {
                    floatBytes[0] = b[6];
                    floatBytes[1] = b[5];
                    floatBytes[2] = b[4];
                    floatBytes[3] = b[3];
                    float result = BitConverter.ToSingle(floatBytes);
                    if ((Math.Abs(result) <= 1E-7 || Math.Abs(result) >= 1E+10) && result != 0)
                    {
                        Thread.Sleep(TimeErrorSleep);
                        continue;
                    }
                    LogerViewModel.WriteDebug($"Прочтено с регистра {reg} значение {result}");
                    return result;
                }
                Thread.Sleep(TimeErrorSleep);
            }
            return 0;
        }
        private void TryWriteFloat(ushort reg, byte[] floatvalue, float value)
        {
            for (int i = 0; i < 10; i++)
            {
                WriteMultiple(reg, floatvalue);
                Thread.Sleep(TimeErrorSleep);
                if (ReadFloat16(reg, 0x03) == value) return;
                if (ReadSwFloat16(reg, 0x03) == value) return;
            }
            throw new Exception("Не получилось записать. Проверьте подключение");
        }
        public void WriteOneUint16(ushort reg, ushort value)
        {
            LogerViewModel.WriteDebug($"Запись в регистр {reg} значение {value}");
            byte[] b = BitConverter.GetBytes(value);
            Write(reg, b);
            Thread.Sleep(1000);
            ReadUint16(reg, 0x03);
        }
        private void TryWriteSingle(ushort reg, byte[] b, ushort value)
        {
            for (int i = 0; i < 10; i++)
            {
                Write(reg, b);
                Thread.Sleep(TimeErrorSleep);
                ushort u = ReadUint16(reg, 0x03);
                if (u == value) return;
            }
            throw new Exception("Не получилось записать. Проверьте подключение");
        }
        private void TryWriteSingle(ushort reg, byte[] b, short value)
        {
            for (int i = 0; i < 10; i++)
            {
                Write(reg, b);
                Thread.Sleep(TimeErrorSleep);
                short u = ReadInt16(reg, 0x03);
                if (u == value) return;
            }
            throw new Exception("Не получилось записать. Проверьте подключение");
        }
        public void WriteInt16(ushort reg, short value)
        {
            try
            {
                LogerViewModel.WriteDebug($"Запись в регистр {reg} значение {value}");
                byte[] b = BitConverter.GetBytes(value);

                TryWriteSingle(reg, b, value);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public short ReadInt16(ushort reg, byte type)
        {
            short result = 0;
            for (int i = 0; i < 10; i++)
            {
                byte[]? b = Read(reg, type, 1);
                if (b != null)
                {
                    result = (short)(b[3] << 8 | b[4]);
                    if ((Math.Abs(result) <= 1E-7 || Math.Abs(result) >= 1E+10) && result != 0)
                    {
                        Thread.Sleep(TimeErrorSleep);
                        continue;
                    }
                    LogerViewModel.WriteDebug($"Прочтено с регистра {reg} значение {result}");
                    return result;
                }
                Thread.Sleep(TimeErrorSleep);
            }
            throw new Exception("Не удалось прочитать. Проверьте подключение");

        }
        public void WriteUint16(ushort reg, ushort value)
        {
            try
            {
                LogerViewModel.WriteDebug($"Запись в регистр {reg} значение {value}");
                byte[] b = BitConverter.GetBytes(value);

                TryWriteSingle(reg, b, value);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public ushort ReadUint16(ushort reg, byte type)
        {
            ushort result = 0;
            for (int i = 0; i < 10; i++)
            {
                byte[]? b = Read(reg, type, 1);
                if (b != null)
                {
                    result = (ushort)(b[3] << 8 | b[4]);
                    if ((Math.Abs(result) <= 1E-7 || Math.Abs(result) >= 1E+10) && result != 0)
                    {
                        Thread.Sleep(TimeErrorSleep);
                        continue;
                    }
                    LogerViewModel.WriteDebug($"Прочтено с регистра {reg} значение {result}");
                    return result;
                }
                Thread.Sleep(TimeErrorSleep);
            }
            LogerViewModel.Write("Не удалось прочитать. Проверьте подключение");
            return result;
        }
        public bool WriteMultiple(ushort reg, byte[] value)
        {
            byte[] register = BitConverter.GetBytes(reg - 1);
            byte[] message = CreateMessageMultipleWrite(address, register, value);
            byte[] response = new byte[8];
            int bytes = 0;
            try
            {
                if (!WaitPortAnswer(message, 8, TimeSleep, 25, out bytes))
                {
                    Console.WriteLine("Ошибка 3 чтения из порта " + PortName);
                    return false;
                }
                ReadPortAnswer(bytes, ref response);
            }
            catch
            {
                Console.WriteLine("Ошибка 6 чтения из порта " + PortName);
                return false;
            }
            return true;
        }
        public bool Write(ushort reg, byte[] value)
        {
            byte[] register = BitConverter.GetBytes(reg - 1);
            byte[] message = CreateMessage(address, 0x06, register, value);
            byte[] response = new byte[8];
            int bytes = 0;

            if (!WaitPortAnswer(message, 8, TimeSleep, 1, out bytes))
            {
                Console.WriteLine("Ошибка 3 чтения из порта " + PortName);
                return false;
            }
            ReadPortAnswer(bytes, ref response);
            //for (int i = 0; i < bytes; i++)
            //{
            //    if (message[i] == response[i])
            //        continue;
            //    Sleep(5000);
            //}
            return true;
        }

        public byte[]? Read(ushort reg, byte type, ushort len)
        {
            int bytes = 0;
            byte[] bytevalue = BitConverter.GetBytes(len);
            byte[] register = BitConverter.GetBytes(reg - 1);
            byte[] message = CreateMessage(address, type, register, bytevalue);
            ushort answerLen = CountAnswerLength(type, len);
            byte[] response = new byte[5 + answerLen];
            try
            {
                if (!WaitPortAnswer(message, response.Length, TimeSleep, 25, out bytes))
                {
                    Console.WriteLine("Ошибка 1 чтения из порта " + PortName);
                    return null;
                }
                ReadPortAnswer(bytes, ref response);

            }
            catch
            {
                Console.WriteLine("Ошибка 2 чтения из порта " + PortName);
                return null;
            }

            return response;
        }
        public virtual bool Setting()
        {
            return false;
        }
       
    }
}
