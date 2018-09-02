using System;
using System.IO;
using System.Collections.Generic;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Query;
using SoundFingerprinting.DAO;
using SoundFingerprinting.Data;

namespace SoundRecognition
{
     static class SoundFingerprintingWrapper
     {
          private static string mFingerPrintExtension = ".fp";
          private static string mTrackReferenceExtension = ".tr";
          private static string mDatabaseDirectoryName = "Database";

          private static readonly IModelService mModelService = new InMemoryModelService(); // Stores fingerprints in RAM.
          private static readonly IAudioService mAudioService = new SoundFingerprintingAudioService(); // Default audio library.

          private static void SaveFingerPrintsInMemory(
              List<HashedFingerprint> newHashedFingerprints,
              IModelReference trackReference,
              string name)
          {
               FilePath fingerPrintsDBFilePath =
                   FilePath.CreateFilePath(Path.Combine(mDatabaseDirectoryName, $"{name}{mFingerPrintExtension}"));
               FilePath trackRefernceFilePath =
                   FilePath.CreateFilePath(Path.Combine(mDatabaseDirectoryName, $"{name}{mTrackReferenceExtension}"));
               SerializationMachine.ProtoSerialize(newHashedFingerprints, fingerPrintsDBFilePath);
               SerializationMachine.ProtoSerialize(trackReference, trackRefernceFilePath);
          }

          /// <summary>
          /// Uses HighPrecisionQueryConfiguration().
          /// </summary>
          /// <param name="queryAudioFile"></param>
          /// <param name="secondsToAnalyze">Number of seconds to analyze from query file</param>
          /// <param name="startAtSecond"></param>
          /// <returns></returns>
          private static ResultEntry GetBestMatchForSong(string queryAudioFile, double secondsToAnalyze, int startAtSecond)
          {
               // Query the underlying database for similar audio sub-fingerprints.
               QueryResult queryResult = QueryCommandBuilder.Instance.BuildQueryCommand()
                                                    .From(queryAudioFile, secondsToAnalyze, startAtSecond)
                                                    .WithQueryConfig(new HighPrecisionQueryConfiguration())
                                                    .UsingServices(mModelService, mAudioService)
                                                    .Query()
                                                    .Result;

               return queryResult?.BestMatch;
          }

          public static void Initialize()
          {
               Directory.CreateDirectory(mDatabaseDirectoryName);
          }

          public static void LoadFingerPrintsDataBase()
          {
               // Search for all files with given extensios in given directory.
               foreach (string fingerPrintFile in Directory.GetFiles(mDatabaseDirectoryName, $"*{mFingerPrintExtension}"))
               {
                    List<HashedFingerprint> hashedFingerprints =
                        SerializationMachine.ProtoDeserialize<List<HashedFingerprint>>(
                            FilePath.CreateFilePath(fingerPrintFile));

                    string trackReferenceFile = fingerPrintFile.Replace(mFingerPrintExtension, mTrackReferenceExtension);
                    if (File.Exists(trackReferenceFile))
                    {
                         IModelReference trackReference =
                             SerializationMachine.ProtoDeserialize<IModelReference>(
                                 FilePath.CreateFilePath(trackReferenceFile));

                         if (hashedFingerprints != null && trackReference != null)
                         {
                              mModelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
                              Console.WriteLine($"Loaded fingerprint of track reference ID: {trackReference.Id}");
                         }
                         else
                         {
                              Console.WriteLine($"Cannot load fingerprint {fingerPrintFile}");
                         }
                    }
                    else
                    {
                         Console.WriteLine($"Error! the track reference file of {fingerPrintFile} is missing");
                    }
               }
          }

          /// <summary>
          /// Stores the fingerprints of the given file into the ModelService.
          /// Uses HighPrecisionFingerprintConfiguration.
          /// </summary>
          /// <param name="waveFile"></param>
          public static void StoreNewAudioFileData(IAudioFile waveFile)
          {
               TrackData track = new TrackData(
                   isrc: "TD100INPROG", // International Standart Recording Code.
                   artist: "The TD's",
                   title: waveFile.FilePath.NameWithoutExtension,
                   album: waveFile.FilePath.DirectoryPath,
                   releaseYear: DateTime.Today.Year,
                   length: waveFile.DurationInSeconds);

               // Stores track metadata in the datasource.
               IModelReference trackReference = mModelService.InsertTrack(track);

               // Creates hashed fingerprints.
               List<HashedFingerprint> hashedFingerprints = FingerprintCommandBuilder.Instance
                                           .BuildFingerprintCommand()
                                           .From(waveFile.FilePath.FileFullPath)
                                           .WithFingerprintConfig(new HighPrecisionFingerprintConfiguration())
                                           .UsingServices(mAudioService)
                                           .Hash()
                                           .Result;

               // Stores hashes in the database.
               SaveFingerPrintsInMemory(hashedFingerprints, trackReference, waveFile.FilePath.NameWithoutExtension);
               mModelService.InsertHashDataForTrack(hashedFingerprints, trackReference);
               Console.WriteLine($"Stored {hashedFingerprints.Count} hashed fingerprints from {waveFile.FilePath.Name}");
          }

          public static bool FindMatchesForAudioFile(IAudioFile audioFile, int amplification, double secondToAnalyze)
          {
               bool isMatchFound = false;

               if (amplification <= 0) { amplification = 1; }
               Console.WriteLine($"Quering with amplification {amplification}, analyzing {secondToAnalyze} seconds");

               for (int i = 0; i < amplification; ++i)
               {
                    ResultEntry resultEntry = GetBestMatchForSong(
                        audioFile.FilePath.FileFullPath,
                        secondToAnalyze,
                        0);
                    if (resultEntry != null)
                    {
                         Console.WriteLine(string.Empty);
                         int minute = (int)(resultEntry.TrackMatchStartsAt / 60);
                         int second = (int)((resultEntry.TrackMatchStartsAt - 60 * minute));
                         Console.WriteLine($@"{audioFile.FilePath.FileFullPath} has match with
Name: {resultEntry.Track.Title}
Album: {resultEntry.Track.Album}
Confidence: {resultEntry.Confidence}
Can be found at: {resultEntry.TrackMatchStartsAt} sec ({minute}:{second})
");
                         if (resultEntry.Confidence > 0.2)
                         {
                              isMatchFound = true;
                         }
                    }
               }

               return isMatchFound;
          }
     }
}