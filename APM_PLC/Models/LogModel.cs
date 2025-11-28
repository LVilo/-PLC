using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APM_PLC.Models
{
    public class LogModel
    {
       public LogModel()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
               .WriteTo.File($"Log\\log-{DateTime.Now:dd.MM.yyyy}.txt", //настройка названия файла
               outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}") // настройка записи в файл


               .WriteTo.File($@"\\files\Общее\Прошивки и методики проверки\Прикладное ПО\АРМ настройки CNV\CommonLogs\log-{DateTime.Now:dd.MM.yyyy}.txt",
               outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
               .CreateLogger();
        }
        public void Write(string msg)
        {
            Log.Information(Environment.UserName + msg);
        }
    }
}
