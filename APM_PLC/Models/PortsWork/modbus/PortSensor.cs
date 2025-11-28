using System;
using System.IO.Ports;

namespace PortsWork
{
	public class PortSensor : Port
	{
		public const byte READMANY_COIL = 0x01;
		public const byte READMANY_DISCRETEINPUTS = 0x02;
		public const byte READMANY_HOLDING = 0x03;
		public const byte READMANY_INPUT = 0x04;
		public const byte WRITEONE_FLAG = 0x05;
		public const byte WRITEONE_HOLDING = 0x06;
		public const byte WRITEMANY_HOLDING = 0x10;

		public const int WRITEVALUE_TRUE = 0xFF00;
		public const int WRITEVALUE_FALSE = 0x0000;

		public int ANSWER_START;
		protected int skipAtEnd; //сколько ячеек ответа в конце незначимы

		/// <summary>
		/// Создание пакета для отправки на устройство
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="funct">Отправляемая на устройство Modbus-функция</param>
		/// <param name="register">Стартовый регистр</param>
		/// <param name="value">Параметр (записываемое значение или число считываемых байт)</param>
		/// <returns>Сгенерированный пакет для отправки на внешнее устройство</returns>
		/// <remarks>Без множественной записи</remarks>
		protected virtual byte[] CreateMessage( byte address, byte funct, int register, int value )
		{
			return null;
		}

		/// <summary>
		/// Создание пакета множественной записи (0х10 функция)
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Стартовый регистр</param>
		/// <param name="len">Количество отправляемых значений</param>
		/// <param name="values">Набор значений для записи</param>
		/// <returns>Сгенерированный пакет для отправки на внешнее устройство</returns>
		protected virtual byte[] CreateMessageWrite( byte address, int register, int len, byte[] values )
		{
			return null;
		}

		// Пересчет coil-регистров в результирующие флаги
		/// <summary>
		/// Пересчет coil-регистров в результирующие флаги
		/// </summary>
		/// <param name="numOfFlags">Число полученных Coil-регистров</param>
		/// <param name="answer">Значения, полученные с устройства</param>
		/// <returns>Набор полученных флагов</returns>
		private bool[] ConvertToFlagValues( int numOfFlags, int[] answer )
		{
			bool[] result = new bool[ numOfFlags ];
			for ( int i = 0; i < numOfFlags; i++ )
			{
				result[ i ] = Convert.ToBoolean( answer[ i / 8 ] % ( int ) Math.Pow( 2, i + 1 ) );
			}
			return result;
		}

		/// <summary>
		/// Подсчёт длины получаемого ответа, чистых значений
		/// </summary>
		/// <param name="type">Отправленная на устройство функция</param>
		/// <param name="length">Число запрошенных регистров</param>
		/// <returns>Число значимых байт ответа</returns>
		protected ushort CountAnswerLength( int type, ushort length )
		{
			switch ( type )
			{
				case READMANY_DISCRETEINPUTS:
				case READMANY_COIL:
					return ( ushort ) Math.Ceiling( ( double ) length / 8 );
				case READMANY_HOLDING:
				case READMANY_INPUT:
					return (ushort)(2 * length);
				default:
					return 0;
			}
		}

		protected byte[] ConvertToBytes( int[] values )
		{
			byte[] result = new byte[ values.Length * 2 ];
			byte[] tmp;
			for ( int i = 0; i < values.Length; i++ )
			{
				tmp = BitConverter.GetBytes( values[ i ] );
				result[ 2 * i ] = tmp[ 0 ];
				result[ 2 * i + 1 ] = tmp[ 1 ];
			}
			return result;
		}

