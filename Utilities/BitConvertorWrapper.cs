using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundRecognition
{
    public class BitConvertorWrapper
    {
        public static byte[] ConvertIntegerToByteArray(object integer, IntType intType)
        {
            byte[] intBytes = null;

            switch (intType)
            {
                case IntType.Int16:
                    intBytes = BitConverter.GetBytes((short)integer);
                    break;
                case IntType.Int32:
                    intBytes = BitConverter.GetBytes((int)integer);
                    break;
                case IntType.Int64:
                    intBytes = BitConverter.GetBytes((long)integer);
                    break;
            }

            return intBytes;
        }

        public static object ConvertByteArrayToInt(byte[] byteArr, IntType intType)
        {
            object integer = null;
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArr);
            }

            switch(intType)
            {
                case IntType.Int16:
                    integer = BitConverter.ToInt16(byteArr, 0);
                    break;
                case IntType.Int32:
                    integer = BitConverter.ToInt32(byteArr, 0);
                    break;
                case IntType.Int64:
                    integer = BitConverter.ToInt64(byteArr, 0);
                    break;
            }

            return integer;
        }

        public enum IntType
        {
            Int16 = 1,
            Int32 = 2,
            Int64 = 4
        }
    }
}
