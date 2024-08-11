using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace SimCity4.ModManager.Installer.FileHandlers;

public abstract class DatHandlerTest : IDisposable
{
    private static readonly string PathToTestMaterial = "TestFiles";

    private static readonly string archivePath = Path.Combine(PathToTestMaterial, "dat","file.dat");

    private static readonly string outputFolder = Path.Combine(PathToTestMaterial, "dat","Output");

    private static readonly string tempFolder = Path.Combine(PathToTestMaterial, "dat","Temp");

    private static readonly string outputFile1 = Path.Combine(PathToTestMaterial, "dat","Output","Plugins", "file.dat");

    public void Dispose()
    {
        if (Directory.Exists(tempFolder))
        {
            Directory.Delete(tempFolder, true);
        }

        if (Directory.Exists(outputFolder))
        { Directory.Delete(outputFolder, true); }

        if (File.Exists(outputFile1))
        {
            File.Delete(outputFile1);
        }
    }

    public class TheFileInfoSetter : DatHandlerTest
    {
        [Fact]
        public void ShouldThrowExceptionIfFileDoesNotPointToZipFile()
        {
            var instance = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.DatHandler();

            Action act = () => instance.FileInfo = new FileInfo(
                Path.Combine(PathToTestMaterial, @"zip\archive.zip"));

            act.Should().Throw<ArgumentException>().WithMessage("FileInfo must point to a .dat file.");
        }
    }

    public class TheExtractFilexToTempMethod() : DatHandlerTest
    {
        [Fact]
        public void ShouldThrowExceptionWhenFileInfoIsNotSet()
        {
            var instance = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.DatHandler();
            Assert.Throws<InvalidOperationException>(() => instance.ExtractFilesToTemp());
        }

        [Fact]
        public void ShouldMoveFilesToTempFolder()
        {
            var instance = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.DatHandler
            {
                FileInfo = new FileInfo(archivePath),
                TempFolder = tempFolder
            };

            var infos = instance.ExtractFilesToTemp();

            infos.Should().HaveCount(1).And.ContainSingle(x => x.Name == "file.dat");
        }
    }

    public class TheMoveFilesToUserFolderMethod : DatHandlerTest
    {
        [Fact]
        public void ShouldThrowExceptionIfTempFileIsNotPresentAndUserFolderIsNull()
        {
            var instance = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.DatHandler();

            Action act = () => instance.MoveToPluginFolder(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("userFolder").WithMessage("UserFolder may not be null.*");
        }

        [Fact]
        public void ShouldThrowExceptionWhenTempFileIsNotPresentAndUserFolderIsValid()
        {
            var instance = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.DatHandler();

            var userFolder = new SimCity4.ModManager.App.Model.UserFolder
            {
                Alias = "Main plugin folder",
                FolderPath = Path.Combine(PathToTestMaterial, "Plugins")
            };

            Action act = () => instance.MoveToPluginFolder(userFolder);

            act.Should().Throw<InvalidOperationException>().WithMessage("The archive has not been extracted to the temp folder.");
        }

        [Fact]
        public void ShouldThrowExceptionWhenTempFileIsPresentAndUserFolderIsNull()
        {
            var instance = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.DatHandler { FileInfo = new FileInfo(archivePath), TempFolder = tempFolder };

            instance.ExtractFilesToTemp();

            Action act = () => instance.MoveToPluginFolder(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("userFolder").WithMessage("UserFolder may not be null.*");
        }

        [Fact]
        public void ShouldMoveFileToPluginFolderAndReturnListOfInstalledFiles()
        {
            const string PluginFolderName = "PluginsFolder";

            var folderCreated = Directory.CreateDirectory(Path.Combine(PathToTestMaterial, PluginFolderName));
                
            var instance = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.DatHandler { FileInfo = new FileInfo(archivePath), TempFolder = tempFolder };
            var userFolder = new SimCity4.ModManager.App.Model.UserFolder { Alias = "Main plugin folder", FolderPath = outputFolder };

            Action act = () => instance.ExtractFilesToTemp();

            act.Should().NotThrow();

            var installedFiles = instance.MoveToPluginFolder(userFolder);

            installedFiles.Should().HaveCount(1).And.ContainSingle(x => x.Path == outputFile1 && x.Checksum == "ce54d1157f2cea1d77bb0e3aef45b37c");

            File.Exists(outputFile1).Should().BeTrue("File 1 not in plugin folder.");
        }
    }
}