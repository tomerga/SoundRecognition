using System;
using System.IO;
using System.Text;

namespace SoundRecognition
{
     class WavFile : IAudioFile
     {
          private readonly string RIFF_STR = "RIFF"; // ChunkID.
          private readonly string WAVE_STR = "WAVE"; // Format.
          private readonly string FMT_STR = "fmt "; // SubChunk1ID.
          private readonly string DATA_STR = "data"; // SubChunk2ID.
          public static readonly string WavFileExtension = ".wav";

          /// <summary>
          /// Size of the waveFile minus 8 (int bytes).
          /// Does not concludes ChunkID and ChunkSize.
          /// </summary>
          private int mChunkSize;

          /// <summary>
          /// Size of fmt chunk.
          /// Does not concludes the SubChunk1ID and SubChunk1Size.
          /// </summary>
          private int mSubChunk1Size;

          /// <summary>
          /// Value 1 for PCM.
          /// </summary>
          private short mAudioFormat;

          /// <summary>
          /// SubChunk2Size.
          /// </summary>
          public int DataSizeInBytes { get; private set; }
          public short NumChannels { get; private set; }
          public int SampleRate { get; private set; }
          public short BitsPerSample { get; private set; }
          public byte[] SoundData { get; private set; }
          public FilePath FilePath { get; private set; }
          public static string Extension { get; private set; } = "wav";

          public WavFile(FilePath wavFilePath)
          {
               LoadWaveFile(wavFilePath);
               FilePath = wavFilePath;
          }

          public WavFile(string wavFilePath)
          {
               FilePath filePath = FilePath.CreateFilePath(wavFilePath);
               LoadWaveFile(filePath);
               FilePath = filePath;
          }

          private void CheckExcpectedBytes(byte[] byteArr, string shouldBe)
          {
               string found = Encoding.UTF8.GetString(byteArr);
               if (shouldBe != found)
               {
                    throw new FormatException($"''{shouldBe}'' is missing or not in place. Instead found {found}");
               }
          }

          private void ReadRiffChunkDescriptor(Stream stream)
          {
               int riffChunkSize = 12;
               stream.Seek(0, SeekOrigin.Begin);
               byte[] fmtSubChunkBytes = new byte[riffChunkSize];
               stream.Read(fmtSubChunkBytes, 0, riffChunkSize);

               CheckExcpectedBytes(fmtSubChunkBytes.SubArray(0, 4, false), RIFF_STR);

               // Must change the reading order for little endien bytes.
               mChunkSize = (int)BitConvertorWrapper.ConvertByteArrayToInt(
                   fmtSubChunkBytes.SubArray<byte>(4, sizeof(int), true),
                   BitConvertorWrapper.IntType.Int32);

               CheckExcpectedBytes(fmtSubChunkBytes.SubArray(8, 4, false), WAVE_STR);
          }

          private void CheckCalculateableVariables(int byteRate, short bytesPerSample)
          {
               if (byteRate != ByteRate)
               {
                    throw new InvalidDataException(
                        $"ByteRate is {byteRate} while it shoudl be {ByteRate}");
               }

               if (bytesPerSample != BytesPerSample)
               {
                    throw new InvalidDataException(
                         $"BytesPerSample is {bytesPerSample} while it shoudl be {BytesPerSample}");
               }
          }

          /// <summary>
          /// Reads the "fmt " sub chunk. Should start at offset 12 (in bytes).
          /// </summary>
          /// <param name="stream"></param>
          private void ReadFmtSubChunk(Stream stream)
          {
               int fmtSubChunkSize = 24; // Reads only 16 bytes. The rest are irrelevant for us.
               stream.Seek(12, SeekOrigin.Begin);
               byte[] fmtSubChunkBytes = new byte[fmtSubChunkSize];
               stream.Read(fmtSubChunkBytes, 0, fmtSubChunkSize);

               int indexReader = 0;

               CheckExcpectedBytes(fmtSubChunkBytes.SubArray(indexReader, 4, false), FMT_STR);

               indexReader += 4;
               mSubChunk1Size = (int)BitConvertorWrapper.ConvertByteArrayToInt(
                   fmtSubChunkBytes.SubArray(indexReader, sizeof(int), true),
                   BitConvertorWrapper.IntType.Int32);

               indexReader += sizeof(int);
               mAudioFormat = (short)BitConvertorWrapper.ConvertByteArrayToInt(
                   fmtSubChunkBytes.SubArray(indexReader, sizeof(short), true),
                   BitConvertorWrapper.IntType.Int16);

               indexReader += sizeof(short);
               NumChannels = (short)BitConvertorWrapper.ConvertByteArrayToInt(
                   fmtSubChunkBytes.SubArray(indexReader, sizeof(short), true),
                   BitConvertorWrapper.IntType.Int16);

               indexReader += sizeof(short);
               SampleRate = (int)BitConvertorWrapper.ConvertByteArrayToInt(
                   fmtSubChunkBytes.SubArray(indexReader, sizeof(int), true),
                   BitConvertorWrapper.IntType.Int32);

               indexReader += sizeof(int);
               int byteRate = (int)BitConvertorWrapper.ConvertByteArrayToInt(
                   fmtSubChunkBytes.SubArray(indexReader, sizeof(int), true),
                   BitConvertorWrapper.IntType.Int32);

               indexReader += sizeof(int);
               short bytesPerSample = (short)BitConvertorWrapper.ConvertByteArrayToInt(
                   fmtSubChunkBytes.SubArray(indexReader, sizeof(short), true),
                   BitConvertorWrapper.IntType.Int16);

               indexReader += sizeof(short);
               BitsPerSample = (short)BitConvertorWrapper.ConvertByteArrayToInt(
                   fmtSubChunkBytes.SubArray(indexReader, sizeof(short), true),
                   BitConvertorWrapper.IntType.Int16);

               CheckCalculateableVariables(byteRate, bytesPerSample);
          }

