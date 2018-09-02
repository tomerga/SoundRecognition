
namespace SoundRecognition
{
     class BitOperations
     {
          /// <summary>
          /// byteIndex shuold be between 0-7. Otherwise, return byteToToggle as it was.
          /// The 0 index is the LSB.
          /// </summary>
          /// <param name="byteToToggle"></param>
          /// <param name="bitIndex"></param>
          /// <returns></returns>
          public static byte ToggleNBit(byte byteToToggle, int bitIndex)
          {
               byte toggledByte = byteToToggle;
               if ((bitIndex >= 0) || (bitIndex <= 7))
               {
                    byte mask = 0x01;                             // 00000001.
                    mask <<= bitIndex;
                    byte extractedBit;
                    byte toggledBit;

                    extractedBit = (byte)(mask & byteToToggle);   // 0000000x.
                    toggledBit = (byte)(extractedBit ^ mask);     // 0000000y.
                    toggledByte |= toggledBit;
               }

               return toggledByte;
          }
     }
}
