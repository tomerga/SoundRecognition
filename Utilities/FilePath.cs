using System.IO;

namespace SoundRecognition
{
     public class FilePath
     {
          public string Name { get; }
          public string DirectoryPath { get; }

          public string NameWithoutExtension
          {
               get
               {
                    return Path.GetFileNameWithoutExtension(Name);
               }
          }

          public string FileFullPath
          {
               get
               {
                    return Path.Combine(DirectoryPath, Name);
               }
          }

          /// <summary>
          /// Extension with dot.
          /// </summary>
          public string Extension
          {
               get
               {
                    return Path.GetExtension(Name).ToLower();
               }
          }
          private FilePath(string directoryPath, string prefix, string nameWithExtension)
          {
               DirectoryPath = directoryPath;
               Name = $"{prefix}_{Path.GetFileName(nameWithExtension)}";
          }

          private FilePath(string path, string name)
          {
               DirectoryPath = path;
               Name = Path.GetFileName(name);
          }

          private FilePath(string fullPath)
          {
               DirectoryPath = Path.GetDirectoryName(fullPath);
               Name = Path.GetFileName(fullPath);
          }

          public static FilePath CreateFilePathWithPrefix(string directoryPath, string prefix, string nameWithExtension)
          {
               return new FilePath(directoryPath, prefix, nameWithExtension);
          }

          public static FilePath CreateFilePath(string fullPath)
          {
               return new FilePath(fullPath);
          }

          public static FilePath CreateFilePath(string path, string name)
          {
               return new FilePath(path, name);
          }
     }
}