          private int? GetStringOffset(byte[] bytesChunk, byte[] bytesToFind)
          {
               int? offset = null;
               if (bytesToFind.Length <= bytesChunk.Length)
               {
                    for (int i = 0; i < bytesChunk.Length - bytesToFind.Length; ++i)
                    {
                         bool found = true;
                         for (int j = 0; (j < bytesToFind.Length) && (found); ++j)
                         {
                              if (bytesChunk[i + j] != bytesToFind[j])
                              {
                                   found = false;
                              }
                         }
                         if (found)
                         {
                              offset = i;
                              break;
                         }
                    }
               }

               return offset;
          }

          /// <summary>
          /// Maybe there is another metadata (Example: LIST, INFO...) that we are ignoring.
          /// </summary>
          /// <param name="stream"></param>
          private void ReadDataSubChunk(Stream stream)
          {
               stream.Seek(mSubChunk1Size + 20, SeekOrigin.Begin);
               byte[] dataSubChunkBytes = new byte[mChunkSize - mSubChunk1Size - 3 * 4];
               stream.Read(dataSubChunkBytes, 0, dataSubChunkBytes.Length);

               int? offsetInArr = GetStringOffset(dataSubChunkBytes,
                   Encoding.UTF8.GetBytes(DATA_STR));
               if (!offsetInArr.HasValue)
               {
                    throw new InvalidDataException($"{DATA_STR} was not found");
               }

               DataSizeInBytes = (int)BitConvertorWrapper.ConvertByteArrayToInt(
                   dataSubChunkBytes.SubArray(offsetInArr.Value + 4, sizeof(int), true),
                   BitConvertorWrapper.IntType.Int32);

               SoundData = dataSubChunkBytes.SubArray(
                   offsetInArr.Value + 8, DataSizeInBytes, false);
          }

          /// <summary>
          /// Assumption: The header is 44 bytes.
          /// </summary>
          /// <param name="savedStream"></param>
          /// <param name="soundDataSizeInBytes"></param>
          private void WriteWaveFileHeader(Stream savedStream, int soundDataSizeInBytes)
          {
               // Writes "RIFF" chunk descriptor.
               StreamOperations.WriteStringToStream(savedStream, RIFF_STR);
               StreamOperations.WriteIntegerToStream(savedStream, soundDataSizeInBytes + 36, BitConvertorWrapper.IntType.Int32);
               StreamOperations.WriteStringToStream(savedStream, WAVE_STR);

               // Write "fmt" sub-chunk.
               StreamOperations.WriteStringToStream(savedStream, FMT_STR);
               StreamOperations.WriteIntegerToStream(savedStream, 16, BitConvertorWrapper.IntType.Int32);
               StreamOperations.WriteIntegerToStream(savedStream, mAudioFormat, BitConvertorWrapper.IntType.Int16);
               StreamOperations.WriteIntegerToStream(savedStream, NumChannels, BitConvertorWrapper.IntType.Int16);
               StreamOperations.WriteIntegerToStream(savedStream, SampleRate, BitConvertorWrapper.IntType.Int32);
               StreamOperations.WriteIntegerToStream(savedStream, ByteRate, BitConvertorWrapper.IntType.Int32);
               StreamOperations.WriteIntegerToStream(savedStream, BytesPerSample, BitConvertorWrapper.IntType.Int16);
               StreamOperations.WriteIntegerToStream(savedStream, BitsPerSample, BitConvertorWrapper.IntType.Int16);

               // Write "data" sub-chunk.
               StreamOperations.WriteStringToStream(savedStream, DATA_STR);
               StreamOperations.WriteIntegerToStream(savedStream, soundDataSizeInBytes, BitConvertorWrapper.IntType.Int32);
          }

