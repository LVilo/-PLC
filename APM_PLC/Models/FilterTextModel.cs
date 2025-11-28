using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace APM_PLC.Models
{
    public static class FilterTextModel
    {
        public static string? OnlyDigits(string? str)
        {
            var digitsOnly = new string(str?.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length > 20)
            {
                digitsOnly = str?[..20];
            }
            return digitsOnly;
        }
        public static string? OnlyLetterOrDigit(string? str)
        {
            var cleaned = new string(str?.Where(char.IsLetterOrDigit).ToArray());
            if (cleaned.Length > 20)
            {
                cleaned = str?[..20];
            }
            return cleaned;
        }
        public static string OnlyFloat(string? str)
        {
            string filteredText = "";
            bool foundComma = false;
            int commaCount = 0;
            if (str.StartsWith(',')) { str = str[1..]; }
            foreach (char c in str)
            {
                if (char.IsDigit(c) || (c == ',' && !foundComma))
                {
                    filteredText += c;
                    if (c == ',')
                    {
                        foundComma = true;
                        commaCount++;
                    }
                }
                if (filteredText.Length == 9) break;
            }
            int commaIndex = filteredText.IndexOf(',');
            if (commaIndex != -1 && filteredText.Length - commaIndex > 4) // 3, потому что один символ для запятой
            {
                filteredText = filteredText.Substring(0, commaIndex + 4);
            }
            return filteredText;
        }
    }
}