		/// <summary>
		/// Перевод ответа из массива байт
		/// </summary>
		/// <param name="response"></param>
		/// <param name="type"></param>
		/// <param name="answerLength"></param>
		/// <param name="responseBytes"></param>
		/// <returns></returns>
		private int[] ConvertBytes( byte[] response, byte type, int answerLength, int responseBytes )
		{
			int[] k = null;
			switch ( type )
			{
				case READMANY_HOLDING:
				case READMANY_INPUT:
					k = new int[ answerLength / 2 ];
					byte[] temparr = new byte[ 2 ];
					for ( int l = 0; l < ( responseBytes - ANSWER_START ) / 2 - skipAtEnd; l++ )
					{
						temparr[ 0 ] = response[ 1 + l * 2 + ANSWER_START ];
						temparr[ 1 ] = response[ l * 2 + ANSWER_START ];
						k[ l ] = BitConverter.ToInt16( temparr, 0 );
					}
					break;
				case READMANY_COIL:
				case READMANY_DISCRETEINPUTS:
					k = new int[ answerLength ];
					for ( int l = 0; l < ( responseBytes - ANSWER_START ) / 2; l++ )
					{
						k[ l ] = response[ l * 2 + ANSWER_START ];
					}
					break;
			}
			return k;
		}

        public float ConvertToFloatValue(byte[] response, int start) // ABCD -> BADC 
        {
            if (response == null || response.Length <= start + 3)
                return 0;
            byte[] tmp = new byte[4];

            // Меняем байты внутри каждого 16-битного слова
            tmp[0] = response[start + 1];  // 2-й байт становится 1-м
            tmp[1] = response[start + 0];  // 1-й байт становится 2-м
            tmp[2] = response[start + 3];  // 4-й байт становится 3-м
            tmp[3] = response[start + 2];  // 3-й байт становится 4-м

            return BitConverter.ToSingle(tmp, 0);
        }

        public float ConvertToSwFloatValue(byte[] response, int start) // ABCD -> CDAB
        {
            if (response == null || response.Length <= start + 3)
                return 0;

            byte[] tmp = new byte[4];

            tmp[0] = response[start + 2];  // 3-й байт становится 1-м
            tmp[1] = response[start + 3];  // 4-й байт становится 2-м  
            tmp[2] = response[start + 0];  // 1-й байт становится 3-м
            tmp[3] = response[start + 1];  // 2-й байт становится 4-м

            return BitConverter.ToSingle(tmp, 0);
        }

        /// <summary>
        /// Запрос 1 coil-регистра ( 0х01 )
        /// </summary>
        /// <param name="address">Modbus-адрес устройства</param>
        /// <param name="register">Запрашиваемый регистр</param>
        /// <param name="sleep">Время ожидания ответа</param>
        /// <returns>Значение лигического регистра</returns>
        public bool GetOneCoilValue( byte address, int register, int sleep )
		{
			bool[] result = GetCoilValue( address, register, 1, sleep );
			return result != null && result[ 0 ];
		}

		/// <summary>
		/// Запрос 1 дискретного входа ( 0х02 )
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Запрашиваемый регистр</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Значение логического регистра</returns>
		public bool GetOneDiscreteInputValue( byte address, int register, int sleep )
		{
			bool[] result = GetDiscreteInputValue( address, register, 1, sleep );
			return result != null && result[ 0 ];
		}
		 
		/// <summary>
		/// Запрос 1 регистра ввода ( 0х04 )
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Запрашиваемый регистр</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Значение регистра</returns>
		public int GetOneInputValue( byte address, int register, int sleep )
		{
			int[] result = GetInputValue( address, register, 1, sleep );
			return result != null ? result[ 0 ] : 0;
		}

		/// <summary>
		/// Запрос 1 регистра хранения ( 0х03 )
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Запрашиваемый регистр</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Значение регистра</returns>
		public int GetOneHoldingValue( byte address, int register, int sleep )
		{
			int[] result = GetHoldingValue( address, register, 1, sleep );
			return result != null ? result[ 0 ] : 0;
		}

