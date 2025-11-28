using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PortsWork
{
	public class MultimeterPicotest : PortMultimeter
	{
		private const int SLEEP_READ = 7100;

		private VisaDevice multimeter;

		public MultimeterPicotest()
		{
			multimeter = new VisaDevice();
		}

		public override string GetName()
		{
			return "USB";
		}

		public override bool SetName( string name )
		{
			if ( string.IsNullOrEmpty( name ) || !multimeter.OpenVisaDevice( name ) )
			{
				return false;
			}
			ClosePort();
			return true;
		}

		public override bool OpenPort()
		{
            if ( multimeter.isOpened )
			{
				return true;
			}
			Console.WriteLine(GetName());
            multimeter.Close();
            bool result = multimeter.OpenVisaDevice();
            Debug.WriteLine($"result {result}");
            //writeremotemode
            WriteBandFilter( BASE_FILTERS[ 1 ].ToString() );
			SetWorkType( typeMultimeter, typeSignal, false );
			return result;
		}

		public override bool IsOpened()
		{
			return multimeter.isOpened;
		}

		protected override void WriteMessage(string message)
		{
			multimeter.Write(message);
		}

		public override double GetVoltage( string type, int time )
		{
			SetWorkType( DEVICE_VOLTMETER, type, true );
			try
			{
				string result = "";
				if ( ( filter != BASE_FILTERS[ 0 ] ) )
				{
					result = multimeter.Query( MESSAGE_READ );
                    Sleep( 10 );			
				} else
				{
					multimeter.Write( MESSAGE_READ );
					Sleep( SLEEP_READ );
					result = multimeter.Read();
					//System.Windows.Forms.MessageBox.Show( "readed " + result );
				}
				return ConvertString( result.Replace( ".", "," ) );
			} catch ( TimeoutException )
			{
				return DOUBLE_FALSEVALUE;
			}
		}

		public override double GetAmperage()
		{
			SetWorkType( DEVICE_AMMETER, SIGNALTYPE_DC, true );
			string result = multimeter.Query( MESSAGE_READ );
            Sleep( 10 );
			return ConvertString( result.Replace( ".", "," ) ) * TO_MILLIVALUES;
		}

		public override void ClosePort()
		{
			//writelocalmode
			if ( multimeter.isOpened )
			{
				WriteRemoteMode( false );
			}
			multimeter.Close();
		}
	}
}
