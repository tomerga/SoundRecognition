using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;

namespace SoundRecognition
{
     public static class SerializationMachine
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
                    catch (Exception e)
                    {
                         Console.WriteLine($"Some error occured using ProtoSerialize: {e.Message}");
                    }
               }
          }

          public static void ProtoSerialize<T>(T record, string filePath) where T : class
          {
               ProtoSerialize(record, FilePath.CreateFilePath(filePath));
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
               catch (Exception e)
               {
                    Console.WriteLine($"Some error occured using ProtoDeserialize: {e.Message}");
               }

               return objectToReturn;
          }

          public static void Serialize<T>(T item, string filePath) where T : class
          {
               Serialize(item, filePath, FileMode.Append);
          }

          public static void Serialize<T>(T item, string filePath, FileMode fileMode) where T : class
          {
               IFormatter formatter = new BinaryFormatter();
               using (Stream stream = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.None))
               {
                    formatter.Serialize(stream, item);
               }
          }

          public static object Deserialize(Stream stream)
          {
               IFormatter formatter = new BinaryFormatter();
               return formatter.Deserialize(stream);
          }

          /// <summary>
          /// At first serializes the size of the dictionary and then its content.
          /// It is very important that the serialize method from the stream will be at FileMode.Append.
          /// </summary>
          public static void SaveDictionaryIntoDB<T,K>(string dataBasePath, Dictionary<T,K> dictionaryToSave) where T : class where K : class
          {
               Serialize(dictionaryToSave.Count.ToString(), dataBasePath, FileMode.Create);
               foreach (KeyValuePair<T, K> keyPairValue in dictionaryToSave)
               {
                    Serialize(keyPairValue.Key, dataBasePath);
                    Serialize(keyPairValue.Value, dataBasePath);
               }

               Console.WriteLine($"Database saved to {dataBasePath}");
          }

          public static Dictionary<T,K> LoadDictionaryFromDB<T,K>(string databasePath)
          {
               Dictionary<T, K> dictionaryFromDB = new Dictionary<T, K>();

               try
               {
                    using (Stream stream = new FileStream(databasePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                         string sizeAsString = (string)Deserialize(stream);
                         int dictionarySize = int.Parse(sizeAsString);
                         for (int i = 0; i < dictionarySize; ++i)
                         {
                              T t = (T)Deserialize(stream);
                              K k = (K)Deserialize(stream);
                              dictionaryFromDB.Add(t, k);
                         }
                    }

                    Console.WriteLine($"Database loaded from {databasePath}");
               }
               catch (Exception e)
               {
                    Console.WriteLine($"Database was not loaded due to {e.Message}");
               }

               return dictionaryFromDB;
          }
     }
}
