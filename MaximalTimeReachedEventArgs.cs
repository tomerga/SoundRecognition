using System;

namespace SoundRecognition
{
    class RecognizerFinishedEventArgs : EventArgs
    {
        public string data { get; set; }
    }
}
