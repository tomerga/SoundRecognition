using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SoundRecognition
{
     class PopsRecognizer : IRecognizerMachine
     {
          private readonly int AMPLIFICATION= 10;
          private readonly double SECONDS_TO_ANALYZE_AUDIO_FILES = 10;
          private static readonly int MS_IN_ONE_SECOND = 1000;
          private readonly int MAXIMAL_POP_INTERVAL_ALLOWED_IN_MS = 4 * MS_IN_ONE_SECOND;

          private readonly string RECORD_EXE_NAME = "Record.sh";
          private readonly string RECORDS_DIRECTORY_NAME = "Records";

          private Queue<IAudioFile> mSubSoundsQueue = new Queue<IAudioFile>();
          private bool mShouldStop { get; set; } = false;

          public event EventHandler<RecognizerFinishedEventArgs> RecognizerFinished;

          private enum ePopState
          {
               Unkown,
               BeforePeak,
               AfterPeak,
          }

          public void LoadProcessedData()
          {
               SoundFingerprintingWrapper.LoadFingerPrintsDataBase();
          }

          public void ProcessNewData()
          {
               Record();
          }

          public void Run(IItemInfo itemInfo)
          {
               ePopState popsState = ePopState.Unkown;
               List<long> popRecognitionTimeInMS = new List<long> { 0L }; // Must add first element.
               long currentPopIntervalInMS, lastPopIntervalInMS = 0;

               /// ++ when pops intervals getting shorter, -- when pops intervals getting longer.
               int popGapCount = 0;

               /// In case detecting 5 times cosequently longer interval then before -
               /// changes to <see cref="ePopState.AfterPeak"/>".
               /// In case detecting 5 times cosequently longer interval then before -
               /// changes to <see cref="ePopState.BeforePeak"/>".
               int popGapDetect = 0;

               Directory.CreateDirectory(RECORDS_DIRECTORY_NAME);
               using (FileSystemWatcher fileSystemWatcher = new FileSystemWatcher())
               {
                    fileSystemWatcher.Path = $"{Path.DirectorySeparatorChar}{RECORDS_DIRECTORY_NAME}";
                    fileSystemWatcher.Filter = WavFile.WavFileExtension;
                    fileSystemWatcher.EnableRaisingEvents = true;

                    /// Listening to records directory. In case new record created, insert
                    /// it into <see cref="mSubSoundsQueue"/>
                    fileSystemWatcher.Created += new FileSystemEventHandler(OnFileOrDirectoryCreated);

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    new Thread(() => ProcessNewData()).Start();

                    while (!mShouldStop)
                    {
                         if (Recognize() == eRecognitionStatus.Recognized)
                         {
                              popRecognitionTimeInMS.Add(stopwatch.ElapsedMilliseconds);
                              currentPopIntervalInMS =
                                   popRecognitionTimeInMS[popRecognitionTimeInMS.Count - 1] -
                                   popRecognitionTimeInMS[popRecognitionTimeInMS.Count - 2];

                              if(currentPopIntervalInMS <= lastPopIntervalInMS) // Pops more frequent.
                              {
                                   popGapCount++;
                                   if (popGapDetect > 0) { popGapDetect++; } else { popGapDetect = 0; }
                              }
                              else // Pops less frequent.
                              {
                                   popGapCount--;
                                   if (popGapDetect < 0) { popGapDetect--; } else { popGapDetect = 0; }
                              }

                              if(popGapDetect == 5)
                              {
                                   popsState = ePopState.BeforePeak;
                              }
                              else if(popGapDetect == -5)
                              {
                                   popsState = ePopState.AfterPeak;
                              }

                              Console.WriteLine(
$@"Pop recognized after: {popRecognitionTimeInMS[popRecognitionTimeInMS.Count - 1]}
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
                                   Console.WriteLine($"Popcorn machine should stop since reached maximal pop interval allowed {MAXIMAL_POP_INTERVAL_ALLOWED_IN_MS}");
                              }
                         }
                    }

                    stopwatch.Stop();
               }
          }

          public void Stop()
          {
               mShouldStop = true;
          }

          private void OnFileOrDirectoryCreated(object source, FileSystemEventArgs e)
          {
               Console.WriteLine($"File: {e.FullPath} created");
               mSubSoundsQueue.Enqueue(new WavFile(e.FullPath));
          }

          private void RecognizerShouldStop(object sender, RecognizerFinishedEventArgs e)
          {
               mShouldStop = false;
               e.data = $"Recognizer should stop since detected finished";
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
                        AMPLIFICATION,
                        SECONDS_TO_ANALYZE_AUDIO_FILES);

                    if (isMatchFound)
                    {
                         recognitionStatus = eRecognitionStatus.Recognized;
                         SoundFingerprintingWrapper.StoreNewAudioFileData(subSound);
                    }
               }

               return recognitionStatus;
          }

          /// <summary>
          /// The script for recording new .wav samples demands the next arguments:
          /// directoryPath of the output .wav files,
          /// secondsToRecordPerWavFile,
          /// </summary>
          private void Record()
          {
               int secondsToRecordPerWavFile = 3;
               string recordsDirectoryName = $"{Path.DirectorySeparatorChar}{RECORDS_DIRECTORY_NAME}";

               string[] argument = new string[]
               {
                    recordsDirectoryName,
                    secondsToRecordPerWavFile.ToString()
               };

               ProcessExecutor.ExecuteProcess(RECORD_EXE_NAME, argument);
          }
     }
}
