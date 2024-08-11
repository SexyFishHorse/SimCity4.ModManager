using System.Collections.Generic;
using System.IO;

namespace SimCity4.ModManager.App.Plugins.Installer.FileHandlers;

public class DatHandler : BaseHandler
{
    public override string RequiredExtension
    {
        get
        {
            return ".dat";
        }
    }

    public override IEnumerable<FileSystemInfo> ExtractFilesToTemp()
    {
        CheckFileInfoIsSet();
        CreateTempFolder();

        var newPath = Path.Combine(TempFolder, FileInfo.Name);
        File.Copy(FileInfo.FullName, newPath);

        return new List<FileSystemInfo> { new FileInfo(newPath) };
    }
}