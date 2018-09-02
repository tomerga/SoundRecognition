using System;

namespace SoundRecognition
{
     [Serializable]
     class MicrowaveItemInfo : IItemInfo
     {
          public string Barcode { get; private set; }
          public int MaxHittingTimeInSeconds { get; private set; }
          public string ProductName { get; private set; }
          public static MicrowaveItemInfo DefaultMicrowaveItem =
               new MicrowaveItemInfo("", MicrowaveMachine.MaximalWorkingTimeInMS, "Default Item");

          public MicrowaveItemInfo(string barcode, int hittingTimeInSec, string productName)
          {
               Barcode = barcode;
               MaxHittingTimeInSeconds = hittingTimeInSec;
               ProductName = productName;
          }
     }
}
