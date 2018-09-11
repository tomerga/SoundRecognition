using System;
using System.IO;

namespace SoundRecognition
{
     // INFO: https://github.com/AddictedCS/soundfingerprinting

     // Test:
     // Test popRecognizer.
     // Test specificSoundsRecognizer.
     // Test directory listener.
     // Test maximal Microwave time (set it to 30 seconds.. see if everything is OK when record is aborted).

     // TODOs:
     // Fix saving and loading fingerprints.
     // TODO investigate recording to computer + saving while recording (or somthing like that) and analyizing the saved data.

     // Less important.
     // TODO find out about higher HighPrecisionQueryConfiguration.
     // TODO investigate how fingerprint is built and try figure out if we can find out simmiilarities.
     // TODO SoundFingerprintingWrapper will get not only wav files

     // DONE:
     // More products <=> more recognize methods.
     // Save DBs.
     // Write barcodes into file.
     // binary file for microwave items data base.
     // Write algorithem which identifies first pops and then start to think about the intervals.
     // MicrowaveItem type - according to tag (bar code), parameters of working time and safty accordingly.
     // Start thinking about application run (safty and all that..).
     // injection point to MicrowaveMachine - IRecognizer should be general.
     // Load fingerprints data on application startup.
     // Cut from wav files the start - where there is no pops of popcorn.
     // Amplification.
     // More popcorn sounds - to add to database.

     class Program
     {
          static void Main(string[] args)
          {
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
