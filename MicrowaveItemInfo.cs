using System;

namespace SoundRecognition
{
     [Serializable]
     class MicrowaveItemInfo : IItemInfo
     {
          public string Barcode { get; private set; }
          public int MaxHittingTimeInSeconds { get; private set; }
          public string ItemName { get; private set; }
          public static MicrowaveItemInfo DefaultMicrowaveItem =
               new MicrowaveItemInfo("", MicrowaveMachine.MaximalWorkingTimeInMS, "Default Item");

          public MicrowaveItemInfo(string barcode, int hittingTimeInSec, string itemName)
          {
               Barcode = barcode;
               MaxHittingTimeInSeconds = hittingTimeInSec;
               ItemName = itemName;
          }
     }
}
