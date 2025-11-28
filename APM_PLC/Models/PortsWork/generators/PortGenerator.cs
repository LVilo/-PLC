using System;

namespace PortsWork
{
	public class PortGenerator : Port
	{
		public enum SignalType { Sine, DC }

        protected int channelNum = 1;

        public double VOLTAGERANGE_MIN
		{
			get;
			protected set;
		}
        /// <summary>
        /// Задание сопротивления
        /// </summary>
        /// <param name="ohm">Сопротивление, Ом</param>
        public virtual void SetLOAD(string ohm)
        {
            WriteMessage("SOURCE" + channelNum + ":LOAD " + ohm);
            Sleep(500);
        }

        /// <summary>
        /// Задание сопротивления
        /// </summary>
        /// <param name="ohm">Частота, Гц</param>
        public void SetLOAD(uint ohm)
        {
            SetLOAD(ohm.ToString());
        }

        /// <summary>
        /// Задание частоты
        /// </summary>
        /// <param name="freq">Частота, Гц</param>
        public virtual void SetFrequency( string freq )
		{
            WriteMessage("SOURCE" + channelNum + ":FREQ " + freq.Replace(",", ".") + " HZ");
            Sleep(500);
        }

		/// <summary>
		/// Задание частоты
		/// </summary>
		/// <param name="freq">Частота, Гц</param>
		public void SetFrequency( double freq )
		{
			freq = Math.Round( freq, 6 );
			SetFrequency( freq.ToString() );
		}

		/// <summary>
		/// Задание амплитуды
		/// </summary>
		/// <param name="volt">Напряжение, В</param>
		public virtual void SetVoltage( string volt )
		{
		}

		/// <summary>
		/// Задание амплитуды
		/// </summary>
		/// <param name="volt">Напряжение, В</param>
		public void SetVoltage( double volt )
		{
			volt = Math.Round( volt, 6 );
			SetVoltage( volt.ToString() );
		}

        public virtual void SetOffset(string value)
        {
            //SetVoltage( ( value / 2 ).ToString() ); // ??????????? 
            WriteMessage(@"SOURCE" + channelNum + ":OFST " + value.Replace(",", "."));
            Sleep(500);
        }
        
        /// <summary>
        /// Выставление типа сигнала
        /// </summary>
        /// <param name="type">Вид сигнала</param>
        public virtual void ChangeSignalType( SignalType type )
		{
		}

		/// <summary>
		/// Выставление нулевой амплитуды
		/// </summary>
		public virtual void SetZeroSignal()
		{
		}

		/// <summary>
		/// Задание постоянного напряжения
		/// </summary>
		/// <param name="value"></param>
		public virtual void SetOffset( double value )
		{

		}

        public virtual void SetChannel( int num )
        {

        }

		public override Port IdentifyDeviceType()
		{
            try
			{
                Port result = new PortGenerator();
                if ( !GetName().Contains( "USB" )  || GetName().Contains("ttyUSB"))
                {
                    
                    if (!OpenPort())
                    {
                        result.SetName(GetName());
                        return result;
                    }
                    string idn = ReadMessage(MESSAGE_IDN);
                    ClosePort();
                    if (idn.Contains("Suin"))
                    {
                        result = new GeneratorAKIP3407x();
                    }
                    else if (idn.Contains("GSS"))
                    {
                        result = new GeneratorGSSxx();
                    }
                    result.SetName(GetName());
                    return result;
                }
				else
				{
                    if (usbInfo.devType.Contains("AKIP-3409"))
                    {
                        result = new GeneratorAKIP3409x();
                    }
                    else
                    {
                        result = new GeneratorSDG1xxx();
                    }
                    result.SetName(GetName());
                    return result;
                }
				
			} catch ( Exception e )
			{
                new Exception(e.Message);
				Port result = new PortGenerator();
				result.SetName( GetName() );
				return result;
			}
		}

	}
}
