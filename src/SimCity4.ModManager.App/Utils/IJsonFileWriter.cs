using System.IO;

namespace SimCity4.ModManager.App.Utils;

public interface IJsonFileWriter
{
    void WriteToFile(FileInfo fileInfo, object objectToWrite, bool indented = true);
}