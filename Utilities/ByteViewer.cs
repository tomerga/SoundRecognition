using System.Text;
using System.IO;

namespace PopcornTest
{
    public class ByteViewer
    {
        public static void PrintBytes(int byteStartIndex, int numberOfBytes, PrintMode printMode, FilePath filePath) // TODO improve.
        {
            using (Stream fileStream = new FileStream(filePath.FileFullPath, FileMode.Open))
            {
                byte[] bytes = new byte[numberOfBytes];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(bytes, 0, numberOfBytes);
                switch (printMode)
                {
                    case PrintMode.HexMode:
                        StringBuilder hexRepresentation = new StringBuilder(bytes.Length * 2);
                        foreach (byte oneByte in bytes)
                        {
                            hexRepresentation.AppendFormat("{0:x2} ", oneByte);
                        }

                        System.Console.WriteLine(hexRepresentation.ToString());
                        break;
                }
            }
        }

        public static void PrintBytes(int byteStartIndex, int numberOfBytes, PrintMode printMode, Stream fileStream)
        {
            byte[] bytes = new byte[numberOfBytes];
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.Read(bytes, 0, numberOfBytes);
            switch (printMode)
            {
                case PrintMode.HexMode:
                    StringBuilder hexRepresentation = new StringBuilder(bytes.Length * 2);
                    foreach (byte oneByte in bytes)
                    {
                        hexRepresentation.AppendFormat("{0:x2} ", oneByte);
                    }

                    System.Console.WriteLine(hexRepresentation.ToString());
                    break;
            }
        }

        public static void PrintBytes(byte[] bytes, PrintMode printMode)
        {
            switch (printMode)
            {
                case PrintMode.HexMode:
                    StringBuilder hexRepresentation = new StringBuilder(bytes.Length * 2);
                    foreach (byte oneByte in bytes)
                    {
                        hexRepresentation.AppendFormat("{0:x2} ", oneByte);
                    }

                    System.Console.WriteLine(hexRepresentation.ToString());
                    break;
            }
        }

        public enum PrintMode
        {
            DecimalMode = 1,
            BinaryMode = 2,
            HexMode = 4
        }
    }
}