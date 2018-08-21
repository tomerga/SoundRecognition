using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PopcornTest
{
     class PopsRecognizer : IRecognizerMachine
     {
          private int mAmplification = 10;
          private double mSecondsToAnalyzeAudioFiles = 10;
          private static readonly int mMSInOneSecond = 1000;
          private readonly int mMaximalPopIntervalAllowedInMS = 4 * mMSInOneSecond;

          private Queue<IAudioFile> mSubSoundsQueue = new Queue<IAudioFile>();
          public bool ShouldStop { get; set; } = false;
          public event EventHandler<RecognizerFinishedEventArgs> RecognizerFinished;

          private enum PopState
          {
               Unknown,
               BeforePeak,
               AfterPeak,
          }

          private void RecognizerShouldStop(object sender, RecognizerFinishedEventArgs e)
          {
               ShouldStop = false;
               e.data = $"Recognizer should stop since detected finished";
               RecognizerFinished.Invoke(this, e);
          }

          private RecognitionStatus Recognize()
          {
               RecognitionStatus recognitionStatus = RecognitionStatus.UnRecognized;
               if (mSubSoundsQueue.Count != 0)
               {
                    IAudioFile subSound = mSubSoundsQueue.Dequeue();
                    bool isMatchFound = SoundFingerprintingWrapper.FindMatchesForAudioFile(
                        subSound,
                        mAmplification,
                        mSecondsToAnalyzeAudioFiles);

                    if (isMatchFound)
                    {
                         recognitionStatus = RecognitionStatus.Recognized;
                         SoundFingerprintingWrapper.StoreNewAudioFileData(subSound);
                    }
               }

               return recognitionStatus;
          }

          private void Record()
          {
               // TODO insert audioFiles into queue.
               Stopwatch stopwatch = new Stopwatch();
               stopwatch.Start();

               while (true)
               {
                    if(stopwatch.ElapsedMilliseconds > 2000)
                    {
                         Console.WriteLine("Recording..");
                         stopwatch.Restart();
                    }
               }
          }

          public void LoadProcessedData()
          {
               SoundFingerprintingWrapper.LoadFingerPrintsDataBase();
          }

          public void ProcessNewData()
          {
               Record();
          }

          public void Run(IItemInfo itemInfo) // TODO improve stop algorithm
          {
               PopState popState = PopState.Unknown;
               List<long> popRecognitionTimeInMS = new List<long> { 0L }; // Should add first element.
               long currentPopIntervalInMS;

               Stopwatch stopwatch = new Stopwatch();
               stopwatch.Start();

               new Thread(() => ProcessNewData()).Start();
               while(!ShouldStop)
               {
                    if (Recognize() == RecognitionStatus.Recognized)
                    {
                         popRecognitionTimeInMS.Add(stopwatch.ElapsedMilliseconds);
                         currentPopIntervalInMS =
                              popRecognitionTimeInMS[popRecognitionTimeInMS.Count - 1] -
                              popRecognitionTimeInMS[popRecognitionTimeInMS.Count - 2];

                         if ((currentPopIntervalInMS > mMaximalPopIntervalAllowedInMS) &&
                              popState == PopState.AfterPeak)
                         {
                              ShouldStop = true;
                              Console.WriteLine($"Popcorn machine should stop since reached maximal pop interval allowed {mMaximalPopIntervalAllowedInMS}");
                         }
                    }
               }

               stopwatch.Stop();
          }
     }
}
