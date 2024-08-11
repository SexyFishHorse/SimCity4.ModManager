using System.IO;
using Newtonsoft.Json;

namespace SimCity4.ModManager.App.Utils;

public class JsonFileWriter : IJsonFileWriter
{
    public void WriteToFile(FileInfo fileInfo, object objectToWrite, bool indented = true)
    {
        if (fileInfo.DirectoryName == null)
        {
            throw new DirectoryNotFoundException($"The path {fileInfo.FullName} does not contain a directory.");
        }

        Directory.CreateDirectory(fileInfo.DirectoryName);

        using (var fileStream = fileInfo.Create())
        using (var streamWriter = new StreamWriter(fileStream))
        using (var jsonWriter = new JsonTextWriter(streamWriter))
        {
            jsonWriter.Formatting = indented ? Formatting.Indented : Formatting.None;

            var serializer = new JsonSerializer();

            serializer.Serialize(jsonWriter, objectToWrite);
        }
    }
}