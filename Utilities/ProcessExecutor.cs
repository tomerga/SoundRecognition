using System;
using System.Diagnostics;
using System.Text;

namespace SoundRecognition
{
     internal class ProcessExecutor
     {
          public static void ExecuteProcess(string executablePath, string[] arguments)
          {
               StringBuilder argumentsBuilder = new StringBuilder();

               if(arguments != null)
               {
                    foreach (string argument in arguments)
                    {
                         argumentsBuilder.Append($"{argument} ");
                    }
               }

               ProcessStartInfo startInfo = new ProcessStartInfo
               {
                    CreateNoWindow = false,
                    UseShellExecute = true,
                    FileName = executablePath,
                    Arguments = argumentsBuilder.ToString(),
                    WindowStyle = ProcessWindowStyle.Hidden
               };

               // Start the process with the info we specified.
               // Call WaitForExit and then the using-statement will close.
               Console.WriteLine($"Start executing shell scrip {startInfo.FileName}");
               Console.WriteLine($"Arguments: {startInfo.Arguments}");
               Stopwatch stopwatch = new Stopwatch();
               stopwatch.Start();
               try
               {
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                         Console.WriteLine($"Waiting for {startInfo.FileName} to finish...");
                         exeProcess.WaitForExit();
                         stopwatch.Stop();
                         Console.WriteLine($"{startInfo.FileName} run for {stopwatch.ElapsedMilliseconds}ms");
                         Console.WriteLine();
                    }
               }
               catch
               {
                    Console.WriteLine($"ERROR: Cannot start {startInfo.FileName}");
               }
          }
     }
}
