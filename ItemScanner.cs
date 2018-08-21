
namespace PopcornTest
{
    class ItemScanner : IScanner
    {
        public IItemInfo Scan()
        {
            string barcode = "1"; // Should get a barcode which will identify the item.
            
            return MicrowaveItemInfo.MicrowaveItemsDictionary[barcode]; 
        }
    }
}
