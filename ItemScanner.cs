using System;
using System.Collections.Generic;
using System.IO;

namespace SoundRecognition
{
     class ItemScanner : IScanner
     {
          private readonly string ITEMS_DATA_BASE_NAME = "ItemsDB.bin";
          private readonly string BARCODE_DIRECTORY_NAME = "Barcodes";
          private readonly string CREATE_NEW_QR_BARCODE_EXE_NAME = "CreateQRBarcode.sh";
          private readonly string SCAN_BARCODE_EXE_NAME = "ScanQRBarcode.sh";
          private readonly string SUB_STRING_TO_SKIP = "QR-Code:";
          private readonly string PNG_EXTENSION = ".png";
          private readonly string TXT_EXTENSION = ".txt";

          private Dictionary<string, MicrowaveItemInfo> mMicrowaveItemsDictionary =
               new Dictionary<string, MicrowaveItemInfo>
          {
            //{"TelmushPopcornXYZ", new MicrowaveItemInfo("TelmushPopcornXYZ", 300, "Telmush Popcorn")},
            //{"NestlushPopcornABC", new MicrowaveItemInfo("NestlushPopcornABC", 280, "Nestlush Popcorn")},
            //{"Metildazzz", new MicrowaveItemInfo("Metildazzz", 190, "Matilda's Home Made Popcorn")}
          };

          public void Initialize()
          {
               using (Stream stream = new FileStream(ITEMS_DATA_BASE_NAME, FileMode.Open, FileAccess.Read, FileShare.None))
               {
                    string sizeAsString = (string)SerializationMachine.Deserialize(stream);
                    int dictionarySize = int.Parse(sizeAsString);
                    for (int i = 0; i < dictionarySize; ++i)
                    {
                         MicrowaveItemInfo itemInfo = 
                              (MicrowaveItemInfo)SerializationMachine.Deserialize(stream);
                         mMicrowaveItemsDictionary.Add(itemInfo.Barcode, itemInfo);
                    }
               }

               Console.WriteLine("Database loaded");
          }

          public IItemInfo Scan()
          {
               MicrowaveItemInfo microwaveItem = MicrowaveItemInfo.DefaultMicrowaveItem;
               string barcode = null;

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
                         break;
               }

               if (barcode == null)
               {
                    Console.WriteLine($"No barcode scanned");
               }
               else if (!mMicrowaveItemsDictionary.TryGetValue(barcode, out microwaveItem))
               {
                    Console.WriteLine($"{barcode} is not valid barcode");
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
          private MicrowaveItemInfo CreateNewBarcode()
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

               MicrowaveItemInfo microwaveItemInfo = new MicrowaveItemInfo(
                    stringToEncode, maxHittingTimeInSeconds, productName);
               mMicrowaveItemsDictionary.Add(stringToEncode, microwaveItemInfo);

               Console.WriteLine($"{microwaveItemInfo.ProductName} added to data base");

               SaveMicrowaveItems();

               return microwaveItemInfo;
          }

          /// <summary>
          /// At first serializes the size of the dictionary and then its content.
          /// </summary>
          private void SaveMicrowaveItems()
          {
               SerializationMachine.Serialize(
                    mMicrowaveItemsDictionary.Count.ToString(), ITEMS_DATA_BASE_NAME, FileMode.Create);

               foreach (KeyValuePair<string, MicrowaveItemInfo> keyPairValue in mMicrowaveItemsDictionary)
               {
                    SerializationMachine.Serialize(keyPairValue.Value, ITEMS_DATA_BASE_NAME);
               }

               Console.WriteLine($"Data base saved to {ITEMS_DATA_BASE_NAME}");
          }

          private void ShowBarcodesAvailable()
          {
               if (Directory.Exists(BARCODE_DIRECTORY_NAME))
               {
                    Console.WriteLine("Barcodes available:");
                    foreach (string qrBarcodeImageName in Directory.GetFiles(BARCODE_DIRECTORY_NAME))
                    {
                         Console.WriteLine(qrBarcodeImageName);
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
          private MicrowaveItemInfo ScanExistingBarcode()
          {
               MicrowaveItemInfo microwaveItem = null;

               Console.WriteLine("Type name of QR-Barcode image to scan");
               string barcodeImageName = Console.ReadLine();

               string[] argument = new string[] { barcodeImageName };

               ProcessExecutor.ExecuteProcess(SCAN_BARCODE_EXE_NAME, argument);

               string imageDirectoryName = $"{Path.DirectorySeparatorChar}{BARCODE_DIRECTORY_NAME}";
               string barcodeTextFilePath =
                    Path.Combine(imageDirectoryName, barcodeImageName.Replace(PNG_EXTENSION, TXT_EXTENSION));

               if(File.Exists(barcodeTextFilePath))
               {
                    using (StreamReader streamReader = new StreamReader(barcodeTextFilePath))
                    {
                         string line = streamReader.ReadLine();
                         int indexToReadFrom = line.LastIndexOf(SUB_STRING_TO_SKIP);
                         if(indexToReadFrom != -1)
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
