using System;

namespace SoundRecognition
{
    interface IRecorder : IDisposable
    {
          int RecordLengthInSec { get; set; }
          void InitializeRecorder(string recordsDirectory);
          void Record();
    }
}
