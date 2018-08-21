using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;

namespace PopcornTest
{
     public static class PrototypeMachine
     {
          public static T DeepClone<T>(this T i_ToClone) where T : class
          {
               using (Stream stream = new MemoryStream())
               {
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(stream, i_ToClone);
                    stream.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    return serializer.Deserialize(stream) as T;
               }
          }

          public static void ProtoSerialize<T>(T record, FilePath filePath) where T : class
          {
               if (record != null)
               {
                    try
                    {
                         using (Stream stream = new FileStream(filePath.FileFullPath,
                              FileMode.Create, FileAccess.Write))
                         {
                              Serializer.Serialize(stream, record);
                         }
                    }
                    catch (System.Exception e)
                    {
                         System.Console.WriteLine("Some error occured using ProtoSerialize");
                         System.Console.WriteLine(e.Message);
                    }
               }
          }

          public static T ProtoDeserialize<T>(FilePath filePath) where T : class
          {
               T objectToReturn = null;
               try
               {
                    using (Stream stream = new FileStream(filePath.FileFullPath,
                             FileMode.Open, FileAccess.Read))
                    {
                         objectToReturn = Serializer.Deserialize<T>(stream);
                    }
               }
               catch
               {
                    System.Console.WriteLine("Some error occured using ProtoDeserialize");
               }

               return objectToReturn;
          }
     }
}
