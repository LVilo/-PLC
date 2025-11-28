using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
namespace PortsWork
{
	public class ModbusTCP : PortSensor 
	{
		public const int PORT_CONNECTION = 502;
		public const int LEN_WRITEMESSAGE_TCP = 6;
		public const int LEN_MULTIPLEWRITE_TCP = 7;

		private TcpClient client;
		private Socket socket;

		private string IP;

		public ModbusTCP()
		{
			client = new TcpClient();
			ANSWER_START = 9;
			skipAtEnd = 0;
		}

		public override bool SetName( string name )
		{
			IP = name;
			return string.IsNullOrEmpty( name );
		}

		public override string GetName()
		{
			return IP;
		}

		public override bool OpenPort()
		{
			try
			{
				if ( !client.Connected )
				{
					client.Connect( IP, PORT_CONNECTION );
					socket = client.Client;
				}
				return true;
			} catch
			{
				new Exception("Контроллер с IP-адресом " + IP.ToString() + " не найден");
				return false;
			}
		}

		protected override byte[] CreateMessage( byte address, byte funct, int register, int value )
		{
			byte[] result = new byte[ LEN_WRITEMESSAGE_TCP ];
			byte[] tmp;
			result[ 0 ] = 0;
			result[ 1 ] = funct;
			tmp = BitConverter.GetBytes( register - 1 );
			result[ 2 ] = tmp[ 1 ];
			result[ 3 ] = tmp[ 0 ];
			tmp = BitConverter.GetBytes( value );
			result[ 4 ] = tmp[ 1 ];
			result[ 5 ] = tmp[ 0 ];
			return result;
		}

		protected override byte[] CreateMessageWrite( byte address, int register, int len, byte[] values )
		{
			byte[] result = new byte[ LEN_MULTIPLEWRITE_TCP + 2 * len ];
			byte[] tmp = new byte[ 2 ];
			result[ 0 ] = 0;
			result[ 1 ] = WRITEMANY_HOLDING;
			tmp = BitConverter.GetBytes( register - 1 );
			result[ 2 ] = tmp[ 1 ];
			result[ 3 ] = tmp[ 0 ];
			tmp = BitConverter.GetBytes( len );
			result[ 4 ] = tmp[ 1 ];
			result[ 5 ] = tmp[ 0 ];
			result[ 6 ] = (byte) ( 2 * len );
			for ( int i = 0; i < len; i++ )
			{
				result[ LEN_MULTIPLEWRITE_TCP + 2 * i ] = values[ 2 * i + 1 ];
				result[ LEN_MULTIPLEWRITE_TCP + 2 * i + 1 ] = values[ 2 * i ];
			}
			return result;
		}

		//public int[] GetHoldingRegisters( int register, int len )
		//{
		//	byte[] tmp = GetValue( PortSensor.READMANY_HOLDING, register, len );
		//	//преобразование
		//	return new int[ 1 ];
		//}

		//public int[] GetInputRegisters( int register, int len )
		//{
		//	byte[] tmp = GetValue( PortSensor.READMANY_INPUT, register, len );

		//	return new int[ 1 ];
		//}

		//public int GetOneHoldingRegister( int register )
		//{
		//	int[] result = GetHoldingRegisters( register, 1 );
		//	return result != null ? result[ 0 ] : 0;
		//}

		//public int GetOneInputRegister( int register )
		//{
		//	int[] result = GetInputRegisters( register, 1 );
		//	return result != null ? result[ 0 ] : 0;
		//}

		//Запрос значений от устройства (общая функция)
		public override byte[] GetClearAnswer( byte address, byte funct, int register, ushort len, int sleep, out int bytes )
		{
			bytes = 0;
			byte[] messageFirst = { 0, 1, 0, 0, 0, (byte) ( LEN_WRITEMESSAGE_TCP ) }; //длина отправляемого пакета
			byte[] message = CreateMessage( 0, funct, register, len );
			byte[] response = new byte[ 9 + 2 * len ] ;
			try
			{
				socket.Send( messageFirst );
				socket.Send( message );
				socket.Receive( response );
				bytes = response.Length;
                Sleep( 10 );
			} catch ( Exception ex)
			{
				new Exception("Ошибка связи с контроллером:\n" + ex.Message);
			}
			return response;
		}

		public override bool SetMultipleValue( byte address, int register, byte[] values, int sleep )
		{
			byte[] messageFirst = { 0, 1, 0, 0, 0, (byte) ( LEN_MULTIPLEWRITE_TCP + values.Length ) };
			byte[] message = CreateMessageWrite( 0, register, values.Length / 2, values );
			byte[] response = new byte[ 9 + values.Length ];
			try
			{
				socket.Send( messageFirst );
				socket.Send( message );
				socket.Receive( response );
                Sleep( 10 );
			} catch ( Exception ex )
			{
                new Exception("Ошибка связи с контроллером:\n" + ex.Message);
            }
			return true;
		}

		public override bool SetValue( byte address, byte type, int register, int value, int sleep )
		{
			byte[] convertedValue = BitConverter.GetBytes(value);
			return SetMultipleValue( address, register, convertedValue, sleep );
		}


	}
}
