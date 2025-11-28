using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortsWork
{
	public class MultimeterAgilent : PortMultimeter
	{
		public MultimeterAgilent( int type )
		{
			DtrEnable = true;
			typeMultimeter = type;
			typeSignal = SIGNALTYPE_AC;
		}

		public MultimeterAgilent()
		{
			ReadTimeout = 3000;
			WriteTimeout = 3000;
			DtrEnable = true;
			typeMultimeter = DEVICE_VOLTMETER;
			typeSignal = SIGNALTYPE_AC;
		}

		public override bool OpenPort()
		{
			try
			{
				if ( !IsOpen )
				{
					Open();
					Sleep( 100 );
					WriteRemoteMode( true );
					WriteBandFilter( BASE_FILTERS[ 1 ].ToString() );
					Sleep( 1000 );
					SetWorkType( typeMultimeter, typeSignal, false );
					Sleep( 1000 );
					
				}
				return CheckPort();
			} catch
			{
				new Exception("Ошибка открытия порта " + PortName);
				Close();
				return false;
			}
		}

		public override double GetVoltage( string type, int time )
		{
			SetWorkType( DEVICE_VOLTMETER, type, true );
			try
			{
				WriteMessage( MESSAGE_READ );
				return WaitPortAnswer() ? ConvertString( ReadLine().Replace( ".", "," ) ) : DOUBLE_FALSEVALUE;
			} catch ( TimeoutException )
			{
				return DOUBLE_FALSEVALUE;
			}
        }

		public override double GetAmperage()
		{
			SetWorkType( DEVICE_AMMETER, SIGNALTYPE_DC, true );
			WriteMessage( MESSAGE_READ );
			if ( !WaitPortAnswer() )
			{
				return DOUBLE_FALSEVALUE;
			}
			double res = ConvertString( ReadLine().Replace( ".", "," ) ) * TO_MILLIVALUES;
			Console.WriteLine( "Считанный ток " + res );
			return res;
		}

		public override void ClosePort()
		{
			if ( IsOpen )
			{
				WriteRemoteMode( false );
			}
			base.ClosePort();
		}
	}
}