		/// <summary>
		/// Запрос coil-регистров ( 0х01 )
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Стартовый регистр</param>
		/// <param name="len">Требуемое число регистров</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Набор регистров</returns>
		public bool[] GetCoilValue( byte address, int register, ushort len, int sleep )
		{
			int[] values = GetValue( address, READMANY_COIL, register, len, sleep );
			return values != null ? ConvertToFlagValues( len, values ) : null;
		}

		/// <summary>
		/// Запрос дискретных входов ( 0х02 )
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Стартовый регистр</param>
		/// <param name="len">Требуемое число регистров</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Набор регистров</returns>
		public bool[] GetDiscreteInputValue( byte address, int register, ushort len, int sleep )
		{
			int[] values = GetValue( address, READMANY_DISCRETEINPUTS, register, len, sleep );
			return values != null ? ConvertToFlagValues( len, values ) : null;
		}

		/// <summary>
		/// Запрос регистров ввода ( 0х04 )
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Стартовый регистр</param>
		/// <param name="len">Требуемое число регистров</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Набор регистров</returns>
		public int[] GetInputValue( byte address, int register, ushort len, int sleep )
		{
			return GetValue( address, READMANY_INPUT, register, len, sleep );
		}

		/// <summary>
		/// Запрос регистров хранения ( 0х03 )
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Стартовый регистр</param>
		/// <param name="len">Требуемое число регистров</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Набор регистров</returns>
		public int[] GetHoldingValue( byte address, int register, ushort len, int sleep )
		{
			return GetValue( address, READMANY_HOLDING, register, len, sleep );
		}

		/// <summary>
		/// Запрос float-значения из регистра
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="type">Отправленная на устройство функция</param>
		/// <param name="register">Запрашиваемый регистр</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Float-число</returns>
		public float GetFloat( byte address, byte type, int register, int sleep )
		{
			int bytes = 0;
			byte[] result = GetClearAnswer( address, type, register, 2, sleep, out bytes );
			if ( result == null )
			{
				Console.WriteLine( "It's got null float" );
				return 0;
			}
			return ConvertToFloatValue( result, ANSWER_START );
		}

		public float GetSwFloat( byte address, byte type, int register, int sleep )
		{
			int bytes = 0;
			byte[] result = GetClearAnswer( address, type, register, 2, sleep, out bytes );
			if ( result == null )
			{
				Console.WriteLine( "It's got null float" );
				return 0;
			}
			return ConvertToSwFloatValue( result, ANSWER_START );
		}

		/// <summary>
		/// Запрос float-значения из регистров хранения
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Запрашиваемый регистр</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Float-число</returns>
		public float GetHoldingFloat( byte address, int register, int sleep )
		{
			return GetFloat( address, READMANY_HOLDING, register, sleep );
		}

		public float GetHoldingSwFloat( byte address, int register, int sleep )
		{
			return GetSwFloat( address, READMANY_HOLDING, register, sleep );
		}

		/// <summary>
		/// Запрос float-значения из регистров ввода
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Запрашиваемый регистр</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Float-число</returns>
		public float GetInputSwFloat( byte address, int register, int sleep )
		{
			return GetSwFloat( address, READMANY_INPUT, register, sleep );
		}

		/// <summary>
		/// Запрос float-значения из регистров ввода
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Запрашиваемый регистр</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Float-число</returns>
		public float GetInputFloat( byte address, int register, int sleep )
		{
			return GetFloat( address, READMANY_INPUT, register, sleep );
		}

		/// <summary>
		/// Получение чистого ответа устройства (без убирания доп. части пакета)
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="funct">Отправленная на устройство функция</param>
		/// <param name="register">Стартовый регистр</param>
		/// <param name="len">Число регистров</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <param name="bytes">Длина полученного ответа</param>
		/// <returns>Ответ устрйоства</returns>
		public virtual byte[] GetClearAnswer( byte address, byte funct, int register, ushort len, int sleep, out int bytes )
		{
			bytes = 0;
			return null;
		}

