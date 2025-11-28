using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortsWork
{
    class GeneratorAKIP3409x : GeneratorUSB
    {
        public GeneratorAKIP3409x()
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
            WriteMessage( "C" + channelNum + ":MDWV AM, MDSP, " + typeText + "\n" );
        }

    }
}
