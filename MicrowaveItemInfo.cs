using System.Collections.Generic;

namespace PopcornTest
{
    class MicrowaveItemInfo : IItemInfo
    {
        public string Barcode { get; private set; }
        public int HittingTimeInSeconds { get; private set; }
        public string ProductName { get; private set; }

        public MicrowaveItemInfo(string barcode, int hittingTimeInSec, string productName)
        {
            Barcode = barcode;
            HittingTimeInSeconds = hittingTimeInSec;
            ProductName = productName;
        }

        internal static Dictionary<string, MicrowaveItemInfo> MicrowaveItemsDictionary = new Dictionary<string, MicrowaveItemInfo>
        {
            {"1", new MicrowaveItemInfo("1", 300, "Telmush Popcorn")},
            {"2", new MicrowaveItemInfo("2", 280, "Nestlush Popcorn")},
            {"3", new MicrowaveItemInfo("3", 190, "Matilda's Home Made Popcorn")}
        };
    }
}
