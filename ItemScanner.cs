using System;
using System.Collections.Generic;
using System.IO;

namespace SoundRecognition
{
     internal class ItemScanner : IScanner, IDatabaseHolder
     {
          private readonly string DATABASE_DIRECTORY_NAME = "Database";
          private readonly string ITEMS_DATA_BASE_NAME = "ItemsDB.bin";
          private readonly string BARCODE_DIRECTORY_NAME = "Barcodes";
          private readonly string CREATE_NEW_QR_BARCODE_EXE_NAME = "CreateQRBarcode.sh";
          private readonly string SCAN_BARCODE_EXE_NAME = "ScanQRBarcode.sh";
          private readonly string SUB_STRING_TO_SKIP = "QR-Code:";
          private readonly string PNG_EXTENSION = ".png";
          private readonly string TXT_EXTENSION = ".txt";

          private Dictionary<string, IItemInfo> mMicrowaveItemsDictionary =
               new Dictionary<string, IItemInfo>
               {
                    //{"TelmushPopcornXYZ", new MicrowaveItemInfo("TelmushPopcornXYZ", 300, "Telmush Popcorn")},
                    //{"NestlushPopcornABC", new MicrowaveItemInfo("NestlushPopcornABC", 280, "Nestlush Popcorn")},
                    //{"Metildazzz", new MicrowaveItemInfo("Metildazzz", 190, "Matilda's Home Made Popcorn")}
               };

          private ItemToCategoryMap mItemToCategoryMap = new ItemToCategoryMap();
          public ItemToRecognizerTypeMap ItemToRecognizerTypeMap = new ItemToRecognizerTypeMap();

          public void Initialize()
          {
               LoadDatabase();
          }

          public void LoadDatabase()
          {
               string itemsDatabasePath = Path.Combine(DATABASE_DIRECTORY_NAME, ITEMS_DATA_BASE_NAME);
               mMicrowaveItemsDictionary = SerializationMachine.LoadDictionaryFromDB<string, IItemInfo>(itemsDatabasePath);
               mItemToCategoryMap.LoadDatabase();
               ItemToRecognizerTypeMap.LoadDatabase();
          }

          public void SaveDatabase()
          {
               string itemsDatabasePath = Path.Combine(DATABASE_DIRECTORY_NAME, ITEMS_DATA_BASE_NAME);
               SerializationMachine.SaveDictionaryIntoDB(itemsDatabasePath, mMicrowaveItemsDictionary);
          }

          public IItemInfo Scan()
          {
               IItemInfo microwaveItem = null;

               ShowScanManu();
               ShowBarcodesAvailable();
               string userInput = Console.ReadLine();

               switch (userInput.ToLowerInvariant())
               {
                    case "create":
                    case "1":
                         microwaveItem = CreateNewBarcode();
                         break;
                    case "scan":
                    case "2":
                         microwaveItem = ScanExistingBarcode();
                         break;
                    case "exit":
                    default:
                         break;
               }

               return microwaveItem;
          }

          private void ShowScanManu()
          {
               Console.WriteLine(@"Choose an option:
1. Create New Barcode.
2. Scan Existing Barcode
");
          }

          /// <summary>
          /// The script for creating new QR Barcode demands the next arguments:
          /// directoryPath of the output image,
          /// imageName (without the png extension),
          /// pixelSize,
          /// stringToEncode
          /// </summary>
          private IItemInfo CreateNewBarcode()
          {
               Directory.CreateDirectory(BARCODE_DIRECTORY_NAME);
               string imageDirectoryName = $"{Path.DirectorySeparatorChar}{BARCODE_DIRECTORY_NAME}";
               int pixelSize = 10;

               Console.WriteLine("Type the name of the product to register");
               string productName = Console.ReadLine();

               Console.WriteLine("Type number of maximal saftey hitting time in seconds ");
               int maxHittingTimeInSeconds = int.Parse(Console.ReadLine());

               Console.WriteLine("Type name of new barcode image");
               string outputImageName = Console.ReadLine();

               Guid guid = Guid.NewGuid();
               Console.WriteLine($"Generating string to encode: {guid}");
               string stringToEncode = guid.ToString();

               string[] argument = new string[]
               {
                    imageDirectoryName,
                    outputImageName,
                    pixelSize.ToString(),
                    stringToEncode
               };

               ProcessExecutor.ExecuteProcess(CREATE_NEW_QR_BARCODE_EXE_NAME, argument);

               IItemInfo microwaveItemInfo = new MicrowaveItemInfo(
                    stringToEncode, maxHittingTimeInSeconds, productName);
               mMicrowaveItemsDictionary.Add(stringToEncode, microwaveItemInfo);
               Console.WriteLine($"{microwaveItemInfo.ItemName} added to database");

               mItemToCategoryMap.ClassifyItemToCategory(microwaveItemInfo);
               ItemToRecognizerTypeMap.ClassifyItemToRecognitionType(microwaveItemInfo);

               SaveDatabase();

               return microwaveItemInfo;
          }

          private void ShowBarcodesAvailable()
          {
               if (Directory.Exists(BARCODE_DIRECTORY_NAME))
               {
                    string[] files = Directory.GetFiles(BARCODE_DIRECTORY_NAME);
                    if (files.Length > 0)
                    {
                         Console.WriteLine("Barcodes available:");
                         foreach (string qrBarcodeImageName in files)
                         {
                              Console.WriteLine(qrBarcodeImageName);
                         }
                    }
                    else
                    {
                         Console.WriteLine("no barcodes available");
                    }
               }
               else
               {
                    Console.WriteLine("no barcodes available");
               }
          }

          /// <summary>
          /// The script for scanning QR Barcode demands the next argument: imageFileName.
          /// The script Creates .txt file with the same name as imageFileName.
          /// The .txt file contaings the next structure: "QR-Code: [decoded string]".
          /// After executing the script, reading the .txt file and extracts the [decoded string].
          /// </summary>
          /// <returns></returns>
          private IItemInfo ScanExistingBarcode()
          {
               IItemInfo microwaveItem = null;

               Console.WriteLine("Type name of QR-Barcode image to scan");
               string barcodeImageName = Console.ReadLine();

               string[] argument = new string[] { barcodeImageName };

               ProcessExecutor.ExecuteProcess(SCAN_BARCODE_EXE_NAME, argument);

               string imageDirectoryName = $"{Path.DirectorySeparatorChar}{BARCODE_DIRECTORY_NAME}";
               string barcodeTextFilePath =
                    Path.Combine(imageDirectoryName, barcodeImageName.Replace(PNG_EXTENSION, TXT_EXTENSION));

               if (File.Exists(barcodeTextFilePath))
               {
                    using (StreamReader streamReader = new StreamReader(barcodeTextFilePath))
                    {
                         string line = streamReader.ReadLine();
                         int indexToReadFrom = line.LastIndexOf(SUB_STRING_TO_SKIP);
                         if (indexToReadFrom != -1)
                         {
                              indexToReadFrom += SUB_STRING_TO_SKIP.Length + 1;
                         }

                         string decodedString = line.Substring(indexToReadFrom);
                         mMicrowaveItemsDictionary.TryGetValue(decodedString, out microwaveItem);
                    }
               }

               return microwaveItem;
          }
     }
}
