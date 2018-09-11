using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SoundRecognition
{
     internal class PopsRecognizer : IRecognizerMachine
     {
          private static readonly int MS_IN_ONE_SECOND = 1000;
          private readonly int MAXIMAL_POP_INTERVAL_ALLOWED_IN_MS = 4 * MS_IN_ONE_SECOND;
          private readonly string RECORDS_DIRECTORY_NAME = "Records";

          private readonly int mAmplification = 10;
          private readonly double mSecondsToAnalyzeAudioFiles = 10;
          private Queue<IAudioFile> mSubSoundsQueue = new Queue<IAudioFile>();
          private bool mShouldStop = false;

          public event EventHandler<RecognizerFinishedEventArgs> RecognizerFinished;

          private enum ePopState
          {
               Unkown,
               BeforePeak,
               AfterPeak,
          }

          public PopsRecognizer(int amplification, int SecondsToAnalyzeAudioFiles)
          {
               mAmplification = amplification;
               mSecondsToAnalyzeAudioFiles = SecondsToAnalyzeAudioFiles;
          }

          public void LoadProcessedData(string itemCategory)
          {
               SoundFingerprintingWrapper.LoadFingerPrintsDataBase(itemCategory);
          }

          public void ProcessNewData(IItemInfo itemInfo)
          {
               using (Recorder recorder = new Recorder(RECORDS_DIRECTORY_NAME))
               using (FileSystemWatcher fileSystemWatcher = new FileSystemWatcher())
               {
                    fileSystemWatcher.Path = $"{RECORDS_DIRECTORY_NAME}"; // TODO validate - in linux should be fileSystemWatcher.Path = $"{Path.DirectorySeparatorChar}{RECORDS_DIRECTORY_NAME}";
                    //fileSystemWatcher.Filter = WavFile.WavFileExtension; // TODO uncomment.
                    fileSystemWatcher.EnableRaisingEvents = true;
                    fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;

                    /// Listening to records directory. In case new record created, inserts
                    /// it into <see cref="mSubSoundsQueue"/>
                    fileSystemWatcher.Created += new FileSystemEventHandler(OnFileOrDirectoryCreated);
                    fileSystemWatcher.Renamed += new RenamedEventHandler(OnFileOrDirectoryCreated);

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    new Thread(() => recorder.Record()).Start();

                    RunPopcornAlgorithm(stopwatch);
                    stopwatch.Stop();
               }
          }

          public void Stop()
          {
               mShouldStop = true;
          }

          private void RunPopcornAlgorithm(Stopwatch stopwatch)
          {
               ePopState popsState = ePopState.Unkown;
               List<long> popsRecognitionTimeInMS = new List<long> { 0L }; // Must add first element.
               long currentPopIntervalInMS, lastPopIntervalInMS = 0;

               /// ++ when pops intervals getting shorter, -- when pops intervals getting longer.
               int popGapCount = 0;

               /// In case detecting 5 times cosequently longer interval then before -
               /// changes to <see cref="ePopState.AfterPeak"/>".
               /// In case detecting 5 times cosequently longer interval then before -
               /// changes to <see cref="ePopState.BeforePeak"/>".
               int popGapDetect = 0;

               while (!mShouldStop)
               {
                    if (Recognize() == eRecognitionStatus.Recognized)
                    {
                         popsRecognitionTimeInMS.Add(stopwatch.ElapsedMilliseconds);
                         currentPopIntervalInMS =
                              popsRecognitionTimeInMS[popsRecognitionTimeInMS.Count - 1] -
                              popsRecognitionTimeInMS[popsRecognitionTimeInMS.Count - 2];

                         if (currentPopIntervalInMS <= lastPopIntervalInMS) // Pops more frequent.
                         {
                              popGapCount++;
                              if (popGapDetect > 0) { popGapDetect++; } else { popGapDetect = 0; }
                         }
                         else // Pops less frequent.
                         {
                              popGapCount--;
                              if (popGapDetect < 0) { popGapDetect--; } else { popGapDetect = 0; }
                         }

                         if (popGapDetect == 5)
                         {
                              popsState = ePopState.BeforePeak;
                         }
                         else if (popGapDetect == -5)
                         {
                              popsState = ePopState.AfterPeak;
                         }

                         Console.WriteLine(
$@"Pop recognized after: {popsRecognitionTimeInMS[popsRecognitionTimeInMS.Count - 1]}
Interval since last recognized pop: {currentPopIntervalInMS}ms
Prior interval: {lastPopIntervalInMS}ms
Current interval - prior interval: {currentPopIntervalInMS - lastPopIntervalInMS}ms
Pop gap count: {popGapCount}
Pop gap detect: {popGapDetect}");

                         lastPopIntervalInMS = currentPopIntervalInMS;

                         if ((currentPopIntervalInMS > MAXIMAL_POP_INTERVAL_ALLOWED_IN_MS) &&
                              popsState == ePopState.AfterPeak)
                         {
                              mShouldStop = true;
                              Console.WriteLine($"Popcorn algorithm should stop since reached maximal pop interval allowed {MAXIMAL_POP_INTERVAL_ALLOWED_IN_MS}");
                         }
                    }
               }
          }

          private void OnFileOrDirectoryCreated(object source, FileSystemEventArgs e)
          {
               Console.WriteLine($"File: {e.FullPath} created");
               mSubSoundsQueue.Enqueue(new WavFile(e.FullPath));
          }

          private void RecognizerShouldStop(object sender, RecognizerFinishedEventArgs e)
          {
               mShouldStop = false;
               e.Data = $"Recognizer should stop since detected finished";
               RecognizerFinished.Invoke(this, e);
          }

          private eRecognitionStatus Recognize()
          {
               eRecognitionStatus recognitionStatus = eRecognitionStatus.UnRecognized;
               if (mSubSoundsQueue.Count != 0)
               {
                    IAudioFile subSound = mSubSoundsQueue.Dequeue();
                    bool isMatchFound = SoundFingerprintingWrapper.FindMatchesForAudioFile(
                        subSound,
                        mAmplification,
                        mSecondsToAnalyzeAudioFiles);

                    if (isMatchFound)
                    {
                         recognitionStatus = eRecognitionStatus.Recognized;
                         SoundFingerprintingWrapper.StoreNewAudioFileData(subSound);
                    }
               }

               return recognitionStatus;
          }
     }
}
