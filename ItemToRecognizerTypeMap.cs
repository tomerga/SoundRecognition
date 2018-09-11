using System;
using System.Collections.Generic;
using System.IO;

namespace SoundRecognition
{
     internal class ItemToRecognizerTypeMap : IDatabaseHolder
     {
          private readonly string ITEMS_TO_RECOGNIZER_TYPE_MAP_NAME = "ItemToRecognizerTypeMap.bin";
          private readonly string DATABASE_DIRECTORY_NAME = "Database";
          private Dictionary<IItemInfo, string> mItemToRecognizerTypeDictionary = new Dictionary<IItemInfo, string>();

          internal static readonly string[] RecognizerType = new string[]
          {
               "UnkownRecognizer",         // Should be first as need to be default value
               "SpecificSoundRequired",
               "Popcorn"
          };

          public void LoadDatabase()
          {
               string itemsToRecognizerTypeMapDatabasePath =
                    Path.Combine(DATABASE_DIRECTORY_NAME, ITEMS_TO_RECOGNIZER_TYPE_MAP_NAME);
               mItemToRecognizerTypeDictionary = SerializationMachine.LoadDictionaryFromDB<IItemInfo, string>(itemsToRecognizerTypeMapDatabasePath);
          }

          public void SaveDatabase()
          {
               string itemsToRecognizerTypeDatabasePath = Path.Combine(DATABASE_DIRECTORY_NAME, ITEMS_TO_RECOGNIZER_TYPE_MAP_NAME);
               SerializationMachine.SaveDictionaryIntoDB(itemsToRecognizerTypeDatabasePath, mItemToRecognizerTypeDictionary);
          }

          public void ClassifyItemToRecognitionType(IItemInfo microwaveItemInfo)
          {
               Console.WriteLine($"Write which recognition type should handle {microwaveItemInfo.ItemName}");
               Console.WriteLine($"Type 1 for {RecognizerType[1]}");
               Console.WriteLine($"Type 2 for {RecognizerType[2]}");
               string recognitionType = Console.ReadLine();

               while ((recognitionType != "1") && (recognitionType != "2"))
               {
                    Console.WriteLine("Invalid input, type again");
                    recognitionType = Console.ReadLine();
               }

               switch (recognitionType)
               {
                    case "1":
                         mItemToRecognizerTypeDictionary.Add(microwaveItemInfo, RecognizerType[1]);
                         break;
                    case "2":
                         mItemToRecognizerTypeDictionary.Add(microwaveItemInfo, RecognizerType[2]);
                         break;
               }

               SaveDatabase();
          }

          public string GetRecognizerTypeByItem(IItemInfo item)
          {
               mItemToRecognizerTypeDictionary.TryGetValue(item, out string recognizerType);
               return recognizerType;
          }
     }
}
