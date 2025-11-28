using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PortsWork
{
    public class GeneratorUSB : PortGenerator
    {
        public const string SIGNALTYPE_SIN = "SINE";
        public const string SIGNALTYPE_DCVOLTAGE = "DC";

        protected VisaDevice generator;

        public override string GetName()
        {
            return "USB";
        }

        public override bool SetName( string name )
        {
            if ( string.IsNullOrEmpty( name ) || !generator.OpenVisaDevice( name ) )
            {
                return false;
            }
            ClosePort();
            return true;
        }

        public override bool OpenPort()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Linux.Acsessusb(PortName);
            if ( generator.isOpened )
            {
                return true;
            }
            bool result = generator.OpenVisaDevice();
            ChangeSignalType( SignalType.Sine );
            SetZeroSignal();
            SetChannel( channelNum );
            return result;
        }

        public override bool IsOpened()
        {
            return generator.isOpened;
        }

        protected override void WriteMessage( string message )
        {
            generator.Write( message );
            Sleep( 10 );
        }
        public override void SetLOAD(string ohm)
        {
            WriteMessage("C" + channelNum + ":BSWV LOAD," + ohm + "\n");
        }
        public override void SetFrequency( string freq )
        {
            WriteMessage( "C" + channelNum + ":BSWV FRQ, " + freq.Replace( ",", "." ) + "\n" );
        }

        public override void SetVoltage( string volt )
        {
            WriteMessage( "C" + channelNum + ":BSWV AMP, " + volt.Replace( ",", "." ) + "\n" );
        }

        public override void SetOffset( double value )
        {
            WriteMessage( "C" + channelNum + ":BSWV OFST, " + value.ToString().Replace( ",", "." ) + "\n" );
        }

        public override void SetZeroSignal()
        {
            SetVoltage( VOLTAGERANGE_MIN );
        }

        public override void SetChannel( int num )
        {
            channelNum = num;
            WriteMessage( "C" + channelNum + ":OUTP ON\n" );
        }

        public override void ClosePort()
        {
            generator.Close();
        }

    }
}
