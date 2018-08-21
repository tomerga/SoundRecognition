using System;
using System.Diagnostics;
using System.Threading;

namespace PopcornTest
{
     class MicrowaveMachine
     {
          private IRecognizerMachine mPopsRecognizer;
          private IScanner mScanner = new ItemScanner();
          private static readonly int mMSInOneSecond = 1000;
          private static readonly int mMaximalWorkingTimeInMS = 360 * mMSInOneSecond; // 6 minutes.

          private MicrowaveItemInfo mMicrowaveItemInfo = MicrowaveItemInfo.MicrowaveItemsDictionary["1"]; // TODO start as null.
          public MachineStatus Status { get; private set; }

          internal enum MachineStatus
          {
               TurnedOff = 0,
               OnAndNotWorking = 1,
               OnAndWorking = 2,
               OnAndShouldStop = 4
          }

          private void ShowManu()
          {
               Console.WriteLine(@"Choose an option:
1. Scan item
2. Start microwave");
          }

          private void ScanItem()
          {
               mMicrowaveItemInfo = mScanner.Scan() as MicrowaveItemInfo;
          }

          private void SetShouldStopStatus()
          {
               Status = MachineStatus.OnAndShouldStop;
               Console.WriteLine("Recognizer machine should stop");
          }

          private void SetShouldStopStatus(object sender, RecognizerFinishedEventArgs e)
          {
               SetShouldStopStatus();
               Console.WriteLine(e.data);
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

               if (mMicrowaveItemInfo == null)
               {
                    StopMachine();
                    return;
               }

               int maxHeatingTimeAllowedInMS = Math.Min(
                   mMicrowaveItemInfo.HittingTimeInSeconds * mMSInOneSecond,
                   mMaximalWorkingTimeInMS);

               Stopwatch stopwatch = new Stopwatch();
               stopwatch.Start();
               Console.WriteLine("Recognizer machine working..");

               Thread popRecognizeThread = new Thread(() => mPopsRecognizer.Run(mMicrowaveItemInfo));
               popRecognizeThread.Start();

               while (Status != MachineStatus.OnAndShouldStop)
               {
                    if (stopwatch.ElapsedMilliseconds >= maxHeatingTimeAllowedInMS)
                    {
                         (mPopsRecognizer as PopsRecognizer).ShouldStop = true;
                         SetShouldStopStatus();
                         Console.WriteLine($"Recognizer machine should stop since reached maximal working time allowed {maxHeatingTimeAllowedInMS}");
                    }
               }

               stopwatch.Stop();
               StopMachine();
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
                              ScanItem();
                              break;
                         case "start":
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
               Console.WriteLine("Recognizer machine turned on");
               mPopsRecognizer = new PopsRecognizer();
               (mPopsRecognizer as PopsRecognizer).RecognizerFinished += SetShouldStopStatus;
               SoundFingerprintingWrapper.Initialize();
               mPopsRecognizer.LoadProcessedData();
          }

          public void TurnOff()
          {
               if (Status != MachineStatus.TurnedOff)
               {
                    Status = MachineStatus.TurnedOff;
                    Console.WriteLine("Recognizer machine turned off");
               }
          }
     }
}