          private void ReadFromStream(Stream wavStream)
          {
               ReadRiffChunkDescriptor(wavStream);
               ReadFmtSubChunk(wavStream);
               ReadDataSubChunk(wavStream);
          }

          private void LoadWaveFile(FilePath filePath)
          {
               using (Stream fileStream = new FileStream(
               filePath.FileFullPath, FileMode.Open, FileAccess.Read))
               {
                    ReadFromStream(fileStream);
                    Console.WriteLine($"{filePath.Name} loaded succesfully");
               }
          }

          /// <summary>
          /// Creates a folder with the same wave file name, moves the wave file to that folder
          /// and creates another folder with the splitted wave files.
          /// </summary>
          /// <param name="intervalInSeconds"></param>
          public void SplitWaveFileByInterval(float intervalInSeconds)
          {
               // Creates directory "waveFilesDirectory".
               FilePath waveFilesDirectory = FilePath.CreateFilePath(
                   FilePath.DirectoryPath, FilePath.NameWithoutExtension);
               Directory.CreateDirectory(waveFilesDirectory.FileFullPath);
               Console.WriteLine($"New directory created:{waveFilesDirectory.FileFullPath}");

               // Moves wave file into directory.
               FilePath newWaveFilePath = FilePath.CreateFilePath(
                    waveFilesDirectory.FileFullPath, FilePath.Name);
               File.Move(FilePath.FileFullPath, newWaveFilePath.FileFullPath);
               FilePath = newWaveFilePath;
               Console.WriteLine($"Wave file renamed to {newWaveFilePath.FileFullPath}");

               // Creates directory inside "waveFilesDirectory" for the splitted wave files.
               FilePath splittedWaveFilesDirectory = FilePath.CreateFilePathWithPrefix(
                   waveFilesDirectory.FileFullPath, "splitted", FilePath.NameWithoutExtension);
               Directory.CreateDirectory(splittedWaveFilesDirectory.FileFullPath);
               Console.WriteLine($"New directory created:{splittedWaveFilesDirectory.FileFullPath}");

               int bytesNumberPerSplit = (int)(intervalInSeconds * SampleRate * BytesPerSample);
               int splitsNumber = (int)Math.Ceiling(
                   Convert.ToDecimal(DataSizeInBytes / bytesNumberPerSplit));
               int bytesReaderIndex = 0;

               Console.WriteLine(
                   $"{FilePath.Name} will be splited into interval of {intervalInSeconds} seconds");

               for (int splitNum = 0; splitNum < splitsNumber; ++splitNum)
               {
                    byte[] samplesGroup = new byte[bytesNumberPerSplit];
                    for (int bytesWriterIndex = 0;
                        (bytesWriterIndex < bytesNumberPerSplit) &&
                        (bytesReaderIndex < DataSizeInBytes);
                         ++bytesWriterIndex, ++bytesReaderIndex)
                    {
                         samplesGroup[bytesWriterIndex] = SoundData[bytesReaderIndex];
                    }

                    FilePath splittedWaveFilePath = FilePath.CreateFilePathWithPrefix(
                        splittedWaveFilesDirectory.FileFullPath, splitNum.ToString(), $"split{WavFileExtension}");
                    using (Stream outputSteam = new FileStream(
                        splittedWaveFilePath.FileFullPath, FileMode.Create, FileAccess.Write))
                    {
                         WriteWaveFileHeader(outputSteam, bytesNumberPerSplit);
                         outputSteam.Write(samplesGroup, 0, bytesNumberPerSplit);
                    }

                    Console.WriteLine($"{splittedWaveFilePath.FileFullPath} was saved");
               }
          }

          /// <summary>
          /// NoiseRatio value represents the percentage of byte that will be noised.
          /// NoiseRatio value is from 0 to 100.
          /// </summary>
          /// <param name="noiseRatio"></param>
          public void AddNoise(int noiseRatio)
          {
               if ((noiseRatio < 0) || (noiseRatio > 100))
               {
                    Console.WriteLine($"Noise ratio {noiseRatio} is illegal. Should be between 0 to 100.");
               }
               else
               {
                    float ruineIntervals = ((float)100 / noiseRatio);
                    float byteRuinerCounter = 0f;
                    for(int i = 0; i < SoundData.Length; ++i)
                    {
                         byteRuinerCounter++;
                         if(byteRuinerCounter >= ruineIntervals)
                         {
                              byteRuinerCounter -= ruineIntervals;
                              SoundData[i] = BitOperations.ToggleNBit(SoundData[i], 0);
                         }
                    }
               }

               FilePath noisedWaveFile = FilePath.CreateFilePathWithPrefix(
                    FilePath.DirectoryPath, $"Noised_{noiseRatio}", FilePath.Name);
               Save(noisedWaveFile);
          }

