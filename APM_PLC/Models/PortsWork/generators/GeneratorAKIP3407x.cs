using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PortsWork
{
	public class GeneratorAKIP3407x : PortGenerator
	{
		public const string SIGNALTYPE_SIN = "SIN";
		public const string SIGNALTYPE_DCVOLTAGE = "P_DC";

		public GeneratorAKIP3407x()
		{
			VOLTAGERANGE_MIN = 0.002;
		}

		public override bool OpenPort()
		{
			Console.WriteLine("------------OpenPort");

            try
			{
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Linux.Acsessusb(PortName);
                if ( !IsOpen )
				{
                    Console.WriteLine("------------if");
                    Console.WriteLine(GetName());
                    Open();
					Sleep( 100 );
					ChangeSignalType( SignalType.Sine );				
					Sleep( 1000 );

                    SetChannel( channelNum );
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
			WriteMessage( "SOURCE" + channelNum + ":FREQ " + freq.Replace( ",", "." ) + " HZ" );
			Sleep( 500 );
		}

		public override void SetVoltage( string volt )
		{
			WriteMessage( @"SOURCE" + channelNum + ":VOLT " + volt.Replace( ",", "." ) );
			Sleep( 500 );
		}

		public override void SetOffset( string value ) 
		{
			//SetVoltage( ( value / 2 ).ToString() ); // ??????????? 
            WriteMessage(@"SOURCE" + channelNum + ":VOLT:OFFSET " + value.Replace(",", "."));
            Sleep(500);
        }
        public override void SetOffset(double value)
        {
            //SetVoltage( ( value / 2 ).ToString() ); // ??????????? 
            value = Math.Round(value, 6);
            SetOffset(value.ToString());
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
			WriteMessage( "SOURCE" + channelNum + ":FUNC " + typeText );
			Sleep( 500 );
		}

        public override void SetChannel( int num )
        {
            channelNum = num;
            WriteMessage( "OUTP" + channelNum + ":STATE 1" );
            Sleep( 500 );
        }

        public override void SetZeroSignal()
		{
			SetVoltage( VOLTAGERANGE_MIN );
			SetFrequency( "10000" );
		}
	}
}
