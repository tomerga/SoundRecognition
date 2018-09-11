using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SoundRecognition
{
     internal class SpecificSoundRecognizer : IRecognizerMachine
     {
          private readonly string RECORDS_DIRECTORY_NAME = "Records";

          private readonly int mAmplification = 10;
          private readonly double mSecondsToAnalyzeAudioFiles = 10;
          private Queue<IAudioFile> mSubSoundsQueue = new Queue<IAudioFile>();
          private bool mShouldStop = false;

          public event EventHandler<RecognizerFinishedEventArgs> RecognizerFinished;

          public SpecificSoundRecognizer(int amplification, int SecondsToAnalyzeAudioFiles)
          {
               mAmplification = amplification;
               mSecondsToAnalyzeAudioFiles = SecondsToAnalyzeAudioFiles;
          }

          public void LoadProcessedData(string itemCategory)
          {
               SoundFingerprintingWrapper.LoadFingerPrintsDataBase(itemCategory);
          }

          public void ProcessNewData(IItemInfo item)
          {
               using (Recorder recorder = new Recorder(RECORDS_DIRECTORY_NAME))
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
                    new Thread(() => recorder.Record()).Start();

                    RunAlgorithm(stopwatch);
                    stopwatch.Stop();
               }
          }

          private void OnFileOrDirectoryCreated(object source, FileSystemEventArgs e)
          {
               Console.WriteLine($"File: {e.FullPath} created");
               mSubSoundsQueue.Enqueue(new WavFile(e.FullPath));
          }

          private void RunAlgorithm(Stopwatch stopwatch)
          {
               while (!mShouldStop)
               {
                    if (Recognize() == eRecognitionStatus.Recognized)
                    {
                         mShouldStop = true;
                         Console.WriteLine($"Algorithm should stop since identified suitable sound");
                    }
               }
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
