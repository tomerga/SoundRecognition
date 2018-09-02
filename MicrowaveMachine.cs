using System;
using System.Diagnostics;
using System.Threading;

namespace SoundRecognition
{
     class MicrowaveMachine
     {
          private static readonly int MS_IN_ONE_SECOND = 1000;
          public static readonly int MaximalWorkingTimeInMS = 360 * MS_IN_ONE_SECOND; // 6 minutes.

          private IRecognizerMachine mPopsRecognizer;
          private IScanner mScanner = new ItemScanner();

          private MicrowaveItemInfo mMicrowaveItemInfo = null;
          public MachineStatus Status { get; private set; }

          internal enum MachineStatus
          {
               TurnedOff = 0,
               OnAndNotWorking = 1,
               OnAndWorking = 2,
               OnAndShouldStop = 4
          }

          public void Run()
          {
               while (Status != MachineStatus.TurnedOff)
               {
                    ShowManu();

                    string userInput = Console.ReadLine();
                    switch (userInput.ToLowerInvariant())
                    {
                         case "scan":
                         case "1":
                              ScanItem();
                              break;
                         case "start":
                         case "2":
                              StartWorking();
                              break;
                         case "turn off":
                         case "exit":
                              TurnOff();
                              break;
                    }
               }
          }

          public void TurnOn()
          {
               Status = MachineStatus.OnAndNotWorking;

               mPopsRecognizer = new PopsRecognizer();
               (mPopsRecognizer as PopsRecognizer).RecognizerFinished += SetShouldStopStatus;

               SoundFingerprintingWrapper.Initialize();
               mPopsRecognizer.LoadProcessedData();

               (mScanner as ItemScanner).Initialize();

               Console.WriteLine("Recognizer machine turned on");
          }

          public void TurnOff()
          {
               if (Status != MachineStatus.TurnedOff)
               {
                    Status = MachineStatus.TurnedOff;
                    Console.WriteLine("Recognizer machine turned off");
               }
          }

          private void ShowManu()
          {
               Console.WriteLine(@"Choose an option:
1. Scan Item
2. Start Microwave");
          }

          private void ScanItem()
          {
               mMicrowaveItemInfo = mScanner.Scan() as MicrowaveItemInfo;
          }

          private void SetShouldStopStatus(object sender, RecognizerFinishedEventArgs e)
          {
               SetShouldStopStatus();
               Console.WriteLine(e.data);
          }

          private void SetShouldStopStatus()
          {
               Status = MachineStatus.OnAndShouldStop;
               Console.WriteLine("Recognizer machine should stop");
          }

          private void StopMachine()
          {
               mMicrowaveItemInfo = null;
               Status = MachineStatus.OnAndNotWorking;
               Console.WriteLine("Recognizer machine stopped");
          }

          private void StartWorking()
          {
               Status = MachineStatus.OnAndWorking;
               Console.WriteLine("Recognizer machine started");

               int maxHeatingTimeAllowedInMS = Math.Min(
                   mMicrowaveItemInfo.MaxHittingTimeInSeconds * MS_IN_ONE_SECOND,
                   MaximalWorkingTimeInMS);

               Stopwatch stopwatch = new Stopwatch();
               stopwatch.Start();
               Console.WriteLine("Recognizer machine working..");

               new Thread(() => mPopsRecognizer.Run(mMicrowaveItemInfo)).Start();

               while (Status != MachineStatus.OnAndShouldStop)
               {
                    if (stopwatch.ElapsedMilliseconds >= maxHeatingTimeAllowedInMS)
                    {
                         (mPopsRecognizer as PopsRecognizer).Stop();
                         SetShouldStopStatus();
                         Console.WriteLine($"Recognizer machine should stop since reached maximal working time allowed {maxHeatingTimeAllowedInMS}");
                    }
               }

               stopwatch.Stop();
               StopMachine();
          }
     }
}
