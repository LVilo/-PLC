using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortsWork
{
    public class StopProcessException : ApplicationException
    {
        public StopProcessException()
            : base( "Остановка выполнения алгоритма пользователем" )
        {
        }
    }
}
