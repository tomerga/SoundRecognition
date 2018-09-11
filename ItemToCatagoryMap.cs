using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoundRecognition
{
     internal class ItemToCategoryMap : IDatabaseHolder
     {
          private readonly string ITEMS_TO_Category_MAP_NAME = "ItemToCategoryMap.bin";
          private readonly string DATABASE_DIRECTORY_NAME = "Database";
          private Dictionary<IItemInfo, string> mItemToCategoryDictionary = new Dictionary<IItemInfo, string>();

          public void LoadDatabase()
          {
               string itemsToCategoryMapDatabasePath =
                    Path.Combine(DATABASE_DIRECTORY_NAME, ITEMS_TO_Category_MAP_NAME);
               mItemToCategoryDictionary = SerializationMachine.LoadDictionaryFromDB<IItemInfo, string>(itemsToCategoryMapDatabasePath);
          }

          public void SaveDatabase()
          {
               string itemsToCategoryMapDatabasePath =
                    Path.Combine(DATABASE_DIRECTORY_NAME, ITEMS_TO_Category_MAP_NAME);
               SerializationMachine.SaveDictionaryIntoDB(itemsToCategoryMapDatabasePath, mItemToCategoryDictionary);
          }

          public void ClassifyItemToCategory(IItemInfo microwaveItemInfo)
          {
               PrintCategories();
               Console.WriteLine($"Write which Category {microwaveItemInfo.ItemName} suits or write new Category");
               string Category = Console.ReadLine();
               string databaseCategoryDirectory = Path.Combine(DATABASE_DIRECTORY_NAME, Category);
               Directory.CreateDirectory(databaseCategoryDirectory);

               if (!mItemToCategoryDictionary.ContainsKey(microwaveItemInfo))
               {
                    mItemToCategoryDictionary.Add(microwaveItemInfo, Category);
               }

               SaveDatabase();
          }

          public string GetCategoryByItem(IItemInfo item)
          {
               mItemToCategoryDictionary.TryGetValue(item, out string Category);
               return Category;
          }

          private void PrintCategories()
          {
               Console.Write("Categories: ");

               if(mItemToCategoryDictionary.Count >= 0)
               {
                    foreach (string value in mItemToCategoryDictionary.Values.Distinct())
                    {
                         Console.Write($"{value}, ");
                    }
               }
               else
               {
                    Console.Write("{No cetagories}");
               }

               Console.WriteLine();
          }
     }
}
