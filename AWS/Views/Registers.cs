using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Views
{
    public static class Registers
    {
        public const int REGISTER_ADRESS_VOLTAGE= 99;
        public const int REGISTER_ADRESS_COEFFICIENT_VOLTAGE = 21;
        public const int REGISTER_ADRESS_PASSWORD = 108;
        public const int REGISTER_ADRESS_ON_CHANNEL_4_20 = 58;
        public const int REGISTER_ADRESS_ON_CHANNEL_IEPE = 29;
        public const int REGISTER_ADRESS_ON_CHANNEL_485 = 73;
        public const int REGISTER_ADRESS_K_A = 16;
        public const int REGISTER_ADRESS_K_B = 18;
        public const int REGISTER_ADRESS_VOLTAGE_IEPE = 1;
        public const int REGISTER_ADRESS_K_A_4_20_INPUT = 52;
        public const int REGISTER_ADRESS_K_B_4_20_INPUT = 54;
        public const int REGISTER_ADRESS_LVL_mA = 37;
        public const int REGISTER_ADRESS_K_A_4_20_OUTPUT = 91;
        public const int REGISTER_ADRESS_K_B_4_20_OUTPUT = 93;
        public const int REGISTER_ADRESS_Output_mA = 63;
        public const int REGISTER_ADRESS_SOURCE_SIGNAL = 90;
        public const int REGISTER_ADRESS_RANGE = 95;
        public const int REGISTER_ADRESS_SPEED = 66;
        public const int REGISTER_ADRESS_TIME = 68;
        public const int REGISTER_ADRESS_PLC = 146;
        public const int REGISTER_ADRESS_NUMBER = 147;
        public const int REGISTER_ADRESS_CODE_FUNCTION = 148;
        public const int REGISTER_ADRESS_SETTINNGS_A = 157;
        public const int REGISTER_ADRESS_SETTINNGS_B = 159;
        public const int REGISTER_ADRESS_A = 151;
        public const int REGISTER_ADRESS_B = 153;
        public const int REGISTER_ADRESS_TYPE_DATA = 149;
        public const int REGISTER_ADRESS_ON_SURVEY = 145;
        public const int REGISTER_ADRESS_ERROR_CRC = 75;
        public const int REGISTER_ADRESS_ERROR_TIMEOUT = 77;
        public const int REGISTER_ADRESS_TIMEOUT = 77;

        public const int PASSWORD = 0xE485; //  -7035 AB           -461045760 ABCD   34276 ДЛЯ DCBA 
        public const int SAVE_CHANGE = 0x01E1; //  481  AB     31522816  ABCD   57601 ДЛЯ DCBA  
        public const float Coef_1 = 1;
        public const float Coef_0 = 0;
        public const int ON = 1;
        public const int OFF = 0;
        public const float SPEED = 115200;
        public const int TIME = 1000;
        public const int NUM_REG = 990;
        public const int NUM_FUNC = 4;
        public const int TIMEOUT = 500;


        public static Dictionary<int, string> Name = new()
        {
            [99] = "Напряжение питания контроллера",
            [21] = "Коэф. корректировки напряжение питания",
            [108] = "Пароль",
            [58] = "[4-20 ВХОД] переключатель",
            [29] = "[IEPE] переключатель",
            [73] = "[RS-485] переключатель",
            [16] = "[IEPE] Коэф. усиления",
            [18] = "[IEPE] Коэф. смещения",
            [1] = "[IEPE] Напряжение (переменка)",
            [52] = "[4-20 ВХОД] Коэф. усиления K",
            [54] = "[4-20 ВХОД] Коэф. смещения B",
            [37] = "[4-20 ВХОД] Ток, мА (постоянка)",
            [91] = "[4-20 ВЫХОД] Коэф. усиления",
            [93] = "[4-20 ВЫХОД] Коэф. смещения",
            [63] = "[4-20 ВЫХОД] Задать значение тока (настр. рег.)",
            [90] = "[4-20 ВЫХОД] Источник сигнала",
            [95] = "[4-20 ВЫХОД] Диапазон",
            [66] = "[485/MASTER] Скорость",
            [68] = "[485/MASTER] Период опроса, мс",
            [146] = "Адрес устройства",
            [147] = "Номер регистра",
            [148] = "Номер функции",
            [157] = "Нижняя предупредительная уставка",
            [159] = "Нижняя аварийная уставка",
            [151] = "Коэф. А",
            [153] = "Коэф. B",
            [149] = "Тип данных",
            [145] = "[485/MASTER] переключатель",
            [75] = "[485/MASTER] Ошибки CRC, %",
            [77] = "[485/MASTER] Ошибки TIMEOUT, %",
            [79] = "[485/MASTER] Количество потерянных пакетов"
        };

        // Обратный словарь для поиска по имени
        public static Dictionary<string, int> Adress = Name.ToDictionary(x => x.Value, x => x.Key);

    }
}
