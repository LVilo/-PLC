using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace PortsWork
{
	public class Port : SerialPort
	{
		public static bool stopFlag = false;

		public static int maxTimeoutForAnswer = 10000; // время ожидания появления ответа устройства

		public const int TO_MILLIVALUES = 1000;

		public const string MESSAGE_IDN = "*IDN?";
		public const string MESSAGE_REMOTEMODE = "SYST:REM";
		public const string MESSAGE_LOCALMODE = "SYST:LOC";

		protected const int BASE_BAUDRATE = 9600;
		protected const int BASE_DATABITS = 8;
		protected const int BASE_READTIMEOUT = 750;
		protected const int BASE_WRITETIMEOUT = 750;

        public VisaDeviceInformation usbInfo;
		//public UsbTmcDevice usbtmc;
        public Port()
		{
           // usbtmc = new UsbTmcDevice();
            BaudRate = BASE_BAUDRATE;
			DataBits = BASE_DATABITS;
			Parity = Parity.None;
			StopBits = StopBits.One;
			ReadTimeout = BASE_READTIMEOUT;
			WriteTimeout = BASE_WRITETIMEOUT;
		}

		/// <summary>
		/// Возвращает имя порта
		/// </summary>
		/// <returns>Возвращает имя(адрес) порта</returns>
		public virtual string GetName()
		{
			return PortName;
		}

		/// <summary>
		/// Задаёт имя порта
		/// </summary>
		/// <param name="name">Имя(адрес), которое требуется задать</param>
		/// <returns>Успешность операции </returns>
		public virtual bool SetName( string name )
		{

			if ( string.IsNullOrEmpty( name ) ) 
				return false;
			
                //usbtmc._path = name;
                PortName = name;
                return true;
		}

		/// <summary>
		/// Подключение порта внешнего устройства
		/// </summary>
		/// <returns>Успешность открытия порта для работы</returns>
		public virtual bool OpenPort()
		{
			//if (PortName.Remove(PortName.Length -1, 1) == "/dev/usbtmc")
			//{
			//             Console.WriteLine("---------if");
			//             try
			//	{
			//		if (!IsOpen)
			//		{
			//			Linux.Acsessusb(usbtmc._path);
			//			usbtmc.Open();
			//		}
			//                 Console.WriteLine($"Открыто устройство {PortName}");
			//                 return true;
			//             }
			//	catch (Exception ex)
			//             {
			//                 Console.WriteLine($"Ошибка при открытии {PortName}: {ex.Message}");
			//                 return false;
			//             }
			//}
			//else
			//{
			//             Console.WriteLine("---------else");
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Linux.Acsessusb(PortName);
			try
			{
				if (!IsOpen)
				{
					Open();
				}
				return true;
			}
			catch (Exception)
			{
				Close();
				return false;
			}
			//}
		}

		/// <summary>
		/// Закрытие работы с портом внешнего устройства
		/// </summary>
		public virtual void ClosePort()
		{
            //if (PortName.Remove(PortName.Length - 1, 1) == "/dev/usbtmc")
            //{
            //    usbtmc.Close();
            //}
            /*else */ Close();
        }

		public virtual bool IsOpened()
		{
			return IsOpen;
		}

		/// <summary>
		/// Отправка сообщения на внешнее устройство
		/// </summary>
		/// <param name="message">Команда для записи на устройство</param>
		protected virtual void WriteMessage( string message )
		{
   //         if (PortName.Remove(PortName.Length - 1, 1) == "/dev/usbtmc")
   //         {
   //             usbtmc.WriteMessage(message);
   //         }
			//else
				WriteLine(message);
		}

        /// <summary>
        /// Чтение с внешнего устройства
        /// </summary>
        /// <param name="message">Команда для записи на устройство</param>
        /// <returns>Ответ внешнего устройства</returns>
        public virtual string ReadMessage( string message )
		{
            Console.WriteLine("---------ReadMessage");
   //         if (PortName.Remove(PortName.Length - 1, 1) == "/dev/usbtmc")
			//{
			//	 return usbtmc.ReadMessage( message );
			//}
			//else
			//{
                WriteLine(message);
                WaitPortAnswer(2000);
                Console.WriteLine("---------WaitPortAnswer");
                return ReadLine();
            //}
		}

		/// <summary>
		/// Ожидание ответа от внешнего устройства
		/// </summary>
		/// <returns>Наличие ответа</returns>
		protected bool WaitPortAnswer()
		{
            return WaitPortAnswer( maxTimeoutForAnswer );
		}

		/// <summary>
		/// Ожидание ответа от внешнего устройства
		/// </summary>
		/// <param name="time">Время ожидания ответа, мс</param>
		/// <returns>Наличие ответа</returns>
		protected bool WaitPortAnswer( int time )
		{
			DiscardOutBuffer();
			DiscardInBuffer();
			for ( ; time > 0; time -= 10 )
			{
				if ( BytesToRead > 0 )
				{
					Console.WriteLine( "Оставшееся время ожидания: " + time );
					return true;
				}
				Sleep( 10 );
			}
			return false;
		}

		/// <summary>
		/// Подключение режима удалённой работы для внешних устройств
		/// </summary>
		/// <param name="needRemote">Режим удалённой/ручной работы</param>
		protected virtual void WriteRemoteMode( bool needRemote )
		{
			if ( needRemote )
			{
				WriteMessage( MESSAGE_REMOTEMODE );
				Sleep( 1000 );
			} else
			{
				WriteMessage( MESSAGE_LOCALMODE );
			}
		}

		/// <summary>
		/// Проверка ответа из порта
		/// </summary>
		/// <returns>Успешность проверки</returns>
		public virtual bool CheckPort()
		{
			string res = ReadMessage( MESSAGE_IDN );
			Console.WriteLine( "name: " + res );
			return true;
		}

		/// <summary>
		/// Инициализация внешнего устройства конкретного типа
		/// </summary>
		/// <returns>Объект для работы с подключенным внешним устройством</returns>
		public virtual Port IdentifyDeviceType()
		{
			return this;
		}

		/// <summary>
		/// Ищет подключенные к USB VISA-устройства
		/// </summary>
		/// <returns>Список описаний найденных устройств</returns>
		public static List<VisaDeviceInformation> FindVisaDevicesInfo()
		{
			VisaDevice device = new VisaDevice();
			List<VisaDeviceInformation> result = new List<VisaDeviceInformation>();
			
			List<string> descripts = device.FindVisaDevice();
			VisaDeviceInformation tmp;
			for ( int i = 0; i < descripts.Count; i++ )
			{
				tmp = new VisaDeviceInformation();
				tmp.description = descripts[ i ];
				if (!device.OpenVisaDevice( tmp.description))
				{
					continue;
				}
				device.GetIdnInfo( MESSAGE_IDN, out tmp.devType, out tmp.serialNum );
				result.Add( tmp );
				device.Close();
			}

			return result;
		}

		/// <summary>
		/// Ожидание системы
		/// </summary>
		/// <param name="time">Время ожидания, мс</param>
		public static void Sleep( int time )
		{
			for ( int i = 0; i < time; i += 10 )
			{
				Thread.Sleep( 10 );
                if ( stopFlag )
                {
                    stopFlag = false;
                    throw new StopProcessException();
                }
			}
		}

		/// <summary>
		/// Преобразовывает строку в число с правающей запятой
		/// </summary>
		/// <param name="s">Строка для преобразования в число</param>
		/// <returns>Найденное число</returns>
		protected double ConvertString( string s )
		{
			double d;
			bool findDouble = double.TryParse( s.Replace( ".", "," ), out d );
			if ( !findDouble )
			{
				Console.WriteLine( "I can't convert this value: " + s );
			}
			return d;
		}
        
    }

}
