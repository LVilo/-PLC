using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortsWork
{
	public class VisaDeviceInformation
	{
		public string description; //текстовое значение адреса порта
		public string devType; //описание типа устройства из idn
		public string serialNum;
		
		public string GetInfo()
		{
			return devType + ": " + serialNum;
		}
	}
}
