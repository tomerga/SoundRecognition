
namespace PopcornTest
{
     interface IRecognizerMachine
     {
          void LoadProcessedData();
          void ProcessNewData();
          void Run(IItemInfo item);
     }

     public enum RecognitionStatus
     {
          Recognized = 0,
          UnRecognized = 0
     }
}
