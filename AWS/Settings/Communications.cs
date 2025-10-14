using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.Settings
{
    public static class Log
    {
        public static Queue<string> messege = new Queue<string>();
        static Log()
        {
            Serilog.Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                            .WriteTo.File("Log\\log.txt", rollingInterval: RollingInterval.Day)
                            .WriteTo.File(@"\\files\Общее\Прошивки и методики проверки\Прикладное ПО\АРМ настройки PLC\CommonLogs\log.txt", rollingInterval: RollingInterval.Day)
                            .CreateLogger();
        }
        public static void CreateMessege(string mes)
        {
            messege.Enqueue(mes);
            WriteLog(mes);
        }
        public static void CreateMessege(Exception ex)
        {
            messege.Enqueue(ex.Message);
            WriteLog(ex.Message);
        }
        public static void WriteLog(string mes)
        {
            Debug.WriteLine(Environment.UserName + mes);
            Serilog.Log.Information(Environment.UserName + mes);
            Console.WriteLine(Environment.UserName + mes);
        }
    }
}
