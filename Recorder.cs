using System.IO;

namespace SoundRecognition
{
     internal class Recorder : IRecorder
     {
          private readonly string RECORD_EXE_NAME = "Record.sh";
          private string mRecordsDirectory;

          public int RecordLengthInSec { get; set;} = 3;

          public Recorder(string recordsDirectory)
          {
               InitializeRecorder(recordsDirectory);
          }

          public void InitializeRecorder(string recordsDirectory)
          {
               mRecordsDirectory = recordsDirectory;
               System.Console.WriteLine($"Creating directory: {mRecordsDirectory}");
               Directory.CreateDirectory(mRecordsDirectory);
          }

          /// <summary>
          /// The script for recording new .wav samples demands the next arguments:
          /// directoryPath of the output .wav files,
          /// secondsToRecordPerWavFile,
          /// </summary>
          public void Record()
          {
               string recordsDirectoryName = $"{Path.DirectorySeparatorChar}{mRecordsDirectory}";

               string[] argument = new string[]
               {
                    recordsDirectoryName,
                    RecordLengthInSec.ToString()
               };

               //ProcessExecutor.ExecuteProcess(RECORD_EXE_NAME, argument); // TODO uncomment.

               // TODO delete from here.
               string path = @"C:\Users\Dor Shaar\Desktop\splitted_cut_DorMaximovPopcorn";
               string[] files = Directory.GetFiles(path);
               System.Collections.Queue queue = new System.Collections.Queue(files);

               System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
               stopwatch.Start();
               while (queue.Count > 0)
               {
                    if(stopwatch.ElapsedMilliseconds >= 3000)
                    {
                         FilePath filePath = FilePath.CreateFilePath((string)queue.Dequeue());
                         File.Move(filePath.FileFullPath, $@"{mRecordsDirectory}\{filePath.Name}");
                         System.Console.WriteLine($"Copied:{filePath.FileFullPath}");
                         stopwatch.Restart();
                    }
               }

               // TODO delete to here.
          }

          public void Dispose()
          {
               System.Console.WriteLine($"Deleting directory: {mRecordsDirectory}");
               Directory.Delete(mRecordsDirectory, true);
          }
     }
}
