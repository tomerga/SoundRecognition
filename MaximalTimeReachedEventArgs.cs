using System;

namespace SoundRecognition
{
    class RecognizerFinishedEventArgs : EventArgs
    {
        public string Data { get; set; }
    }
}
