using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using RsVisaLoader;

namespace PortsWork
{
	public class GeneratorSDG1xxx : GeneratorUSB
	{

		public GeneratorSDG1xxx()
		{
			VOLTAGERANGE_MIN = 0.004;
			generator = new VisaDevice();
		}	

		public override void ChangeSignalType( SignalType type )
		{
			string typeText = "";
			switch ( type )
			{
				case SignalType.Sine:
					typeText = SIGNALTYPE_SIN;
					break;
				case SignalType.DC:
					typeText = SIGNALTYPE_DCVOLTAGE;
					break;
			}
			WriteMessage( "C" + channelNum + ": BSWV WVTP, " + typeText + "\n" );
		}
	}
}
