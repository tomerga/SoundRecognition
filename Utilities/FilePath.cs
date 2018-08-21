using System.IO;

namespace PopcornTest
{
     public class FilePath
     {
          private string mDirectoryPath;
          private string mFileName;

          private FilePath(string directoryPath, string prefix, string nameWithExtension)
          {
               mDirectoryPath = directoryPath;
               mFileName = $"{prefix}_{Path.GetFileName(nameWithExtension)}";
          }

          private FilePath(string path, string name)
          {
               mDirectoryPath = path;
               mFileName = Path.GetFileName(name);
          }

          private FilePath(string fullPath)
          {
               mDirectoryPath = Path.GetDirectoryName(fullPath);
               mFileName = Path.GetFileName(fullPath);
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

          // Setters and getters.
          public string NameWithoutExtension
          {
               get
               {
                    return Path.GetFileNameWithoutExtension(mFileName);
               }
          }

          public string Name
          {
               get
               {
                    return mFileName;
               }
          }

          public string DirectoryPath
          {
               get
               {
                    return mDirectoryPath;
               }
          }

          public string FileFullPath
          {
               get
               {
                    return Path.Combine(mDirectoryPath, mFileName);
               }
          }

          /// <summary>
          /// Extension with dot.
          /// </summary>
          public string Extension
          {
               get
               {
                    return Path.GetExtension(mFileName).ToLower();
               }
          }
     }
}
