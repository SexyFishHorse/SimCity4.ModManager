namespace SimCity4.ModManager.Installer.FileHandlers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using SimCity4.ModManager.App.Model;
using SimCity4.ModManager.App.Plugins.Installer.FileHandlers;
using Xunit;

public abstract class ZipHandlerTest : IDisposable
{
    private const int ExpectedNumberOfFiles = 2;

    private static readonly string PathToTestMaterial = "TestFiles";
    
    private const string TxtFilename = "readme.txt";
    
    private const string DatFilename = "PEG_Mem-Park-Kit_106.dat";

    private static readonly string ArchivePath = Path.Combine(PathToTestMaterial, @"zip", "archive.zip");

    private readonly List<string> directoriesUsed = [];

    public void Dispose()
    {
        foreach (var directory in directoriesUsed.Where(Directory.Exists))
        {
            Directory.Delete(directory, true);
        }

        directoriesUsed.Clear();
    }

    public class TheExtractFilesToTempMethod : ZipHandlerTest
    {
        [Fact]
        public void ShouldThrowExceptionWhenFileInfoIsNotSet()
        {
            var instance = new ArchiveHandler();

            var act = new Action(() => instance.ExtractFilesToTemp());

            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("FileInfo is not set.");
        }

        [Fact]
        public void ShouldExtractFilesToTempFolder()
        {
            var tempFolder = Directory.CreateTempSubdirectory().FullName;
            directoriesUsed.Add(tempFolder);

            var instance = new ArchiveHandler
            {
                FileInfo = new FileInfo(ArchivePath),
                TempFolder = tempFolder,
            };

            instance.ExtractFilesToTemp();

            Directory.Exists(tempFolder).Should().BeTrue("Install directory does not exist.");

            var entries = Directory.GetFileSystemEntries(tempFolder);

            entries.Should()
                .HaveCount(ExpectedNumberOfFiles, "archive should only contain two file which should both be copied over.")
                .And.Contain(Path.Combine(tempFolder, DatFilename))
                .And.Contain(Path.Combine(tempFolder, TxtFilename));
        }
    }

    public class TheMoveFilesToUserFolderMethod : ZipHandlerTest
    {
        [Fact]
        public void ShouldThrowExceptionWhenUserFolderIsNullAndTempFileIsMissing()
        {
            var instance = new ArchiveHandler();

            var act = () => instance.MoveToPluginFolder(null);

            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("userFolder")
                .WithMessage("UserFolder may not be null.*");
        }

        [Fact]
        public void ShouldThrowExceptionWhenTempFileIsNotPresent()
        {
            var folderPath = Directory.CreateTempSubdirectory().FullName;
            directoriesUsed.Add(folderPath);

            var instance = new ArchiveHandler();
            var userFolder = new UserFolder
            {
                Alias = "Main plugin folder",
                FolderPath = folderPath,
            };

            var act = () => instance.MoveToPluginFolder(userFolder);

            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("The archive has not been extracted to the temp folder.");
        }

        

        [Fact]
        public void ShouldThrowExceptionWhenUserFolderIsNull()
        {
            var tempFolder = Directory.CreateTempSubdirectory().FullName;
            directoriesUsed.Add(tempFolder);

            var instance = new ArchiveHandler
            {
                FileInfo = new FileInfo(ArchivePath),
                TempFolder = tempFolder,
            };

            var act = () =>
            {
                instance.ExtractFilesToTemp();
                instance.MoveToPluginFolder(null);
            };

            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("userFolder")
                .WithMessage("UserFolder may not be null.*");
        }

        [Fact]
        public void ShouldMoveFilesToPluginAndReturnListOfInstalledFiles()
        {
            var pluginFolderPath = Directory.CreateTempSubdirectory().FullName;
            var tempFolder = Directory.CreateTempSubdirectory().FullName;
            directoriesUsed.AddRange([pluginFolderPath, tempFolder]);

            var expectedTxtFilePath = Path.Combine(pluginFolderPath, "Plugins", TxtFilename);
            var expectedDatFilePath = Path.Combine(pluginFolderPath, "Plugins", DatFilename);

            var instance = new ArchiveHandler
            {
                FileInfo = new FileInfo(ArchivePath),
                TempFolder = tempFolder,
            };
            var userFolder = new UserFolder
            {
                Alias = "Main plugin folder",
                FolderPath = pluginFolderPath,
            };

            instance.ExtractFilesToTemp();
            var installedFiles = instance.MoveToPluginFolder(userFolder).ToList();

            installedFiles.Should()
                .Contain(x => x.Path == expectedTxtFilePath && x.Checksum == "7c1e41fd4219c43db85ca75bbba9d1ad")
                .And.Contain(x => x.Path == expectedDatFilePath && x.Checksum == "95f09d6e18bc1775b487eaf11909776c");

            File.Exists(expectedTxtFilePath).Should().BeTrue("File 1 not in plugin folder.");
            File.Exists(expectedDatFilePath).Should().BeTrue("File 2 not in plugin folder.");
        }
    }
}
