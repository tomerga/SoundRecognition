using System;
using System.IO;

namespace SoundRecognition
{
     // INFO: https://github.com/AddictedCS/soundfingerprinting

     // TODOs:
     // TODO more products - more recognize methods.
     // TODO investigate recording to computer + saving while recording (or somthing like that) and analyizing the saved data.

     // Less important.
     // TODO find out about higher HighPrecisionQueryConfiguration.
     // TODO investigate how fingerprint is built and try figure out if we can find out simmiilarities.
     // TODO SoundFingerprintingWrapper will get not only wav files

     // DONE:
     // TODO Write barcodes into file.
     // TODO binary file for microwave items data base.
     // TODO Write algorithem which identifies first pops and then start to think about the intervals.
     // MicrowaveItem type - according to tag (bar code), parameters of working time and safty accordingly. --> V
     // Start thinking about application run (safty and all that..). --> V
     // injection point to MicrowaveMachine - IRecognizer should be general. --> V
     // Load fingerprints data on application startup. --> V
     // cut from wav files the start - where there is no pops of popcorn. --> V
     // amplification. --> V
     // more popcorn sounds - to add to database. --> V

     class Program
     {
          static void Main(string[] args)
          {
               //RecordTest();
               //return;

               //SoundFingerprintingWrapper.Initialize();

               //foreach (string wavFile in Directory.GetFileSystemEntries(
               //     @"C:\Users\Dor Shaar\Desktop\Media - Backup",
               //     $"*{WavFile.Extension}",
               //     SearchOption.AllDirectories))
               //{
               //     SoundFingerprintingWrapper.StoreNewAudioFileData(new WavFile(wavFile));
               //}
               //return;

               MicrowaveMachine popcornMachine = new MicrowaveMachine();
               popcornMachine.TurnOn();
               popcornMachine.Run();
          }

          public static void UseWaveFile()
          {
               try
               {
                    foreach (string filePath in Directory.GetFiles(@"Resources\"))
                    {
                         try
                         {
                              WavFile waveFile = new WavFile(filePath);
                              //waveFile.CutFromEnd(waveFile.WaveFilePath, 7);
                              //waveFile.AddNoise(70);
                              waveFile.SplitWaveFileByInterval(10f);
                         }
                         catch (FileNotFoundException e)
                         {
                              throw e;
                         }
                         catch (FormatException e)
                         {
                              Console.WriteLine(e.Message);
                         }
                    }
               }
               catch (FileNotFoundException e)
               {
                    Console.WriteLine(e.Message);
               }
               catch (FormatException e)
               {
                    Console.WriteLine(e.Message);
               }
          }
     }
}
