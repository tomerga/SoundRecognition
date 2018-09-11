using System;

namespace SoundRecognition
{
     interface IRecognizerMachine
     {
          event EventHandler<RecognizerFinishedEventArgs> RecognizerFinished;
          void LoadProcessedData(string itemCategory);
          void ProcessNewData(IItemInfo item);
     }

     public enum eRecognitionStatus
     {
          Recognized = 0,
          UnRecognized = 0
     }
}
