using System.Collections.Generic;
using System.IO;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

namespace SimCity4.ModManager.App.Plugins.Installer.FileHandlers;

public class ArchiveHandler : BaseHandler
{
    public override string RequiredExtension
    {
        get
        {
            return "*";
        }
    }

    public override IEnumerable<FileSystemInfo> ExtractFilesToTemp()
    {
        CheckFileInfoIsSet();
        CreateTempFolder();

        var tempEntries = new List<FileSystemInfo>();

        using (var archive = ArchiveFactory.Open(FileInfo))
        {
            if (archive is RarArchive)
            {
                var rarArchive = archive as RarArchive;

                if (rarArchive.IsMultipartVolume() && !rarArchive.IsFirstVolume())
                {
                    throw new MultiVolumeExtractionException("You can only select part 1 in a multiRar plugin.");
                }
            }

            foreach (var entry in archive.Entries)
            {
                var tempPath = Path.Combine(TempFolder, entry.Key);

                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(tempPath);
                    tempEntries.Add(new DirectoryInfo(tempPath));
                }
                else
                {
                    var directories = Path.GetDirectoryName(tempPath);
                    if (!string.IsNullOrWhiteSpace(directories))
                    {
                        Directory.CreateDirectory(directories);
                    }

                    entry.WriteToFile(tempPath);
                    tempEntries.Add(new FileInfo(tempPath));
                }
            }
        }

        return tempEntries;
    }
}