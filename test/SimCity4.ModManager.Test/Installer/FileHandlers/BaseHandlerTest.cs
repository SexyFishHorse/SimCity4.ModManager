namespace SimCity4.ModManager.Installer.FileHandlers;

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using SimCity4.ModManager.App.Model;
using SimCity4.ModManager.App.Plugins.Installer.FileHandlers;
using Xunit;

public class BaseHandlerTest
{
    private const string PathToTestMaterial = "TestFiles";

    #region set_FileInfo

    [Fact(DisplayName = "set_FileInfo, null as FileInfo, Throws ArgumentNullException")]
    public void FileInfoSetterTest1()
    {
        var instance = new BaseHandlerImpl();
        var exception = Assert.Throws<ArgumentNullException>(() => instance.FileInfo = null);
        exception.Message.Should().Contain("value");
    }

    [Fact(DisplayName = "set_FileInfo, FileInfo for non-existent file, Throws FileNotFoundException")]
    public void FileInfoSetterTest2()
    {
        var instance = new BaseHandlerImpl();
        var value = new FileInfo(Path.Combine(PathToTestMaterial, "nonexistentfile.test"));
        var exception = Assert.Throws<FileNotFoundException>(() => instance.FileInfo = value);
        exception.Message.Should().Be("FileInfo does not point to an existing file.");
    }

    #endregion

    #region Class implementation

    internal class PluginFileTestComparer : IEqualityComparer<PluginFile>
    {
        public bool Equals(PluginFile? x, PluginFile? y)
        {
            if (x is null || y is null)
            {
                return false;
            }
            
            return x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase)
                   && x.Checksum.Equals(y.Checksum, StringComparison.Ordinal);
        }

        public int GetHashCode(PluginFile obj) => obj.Checksum.GetHashCode();
    }

    private class BaseHandlerImpl : BaseHandler
    {
        public override string RequiredExtension => throw new NotImplementedException();

        public override IEnumerable<FileSystemInfo> ExtractFilesToTemp() => throw new NotImplementedException();
    }

    #endregion
}
