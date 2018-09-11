
namespace SoundRecognition
{
     internal class MachineRecognizerFactory
     {
          internal static IRecognizerMachine CreateRecognizer(string recognizerType, int amplification, int secondsToAnalyzeAudioFiles)
          {
               IRecognizerMachine recognizerMachine = null;
               if(recognizerType == ItemToRecognizerTypeMap.RecognizerType[2])
               {
                    recognizerMachine = new PopsRecognizer(amplification, secondsToAnalyzeAudioFiles);
               }
               else if (recognizerType == ItemToRecognizerTypeMap.RecognizerType[1])
               {
                    recognizerMachine = new SpecificSoundRecognizer(amplification, secondsToAnalyzeAudioFiles);
               }

               return recognizerMachine;
          }
     }
}
