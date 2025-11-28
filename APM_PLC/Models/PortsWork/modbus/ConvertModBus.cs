using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortsWork
{
    /// <summary>
    /// Extension-методы для конвертации массивов Modbus регистров
    /// </summary>
    public static class ConvertModBus
    {
        /// <summary>
        /// Конвертирует два регистра в 32-битное целое число (Big Endian)
        /// </summary>
        public static int ToInt32(this ushort[] registers, int startIndex = 0)
        {
            if (registers == null || registers.Length < startIndex + 2)
                throw new ArgumentException("Недостаточно регистров для конвертации в Int32");

            return (registers[startIndex] << 16) | registers[startIndex + 1];
        }

        /// <summary>
        /// Конвертирует два регистра в 32-битное целое число без знака (Big Endian)
        /// </summary>
        public static uint ToUInt32(this ushort[] registers, int startIndex = 0)
        {
            if (registers == null || registers.Length < startIndex + 2)
                throw new ArgumentException("Недостаточно регистров для конвертации в UInt32");

            return ((uint)registers[startIndex] << 16) | registers[startIndex + 1];
        }

        /// <summary>
        /// Конвертирует два регистра в число с плавающей точкой (IEEE 754, Big Endian)
        /// </summary>
        public static float ToFloat(this ushort[] registers, int startIndex = 0)
        {
            if (registers == null || registers.Length < startIndex + 2)
                throw new ArgumentException("Недостаточно регистров для конвертации в Float");

            byte[] bytes = new byte[4];
            bytes[0] = (byte)(registers[startIndex] >> 8);
            bytes[1] = (byte)(registers[startIndex] & 0xFF);
            bytes[2] = (byte)(registers[startIndex + 1] >> 8);
            bytes[3] = (byte)(registers[startIndex + 1] & 0xFF);

            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Конвертирует четыре регистра в 64-битное целое число (Big Endian)
        /// </summary>
        public static long ToInt64(this ushort[] registers, int startIndex = 0)
        {
            if (registers == null || registers.Length < startIndex + 4)
                throw new ArgumentException("Недостаточно регистров для конвертации в Int64");

            return ((long)registers[startIndex] << 48) |
                   ((long)registers[startIndex + 1] << 32) |
                   ((long)registers[startIndex + 2] << 16) |
                   registers[startIndex + 3];
        }

        /// <summary>
        /// Конвертирует четыре регистра в число с плавающей точкой двойной точности (IEEE 754, Big Endian)
        /// </summary>
        public static double ToDouble(this ushort[] registers, int startIndex = 0)
        {
            if (registers == null || registers.Length < startIndex + 4)
                throw new ArgumentException("Недостаточно регистров для конвертации в Double");

            byte[] bytes = new byte[8];
            bytes[0] = (byte)(registers[startIndex] >> 8);
            bytes[1] = (byte)(registers[startIndex] & 0xFF);
            bytes[2] = (byte)(registers[startIndex + 1] >> 8);
            bytes[3] = (byte)(registers[startIndex + 1] & 0xFF);
            bytes[4] = (byte)(registers[startIndex + 2] >> 8);
            bytes[5] = (byte)(registers[startIndex + 2] & 0xFF);
            bytes[6] = (byte)(registers[startIndex + 3] >> 8);
            bytes[7] = (byte)(registers[startIndex + 3] & 0xFF);

            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Конвертирует весь массив регистров в массив 32-битных целых чисел
        /// </summary>
        public static int[] ToInt32Array(this ushort[] registers)
        {
            if (registers == null || registers.Length % 2 != 0)
                throw new ArgumentException("Количество регистров должно быть четным");

            int[] result = new int[registers.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = registers.ToInt32(i * 2);
            }
            return result;
        }

        /// <summary>
        /// Конвертирует весь массив регистров в массив чисел с плавающей точкой
        /// </summary>
        public static float[] ToFloatArray(this ushort[] registers)
        {
            if (registers == null || registers.Length % 2 != 0)
                throw new ArgumentException("Количество регистров должно быть четным");

            float[] result = new float[registers.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = registers.ToFloat(i * 2);
            }
            return result;
        }

        /// <summary>
        /// Конвертирует один регистр в 16-битное целое число со знаком
        /// </summary>
        public static short ToInt16(this ushort[] registers, int startIndex = 0)
        {
            if (registers == null || registers.Length < startIndex + 1)
                throw new ArgumentException("Недостаточно регистров для конвертации в Int16");

            return (short)registers[startIndex];
        }

        /// <summary>
        /// Конвертирует один регистр в логическое значение (0 = false, остальное = true)
        /// </summary>
        public static bool ToBoolean(this ushort[] registers, int startIndex = 0)
        {
            if (registers == null || registers.Length < startIndex + 1)
                throw new ArgumentException("Недостаточно регистров для конвертации в Boolean");

            return registers[startIndex] != 0;
        }
    }
}
