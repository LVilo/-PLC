using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RsVisaLoader;

namespace PortsWork
{
	public class VisaDevice
	{
		private int mDefaultRM = visa32.VI_NULL;
		private int session;
		private string deviceDescription;
		public bool isOpened
		{
			get;
			private set;
		}

		public VisaDevice()
		{
			visa32.viOpenDefaultRM( out mDefaultRM );
		}

        private static bool IsVisaLibraryInstalled(UInt16 iManfId)
        {
            return RsVisa.RsViIsVisaLibraryInstalled(iManfId) != 0;
        }

        public List<string> FindVisaDevice()
		{
            List<string> result = new List<string>();
            if (!IsVisaLibraryInstalled(RsVisa.RSVISA_MANFID_DEFAULT) &&
                !IsVisaLibraryInstalled(RsVisa.RSVISA_MANFID_RS))
            {
                new Exception("Не установлены драйвера VISA взаимодействия с удаленными устройствами");

                return result;
            }

            int vi = 0;
			int retCount = 0;
			StringBuilder devLstDescription = new StringBuilder( 256 );
			visa32.viFindRsrc( mDefaultRM, "?*", out vi, out retCount, devLstDescription );
			if ( retCount == 0 )
			{
				Console.WriteLine( "Не найдено внешних USB-устройств" );
				return result;
			}
			result.Add( devLstDescription.ToString() );
			for ( int i = 1; i < retCount; i++ )
			{
				visa32.viFindNext( vi, devLstDescription );
				result.Add( devLstDescription.ToString() );
			}
			return result;
		}

		public bool OpenVisaDevice( string description )
		{
			Close();
			deviceDescription = description;
			return OpenVisaDevice();
		}

		public bool OpenVisaDevice()
		{
			//ViStatus status = visa32.viOpen( mDefaultRM, deviceDescription, 0, 0, out session );
			ViStatus status = visa32.viOpen( mDefaultRM, deviceDescription, 0, 100, out session );
			if ( status < ViStatus.VI_SUCCESS )
			{
				//write error from status
				Console.WriteLine( "Ошибка открытия соединения с устройством" );
				return false;
			}
			isOpened = true;
			return true;
		}

		public void GetIdnInfo( string message, out string type, out string serialNum )
		{
			string idn = Query( message );
			if ( idn == "" )
			{
				type = "undefined device";
				serialNum = "";
				return;
			}
			idn = idn.Remove( 0, idn.IndexOf(",") + 1 );
			type = idn.Remove( idn.IndexOf( "," ) );
			idn = idn.Remove( 0, idn.IndexOf( "," ) + 1 );
			serialNum = idn.Remove( idn.IndexOf( "," ) );
			Write( Port.MESSAGE_LOCALMODE );
		}

		private static ViStatus Write( int vi, string buffer )
		{
			int retCount;
			return visa32.viWrite( vi, buffer, buffer.Length, out retCount );
		}

		//Чтение результата с устройства
		private static ViStatus Read( int vi, out string buffer )
		{
			ViStatus status;
			buffer = "";
			StringBuilder sTemp = new StringBuilder( 1024 );
			do
			{
				int retCount;
				status = visa32.viRead( vi, sTemp, sTemp.Capacity, out retCount );
				if ( retCount > 0 )
				{
					buffer += sTemp.ToString( 0, retCount );
				}
			} while ( status == ViStatus.VI_SUCCESS_MAX_CNT );

			return status;
		}

		//Отправка запроса, предполагающего ответ
		private ViStatus Query( int vi, string sQuery, out string sAnswer )
		{
			sAnswer = "";
			ViStatus status = Write( vi, sQuery );
			if ( status < 0 )
			{
				return status;
			}
			return Read( vi, out sAnswer );
		}

		//Отправка сообщения на устройство
		public bool Write( string message )
		{
			ViStatus status = Write( session, message );
			if ( status < ViStatus.VI_SUCCESS )
			{
				Console.WriteLine( "Error at message send" );
				return false;
			}
			return true;
		}

		public string Read()
		{
			string message;
			ViStatus status = Read( session, out message );
			if ( status < ViStatus.VI_SUCCESS )
			{
				Console.WriteLine( "Error at message reading" );
				return string.Empty;
			}
			return message;
		}

		public string Query( string message )
		{
			string response;
			ViStatus status = Query( session, message, out response );
			if ( status < ViStatus.VI_SUCCESS )
			{
				Console.WriteLine( "Error at query" );
				return string.Empty;
			}
			return response;
		}

		public void Close()
		{
			isOpened = false;
			visa32.viClose( session );
		}

	}
}
