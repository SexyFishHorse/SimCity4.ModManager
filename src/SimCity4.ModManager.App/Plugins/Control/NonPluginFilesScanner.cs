using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;

namespace SimCity4.ModManager.App.Plugins.Control;

using SearchOption = System.IO.SearchOption;

public class NonPluginFilesScanner
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public NonPluginFilesScanner(string storageLocation)
    {
        StorageLocation = storageLocation;

        LoadFileTypesFromDisc();
    }

    public string StorageLocation { get; set; }

    public IEnumerable<SimCity4.ModManager.App.Model.FileTypeInfo> FileTypes { get; set; }

    public void LoadFileTypesFromDisc()
    {
        var fileLocation = Path.Combine(StorageLocation, "NonPluginFileTypes.json");

        var newFileTypes = new Collection<SimCity4.ModManager.App.Model.FileTypeInfo>();

        if (File.Exists(fileLocation))
        {
            using (var reader = new StreamReader(fileLocation))
            {
                var json = reader.ReadToEnd();

                dynamic fileTypeJson = JArray.Parse(json);

                foreach (var fileType in fileTypeJson)
                {
                    newFileTypes.Add(
                        new SimCity4.ModManager.App.Model.FileTypeInfo
                        {
                            Extension = fileType.Extension,
                            DescriptiveName = fileType.DescriptiveName,
                            Description = fileType.Description
                        });
                }
            }
        }

        FileTypes = newFileTypes;
    }

    public ICollection<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo> GetFilesAndFoldersToRemove(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        var fileTypeCandidateInfos = GetCandiateFileTypeInfos(userFolder);

        var emptyFolders = GetEmptyFolders(userFolder);

        var output = new Collection<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo>();

        foreach (var fileTypeCandidateInfo in fileTypeCandidateInfos)
        {
            output.Add(fileTypeCandidateInfo);
        }

        if (emptyFolders.Any())
        {
            output.Add(
                new SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo
                {
                    FileTypeInfo =
                        new SimCity4.ModManager.App.Model.FileTypeInfo
                        {
                            Extension = string.Empty,
                            Description = "Empty Folders",
                            DescriptiveName = "Folders"
                        },
                    NumberOfEntities = emptyFolders.Count
                });
        }

        return output;
    }

    public SimCity4.ModManager.App.Model.NonPluginFileRemovalSummary RemoveNonPluginFiles(SimCity4.ModManager.App.Model.UserFolder userFolder, IEnumerable<SimCity4.ModManager.App.Model.FileTypeInfo> fileTypesToRemove)
    {
        var filesToDelete = GetFilesToDelete(userFolder, fileTypesToRemove);

        var numFiles = 0;
        var numFolders = 0;

        var errors = new Dictionary<string, Exception>();

        foreach (var file in filesToDelete)
        {
            try
            {
                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                numFiles++;
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to delete file \"{file}\", Error: {ex.Message}");
                errors.Add(file, ex);
            }
        }

        var foldersToDelete = GetEmptyFolders(userFolder);

        foreach (var folder in foldersToDelete.Where(Directory.Exists))
        {
            try
            {
                FileSystem.DeleteDirectory(folder, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                numFolders++;
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to delete the folder \"{folder}\", Error: {ex.Message}");
                errors.Add(folder, ex);
            }
        }

        return new SimCity4.ModManager.App.Model.NonPluginFileRemovalSummary
        {
            NumFilesRemoved = numFiles,
            NumFoldersRemoved = numFolders,
            Errors = errors
        };
    }

    private static IList<string> GetEmptyFolders(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        var folders = Directory.EnumerateDirectories(userFolder.PluginFolderPath, "*", SearchOption.AllDirectories).ToList();
        var foldersToDelete =
            folders.Where(x => !new DirectoryInfo(x).EnumerateFiles("*", SearchOption.AllDirectories).Any()).ToList();
        return foldersToDelete;
    }

    private IEnumerable<string> GetFilesToDelete(SimCity4.ModManager.App.Model.UserFolder userFolder, IEnumerable<SimCity4.ModManager.App.Model.FileTypeInfo> fileTypesToRemove)
    {
        var files = Directory.EnumerateFiles(userFolder.PluginFolderPath, "*", SearchOption.AllDirectories).ToList();
        var filesToDelete = new List<string>();

        foreach (var fileType in fileTypesToRemove)
        {
            filesToDelete.AddRange(files.Where(x => x.ToUpperInvariant().EndsWith(fileType.Extension.ToUpperInvariant())));
        }

        return filesToDelete;
    }

    private IEnumerable<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo> GetCandiateFileTypeInfos(SimCity4.ModManager.App.Model.UserFolder userFolder)
    {
        var files = Directory.EnumerateFiles(userFolder.PluginFolderPath, "*", SearchOption.AllDirectories).ToList();
        var output = new Collection<SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo>();

        foreach (var fileTypeInfo in FileTypes)
        {
            var numberOfFiles =
                files.Count(x => x.EndsWith(fileTypeInfo.Extension, StringComparison.OrdinalIgnoreCase));

            if (numberOfFiles > 0)
            {
                output.Add(new SimCity4.ModManager.App.Model.NonPluginFileTypeCandidateInfo { FileTypeInfo = fileTypeInfo, NumberOfEntities = numberOfFiles });
            }
        }

        return output;
    }
}