		/// <summary>
		/// Запрос значений с устройства
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="type">Отправленная на устройство функция</param>
		/// <param name="register">Стартовый регистр</param>
		/// <param name="len">Число регистров</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Набор значений регистров</returns>
		public int[] GetValue( byte address, byte type, int register, ushort len, int sleep )
		{
			int bytes;
			int answerLen = CountAnswerLength( type, len );
			byte[] answer = GetClearAnswer( address, type, register, len, sleep, out bytes );
			return ConvertBytes( answer, type, answerLen, bytes );
		}

		/// <summary>
		/// Установка coil-регистра
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Изменяемый регистр</param>
		/// <param name="value">Отправляемый флаг</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Ответ устройства на операцию записи</returns>
		public bool SetFlag( byte address, int register, bool value, int sleep )
		{
			int val = value ? WRITEVALUE_TRUE : WRITEVALUE_FALSE;
			return SetValue( address, WRITEONE_FLAG, register, val, sleep );
		}

		/// <summary>
		/// Установка регистра хранения
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="register">Изменяемый регистр</param>
		/// <param name="value">Отправляемое значение</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Успешность операции</returns>
		public bool SetValue( byte address, int register, int value, int sleep )
		{
			return SetValue( address, WRITEONE_HOLDING, register, value, sleep );
		}

		// Установка значения на устройстве. Оставить или убрать?
		public bool SetValue( byte address, int register, int defRegister, int value, bool needToOpen, int sleep )
		{
			if ( needToOpen )
			{
				//открытие на запись
				if ( !SetValue( address, defRegister, 0xABCD, sleep ) )
				{
					return false;
				}
			}

			if ( !SetValue( address, register, value, sleep ) )
			{
				return false;
			}
			if ( needToOpen )
			{
				//переход в обычный режим
				if ( !SetValue( address, defRegister, 0, sleep ) )
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="register"></param>
		/// <param name="values"></param>
		/// <param name="sleep"></param>
		/// <returns></returns>
		public virtual bool SetMultipleValue( byte address, int register, byte[] values, int sleep )
		{
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="address"></param>
		/// <param name="register"></param>
		/// <param name="values"></param>
		/// <param name="sleep"></param>
		/// <returns></returns>
		public bool SetMultipleValue( byte address, int register, int[] values, int sleep )
		{
			byte[] registerPoints = ConvertToBytes( values );
			return SetMultipleValue( address, register, registerPoints, sleep );
		}

		public bool SetFloatValue( byte address, int register, float value, int sleep )
		{
			int[] tmp = new int[ 2 ];
			tmp[ 0 ] = (int) BitConverter.ToInt16( BitConverter.GetBytes( value ), 0 );
			tmp[ 1 ] = (int) BitConverter.ToInt16( BitConverter.GetBytes( value ), 2 );
			return SetMultipleValue( address, register, tmp, sleep );
		}

		public bool SetSwFloatValue( byte address, int register, float value, int sleep )
		{
			int[] tmp = new int[ 2 ];
			tmp[ 0 ] = (int) BitConverter.ToInt16( BitConverter.GetBytes( value ), 2 );
			tmp[ 1 ] = (int) BitConverter.ToInt16( BitConverter.GetBytes( value ), 0 );
			return SetMultipleValue( address, register, tmp, sleep );
		}

		/// <summary>
		/// ОТправка пакета записи
		/// </summary>
		/// <param name="address">Modbus-адрес устройства</param>
		/// <param name="type">Отправляемая на устройство функция</param>
		/// <param name="register">Изменяемый регистр</param>
		/// <param name="value">Отправляемое значение</param>
		/// <param name="sleep">Время ожидания ответа</param>
		/// <returns>Успешность операции</returns>
		public virtual bool SetValue( byte address, byte type, int register, int value, int sleep )
		{
			return true;
		}

	}
}
