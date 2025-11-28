using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PortsWork
{
	public class GeneratorGSSxx : PortGenerator
	{
		public const string SIGNALTYPE_SIN = "SIN";
		public const string SIGNALTYPE_DCVOLTAGE = "P_DC";

		public GeneratorGSSxx()
		{
			VOLTAGERANGE_MIN = 0.002;
		}

		public override bool OpenPort()
		{

			try
			{
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Linux.Acsessusb(PortName);
                if ( !IsOpen )
				{
					Open();
					Sleep( 100 );
					ChangeSignalType( SignalType.Sine );				
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

		public override void SetFrequency( string freq )
		{
			WriteMessage( "FREQ " + freq.Replace( ",", "." ) + " HZ" );
			Sleep( 50 );
		}

		public override void SetVoltage( string volt )
		{
			WriteMessage( @"VOLT " + volt.Replace( ",", "." ) );
			Sleep( 50 );
		}

		public override void SetOffset( double value )
		{
			SetVoltage( ( value / 2 ).ToString() );
		}

		public override void ClosePort()
		{
			if ( IsOpen )
			{
				WriteRemoteMode( false );
			}
			base.ClosePort();
		}

		public override void ChangeSignalType( SignalType type )
		{
			string typeText = "";
			switch (type )
			{
				case SignalType.Sine:
					typeText = SIGNALTYPE_SIN;
					break;
				case SignalType.DC:
					typeText = SIGNALTYPE_DCVOLTAGE;
					break;
			}
			WriteMessage( "FUNC:SHAPE " + typeText );
			Sleep( 50 );
		}

		public override void SetZeroSignal()
		{
			SetVoltage( VOLTAGERANGE_MIN );
			SetFrequency( "1 MHZ" );
		}
	}
}
