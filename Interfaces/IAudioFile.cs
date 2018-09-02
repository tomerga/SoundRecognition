
namespace SoundRecognition
{
     interface IAudioFile
     {
          FilePath FilePath { get; }
          int DurationInSeconds { get; }
     }
}