          public void Save(FilePath filePath)
          {
               using (Stream savedStream = new FileStream(
                   filePath.FileFullPath, FileMode.Create, FileAccess.Write))
               {
                    WriteWaveFileHeader(savedStream, SoundData.Length);
                    StreamOperations.WriteBytesToStream(savedStream, SoundData);
                    Console.WriteLine($"{filePath.Name} saved");
               }
          }

          public void Save()
          {
               Save(FilePath);
          }

          public void CutFromWaveFile(FilePath outputFilePath, int cutBeginInSeconds, int cutEndInSeconds)
          {
               int secondsToCut = cutEndInSeconds - cutBeginInSeconds;
               if (secondsToCut < 0)
               {
                    Console.WriteLine($"{nameof(cutBeginInSeconds)} is greater then {nameof(cutEndInSeconds)}");
                    return;
               }
               else if ((cutBeginInSeconds < 0) || (cutEndInSeconds > DurationInSeconds))
               {
                    Console.WriteLine($"Illegel value of {nameof(cutBeginInSeconds)} or {nameof(cutEndInSeconds)}");
                    return;
               }
               else
               {
                    // The method: Copies from start. When reaches endCopyPosition stop to copy and return to
                    // copy when reaches startCopyAgainPosition.
                    int endCopyPosition = cutBeginInSeconds * ByteRate;
                    int startCopyAgainPosition = cutEndInSeconds * ByteRate;
                    int writer = 0;

                    byte[] cutSoundData = new byte[SoundData.Length - (startCopyAgainPosition - endCopyPosition)];

                    for (int reader = 0; reader < endCopyPosition; ++reader, ++writer)
                    {
                         cutSoundData[writer] = SoundData[reader];
                    }

                    for (int reader = startCopyAgainPosition; reader < SoundData.Length; ++reader, ++writer)
                    {
                         cutSoundData[writer] = SoundData[reader];
                    }

                    using (Stream outputSteam = new FileStream(
                        outputFilePath.FileFullPath, FileMode.Create, FileAccess.Write))
                    {
                         WriteWaveFileHeader(outputSteam, cutSoundData.Length);
                         outputSteam.Write(cutSoundData, 0, cutSoundData.Length);
                    }

                    Console.WriteLine($"{outputFilePath.FileFullPath} was saved");
               }
          }

          public void CutFromStart(FilePath outputFilePath, int secondsToCut)
          {
               CutFromWaveFile(outputFilePath, 0, secondsToCut);
          }

          public void CutFromEnd(FilePath outputFilePath, int secondsToCut)
          {
               CutFromWaveFile(outputFilePath, DurationInSeconds - secondsToCut, DurationInSeconds);
          }

          public void PrintSummary()
          {
               Console.WriteLine($@"{FilePath.Name} Summary:
Format:             {mAudioFormat}
Channels number:    {NumChannels}
SampleRate:         {SampleRate}
ByteRate:           {ByteRate}  
BlockAlign:         {BytesPerSample}
BitsPerSample:      {BitsPerSample}
DataSizeInBytes:    {DataSizeInBytes}
");
          }

          /// <summary>
          /// Number of bytes that been read per second. 
          /// </summary>
          public int ByteRate
          {
               get
               {
                    return SampleRate * NumChannels * BitsPerSample / 8;
               }
          }

          /// <summary>
          /// Block align.
          /// </summary>
          private short BytesPerSample
          {
               get
               {
                    return (short)(NumChannels * BitsPerSample / 8);
               }
          }

          /// <summary>
          /// Length is number of samples.
          /// </summary>
          public long SamplesNumber
          {
               get
               {
                    return (long)(DataSizeInBytes * 0.5);
               }
          }

          public int DurationInSeconds
          {
               get
               {
                    return (int)((SamplesNumber / SampleRate) / NumChannels);
               }
               // or get
               //{
               //     return SoundData.Length / (SampleRate * BytesPerSample);
               //}
          }

          public int FileSizeInBytes
          {
               get
               {
                    return mChunkSize + 8;
               }
          }
     }
}
