using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace PortsWork
{
	public class ModbusRTU : PortSensor
	{
		private const int LEN_WRITEMESSAGE_RTU = 8;
		private const int BASE_ATTEMPTS = 25;

		public ModbusRTU()
		{
			ANSWER_START = 3;
			skipAtEnd = 1;
		}

		public ModbusRTU( int baudrate, StopBits stop)
		{
			BaudRate = baudrate;
			StopBits = stop;
			ReadTimeout = 1000;
			WriteTimeout = 1000;
			ANSWER_START = 3;
			skipAtEnd = 1;
		}

		public void SetParameters( int baudrate, StopBits stop )
		{
			BaudRate = baudrate;
			StopBits = stop;
		}

		protected void GetCRC( byte[] message, ref byte[] CRC )
		{
			ushort CRCFull = 0xFFFF;

			for ( int i = 0; i < ( message.Length ) - 2; i++ )
			{
				CRCFull = (ushort) ( CRCFull ^ message[ i ] );

				for ( int j = 0; j < 8; j++ )
				{
					char CRCLSB = (char) ( CRCFull & 0x0001 );
					CRCFull = (ushort) ( ( CRCFull >> 1 ) & 0x7FFF );

					if ( CRCLSB == 1 )
						CRCFull = (ushort) ( CRCFull ^ 0xA001 );
				}
			}
			CRC[ 1 ] = (byte) ( ( CRCFull >> 8 ) & 0xFF );
			CRC[ 0 ] = (byte) ( CRCFull & 0xFF );
		}
		protected override byte[] CreateMessage( byte address, byte funct, int register, int value )
		{
			byte[] result = new byte[ LEN_WRITEMESSAGE_RTU ];
			byte[] tmp;
			result[ 0 ] = address;
			result[ 1 ] = funct;
			tmp = BitConverter.GetBytes( register - 1 );
			result[ 2 ] = tmp[ 1 ];
			result[ 3 ] = tmp[ 0 ];
			tmp = BitConverter.GetBytes( value );
			result[ 4 ] = tmp[ 1 ];
			result[ 5 ] = tmp[ 0 ];
			GetCRC( result, ref tmp );
			result[ 6 ] = tmp[ 0 ];
			result[ 7 ] = tmp[ 1 ];
			return result;
		}

		protected byte[] CreateMessageMultipleWrite( byte address, int register, byte[] values )
		{
			byte[] result = new byte[ LEN_WRITEMESSAGE_RTU + 1 + values.Length ];
			byte[] tmp;
			result[ 0 ] = address;
			result[ 1 ] = WRITEMANY_HOLDING;
			tmp = BitConverter.GetBytes( register - 1 );
			result[ 2 ] = tmp[ 1 ];
			result[ 3 ] = tmp[ 0 ];
			tmp = BitConverter.GetBytes( (int) values.Length / 2 );
			result[ 4 ] = tmp[ 1 ];
			result[ 5 ] = tmp[ 0 ];
			result[ 6 ] = (byte) values.Length;

			for ( int i = 0; i < values.Length / 2; i++ )
			{
				result[ 7 + 2 * i ] = values[ 2 * i + 1 ];
				result[ 7 + 2 * i + 1 ] = values[ 2 * i ];
			}

			GetCRC( result, ref tmp );
			result[ result.Length - 2 ] = tmp[ 0 ];
			result[ result.Length - 1 ] = tmp[ 1 ];
			return result;
		}

		// Ожидание ответа устройства
		protected bool WaitPortAnswer( byte[] message, int answerLength, int sleepTime, int attempts, out int bytes )
		{
			bytes = 0;
			for ( int i = 0; i < attempts; i++ )
			{
				DiscardOutBuffer();
				DiscardInBuffer();
				Write( message, 0, message.Length );
				Sleep( sleepTime );
				bytes = BytesToRead;
				if ( bytes >= answerLength )
				{
					bytes = answerLength;
					return true; 
				}
			}
			return false;
		}

		// Получение ответа
		protected void ReadPortAnswer( int bytes, ref byte[] response )
		{
			for ( int i = 0; i < bytes; i++ )
			{
				response[ i ] = (byte) ( ReadByte() );
			}
		}

		public override byte[] GetClearAnswer( byte address, byte type, int register, ushort len, int sleep, out int bytes )
		{
			bytes = 0;
			byte[] message = CreateMessage( address, type, register, len );
			int answerLen = CountAnswerLength( type, len );
			byte[] response = new byte[ 5 + answerLen ];
			try
			{
				if ( !WaitPortAnswer( message, response.Length, sleep, BASE_ATTEMPTS, out bytes ) )
				{
					Console.WriteLine( "Ошибка 1 чтения из порта " + PortName );
					return null;
				}
				ReadPortAnswer( bytes, ref response );
			} catch
			{
				Console.WriteLine( "Ошибка 2 чтения из порта " + PortName );
				return null;
			}
			return response;
		}

		public override bool SetValue( byte address, byte type, int register, int value, int sleep )
		{
			Console.WriteLine( "writing " + value + " to register " + register );
			byte[] message = CreateMessage( address, type, register, value );
			byte[] response = new byte[ LEN_WRITEMESSAGE_RTU ];
			int bytes;
			try
			{
				if ( !WaitPortAnswer( message, LEN_WRITEMESSAGE_RTU, sleep, BASE_ATTEMPTS, out bytes ) )
				{
					Console.WriteLine( "Ошибка 3 чтения из порта " + PortName );
					return false;
				}
				ReadPortAnswer( bytes, ref response );
			} catch
			{
				Console.WriteLine( "Ошибка 4 чтения из порта " + PortName );
				return false;
			}
			for ( int i = 0; i < bytes; i++ )
			{
				if ( message[ i ] == response[ i ] )
					continue;
				Sleep( 5000 );
				if ( GetOneHoldingValue( address, register, sleep ) == value )
				{
					Console.WriteLine( "Неверный ответ устройства, запись значения прошла успешно" );
					break;
				}
				if ( GetOneHoldingValue( address, register, sleep ) == value )
				{
					Console.WriteLine( "Неверный ответ устройства, запись значения прошла успешно" );
					break;
				}
				if ( GetOneHoldingValue( address, register, sleep ) == value )
				{
					Console.WriteLine( "Неверный ответ устройства, запись значения прошла успешно" );
					break;
				}
				if ( GetOneHoldingValue( address, register, sleep ) == value )
				{
					Console.WriteLine( "Неверный ответ устройства, запись значения прошла успешно" );
					break;
				}
				if ( GetOneHoldingValue( address, register, sleep ) == value )
				{
					Console.WriteLine( "Неверный ответ устройства, запись значения прошла успешно" );
					break;
				}
				if ( GetOneHoldingValue( address, register, sleep ) == value )
				{
					Console.WriteLine( "Неверный ответ устройства, запись значения прошла успешно" );
					break;
				}
				if ( GetOneHoldingValue( address, register, sleep ) == value )
				{
					Console.WriteLine( "Неверный ответ устройства, запись значения прошла успешно" );
					break;
				}

				Console.WriteLine( "Ошибка записи зачения" );
				return false;
			}
			return true;
		}

		public override bool SetMultipleValue( byte address, int register, byte[] values, int sleep )
		{
			byte[] message = CreateMessageMultipleWrite( address, register, values );
			byte[] response = new byte[ LEN_WRITEMESSAGE_RTU ];
			int bytes;
			try
			{
				if ( !WaitPortAnswer( message, LEN_WRITEMESSAGE_RTU, sleep, BASE_ATTEMPTS, out bytes ) )
				{
					Console.WriteLine( "Ошибка 5 чтения из порта " + PortName );
					return false;
				}
				ReadPortAnswer( bytes, ref response );
			} catch
			{
				Console.WriteLine( "Ошибка 6 чтения из порта " + PortName );
				return false;
			}
			return true;
		}
	}
}
