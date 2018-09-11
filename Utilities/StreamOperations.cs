using System.IO;
using System.Text;

namespace SoundRecognition
{
     public class StreamOperations
     {
          public static void WriteStringToStream(Stream stream, string stringToWrite)
          {
               stream.Write(Encoding.UTF8.GetBytes(stringToWrite), 0, stringToWrite.Length);
               //ByteViewer.PrintBytes(Encoding.UTF8.GetBytes(stringToWrite), ByteViewer.PrintMode.HexMode);
          }

          public static void WriteIntegerToStream(
              Stream stream, object integer, BitConvertorWrapper.IntType intType)
          {
               byte[] intBytes = BitConvertorWrapper.ConvertIntegerToByteArray(integer, intType);
               stream.Write(intBytes, 0, intBytes.Length);
               //ByteViewer.PrintBytes(intBytes, ByteViewer.PrintMode.HexMode);
          }

          public static void CreateTextFile(string filePath, string textToWrite)
          {
               using (Stream stream = new FileStream(filePath, FileMode.Create))
               {
                    byte[] stringBytes = new UTF8Encoding(true).GetBytes(textToWrite);
                    stream.Write(stringBytes, 0, stringBytes.Length);
               }
          }

          public static void WriteBytesToStream(Stream stream, byte[] data)
          {
               stream.Write(data, 0, data.Length);
          }
     }
}
