
namespace SoundRecognition
{
     interface IRecognizerMachine
     {
          void LoadProcessedData();
          void ProcessNewData();
          void Run(IItemInfo item);
     }

     public enum eRecognitionStatus
     {
          Recognized = 0,
          UnRecognized = 0
     }
}
