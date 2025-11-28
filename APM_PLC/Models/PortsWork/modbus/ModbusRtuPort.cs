using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;

namespace PortsWork
{
    /// <summary>
    /// Класс для работы с Modbus RTU, унаследованный от Port
    /// </summary>
    public class ModbusRtuPort : Port
    {
        public byte SlaveId { get; set; } = 10;

        public ModbusRtuPort() : base()
        {
            BaudRate = 115200;
            DataBits = 8;
            Parity = Parity.Even;
            StopBits = StopBits.One;
        }
        public void WriteSwFloat(ushort reg, float f)
        {
            byte[] b = BitConverter.GetBytes(f);
            ushort hi = BitConverter.ToUInt16(b, 2);
            ushort lo = BitConverter.ToUInt16(b, 0);
            WriteHolding(reg, hi);
            WriteHolding((ushort)(reg + 1), lo);
        }
        public float ReadSwFloat(ushort reg)
        {
           ushort[] value = ReadHolding(reg, 2);
            return ConvertModBus.ToFloat(value);
        }
        /// <summary>
        /// Формирование запроса Modbus RTU
        /// </summary>
        private byte[] BuildRequest(byte slave, byte function, ushort startAddr, ushort count)
        {
            byte[] frame = new byte[8];
            frame[0] = slave;
            frame[1] = function;
            frame[2] = (byte)(startAddr >> 8);
            frame[3] = (byte)(startAddr & 0xFF);
            frame[4] = (byte)(count >> 8);
            frame[5] = (byte)(count & 0xFF);

            ushort crc = Crc16(frame, 6);
            frame[6] = (byte)(crc & 0xFF);
            frame[7] = (byte)(crc >> 8);

            return frame;
        }

        /// <summary>
        /// Подсчёт CRC16 (Modbus)
        /// </summary>
        protected static ushort Crc16(byte[] buf, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= buf[i];
                for (int j = 0; j < 8; j++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;
                    if (lsb)
                        crc ^= 0xA001;
                }
            }
            return crc;
        }

        /// <summary>
        /// Чтение holding-регистров (функция 03)
        /// </summary>
        public ushort[] ReadHolding(ushort startAddr, ushort count)
        {
            byte[] req = BuildRequest(SlaveId, 0x03, startAddr, count);

            Write(req, 0, req.Length);

            if (!WaitPortAnswer())
                throw new TimeoutException("Modbus: устройство не отвечает");

            Thread.Sleep(50);

            int bytes = BytesToRead;
            if (bytes < 5)
                throw new Exception("Modbus: слишком короткий ответ");

            byte[] resp = new byte[bytes];
            Read(resp, 0, bytes);

            if (resp[0] != SlaveId || resp[1] != 0x03)
                throw new Exception("Modbus: неверный ответ устройства");

            byte byteCount = resp[2];
            if (byteCount != count * 2)
                throw new Exception("Modbus: длина данных не совпадает");

            ushort respCrc = (ushort)(resp[bytes - 2] | (resp[bytes - 1] << 8));
            ushort calcCrc = Crc16(resp, bytes - 2);

            if (respCrc != calcCrc)
                throw new Exception("Modbus: ошибка CRC");

            ushort[] result = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = (ushort)((resp[3 + i * 2] << 8) | resp[4 + i * 2]);
            }

            return result;
        }
        /// <summary>
        /// Пишет message и ждёт ответа длиной >= answerLength.
        /// Возвращает true и out bytes фактически прочитанные (ограниченные answerLength).
        /// </summary>
        private bool WaitPortAnswer(byte[] message, int answerLength, int sleepTime, int attempts, out int bytes)
        {
            bytes = 0;

            if (message == null) throw new ArgumentNullException(nameof(message));
            if (answerLength <= 0) throw new ArgumentOutOfRangeException(nameof(answerLength));
            if (attempts <= 0) attempts = 1;
            if (sleepTime < 0) sleepTime = 50;

            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    // Очистим вход/выход — важно, чтобы не было "хвостов" от предыдущих посылок
                    try { DiscardOutBuffer(); } catch { /* ignore */ }
                    try { DiscardInBuffer(); } catch { /* ignore */ }

                    Write(message, 0, message.Length);

                    Thread.Sleep(sleepTime);

                    int available = BytesToRead;
                    if (available >= answerLength)
                    {
                        bytes = answerLength;
                        return true;
                    }

                    // если пришло что-то, но меньше требуемого — можно подождать чуть дольше
                    // (дополнительная короткая пауза)
                    if (available > 0)
                    {
                        Thread.Sleep(10);
                        available = BytesToRead;
                        if (available >= answerLength)
                        {
                            bytes = answerLength;
                            return true;
                        }
                    }
                }
                catch
                {
                    // в случае проблем с портом — пробуем повторить попытку
                }
            }

            return false;
        }
        /// <summary>
        /// Чтение input-регистров (функция 04)
        /// </summary>
        public ushort[] ReadInput(ushort startAddr, ushort count)
        {
            byte[] req = BuildRequest(SlaveId, 0x04, startAddr, count);
            Write(req, 0, req.Length);


            if (!WaitPortAnswer())
                throw new TimeoutException("Modbus: устройство не отвечает");


            Thread.Sleep(50);


            int bytes = BytesToRead;
            if (bytes < 5)
                throw new Exception("Modbus: слишком короткий ответ");


            byte[] resp = new byte[bytes];
            Read(resp, 0, bytes);


            if (resp[0] != SlaveId || resp[1] != 0x04)
                throw new Exception("Modbus: неверный ответ устройства");


            byte byteCount = resp[2];
            if (byteCount != count * 2)
                throw new Exception("Modbus: длина данных не совпадает");


            ushort respCrc = (ushort)(resp[bytes - 2] | (resp[bytes - 1] << 8));
            ushort calcCrc = Crc16(resp, bytes - 2);


            if (respCrc != calcCrc)
                throw new Exception("Modbus: ошибка CRC");


            ushort[] result = new ushort[count];
            for (int i = 0; i < count; i++)
                result[i] = (ushort)((resp[3 + i * 2] << 8) | resp[4 + i * 2]);


            return result;
        }
        /// <summary>
        /// Запись одного holding-регистра (функция 06)
        /// </summary>
        public void WriteHolding(ushort address, ushort value)
        {
            byte[] frame = new byte[8];
            frame[0] = SlaveId;
            frame[1] = 0x06;
            frame[2] = (byte)(address >> 8);
            frame[3] = (byte)(address & 0xFF);
            frame[4] = (byte)(value >> 8);
            frame[5] = (byte)(value & 0xFF);

            ushort crc = Crc16(frame, 6);
            frame[6] = (byte)(crc & 0xFF);
            frame[7] = (byte)(crc >> 8);


            //Write(frame, 0, frame.Length);
            int b = 0;
            if (!WaitPortAnswer(frame, frame.Length,2000,5,out b))
                throw new TimeoutException("Modbus: устройство не отвечает при записи");

            Thread.Sleep(40);


            byte[] resp = new byte[BytesToRead];
            Read(resp, 0, resp.Length);


            if (resp.Length < 8)
                throw new Exception("Modbus: слишком короткий ответ при записи");


            ushort respCrc = (ushort)(resp[resp.Length - 2] | (resp[resp.Length - 1] << 8));
            ushort calcCrc = Crc16(resp, resp.Length - 2);


            if (respCrc != calcCrc)
                throw new Exception("Modbus: ошибка CRC при записи");


            if (resp[0] != SlaveId || resp[1] != 0x06)
                throw new Exception("Modbus: неверный ответ устройства при записи");
        }
    }
}
