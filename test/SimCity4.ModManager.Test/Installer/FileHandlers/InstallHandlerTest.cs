using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace SimCity4.ModManager.Installer.FileHandlers;

public class InstallHandlerTest : IDisposable
{
    private static readonly string PathToTestMaterial = Path.Combine("TestFiles", "multirar");

    private static readonly string archivePath = Path.Combine(PathToTestMaterial, @"archive.part1.rar");

    private static readonly string archivePart2Path = Path.Combine(PathToTestMaterial, @"archive.part2.rar");

    private static readonly string outputFolder = Path.Combine(PathToTestMaterial, @"Output");

    private static readonly string tempFolder = Path.Combine(PathToTestMaterial, @"Temp");

    [Fact]
    public void ShouldExtractMultipartRarArchive()
    {
        var instance = new SimCity4.ModManager.App.Plugins.Installer.FileHandlers.ArchiveHandler { FileInfo = new FileInfo(archivePath), TempFolder = tempFolder };

        var infos = instance.ExtractFilesToTemp().ToList();

        infos.Should().HaveCount(7)
            .And.Contain(x => x.Name.Equals("Floating_hotel", StringComparison.OrdinalIgnoreCase))
            .And.Contain(x => x.Name.Equals("Floating_hotel.jpg", StringComparison.OrdinalIgnoreCase))
            .And.Contain(x => x.Name.Equals("Floating_hotel-0x5ad0e817_0x1112e585_0x30000.SC4Model", StringComparison.OrdinalIgnoreCase))
            .And.Contain(x => x.Name.Equals("Floating_hotel-0x6534284a-0x304a15b2-0x513da7c8.SC4Desc", StringComparison.OrdinalIgnoreCase))
            .And.Contain(x => x.Name.Equals("Floating_hotel-dummy-0x5ad0e817_0x511378c7_0x30000.SC4Model", StringComparison.OrdinalIgnoreCase))
            .And.Contain(x => x.Name.Equals("Floating_hotel-dummy-0x6534284a-0x304a15b2-0x513da8f0.SC4Desc", StringComparison.OrdinalIgnoreCase))
            .And.Contain(x => x.Name.Equals("LM1x1_somy-Floating_hotel----------_b13dada1.SC4Lot", StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        if(Directory.Exists(outputFolder))
        {
            Directory.Delete(outputFolder, true);
        }

        if(Directory.Exists(tempFolder))
        {
            Directory.Delete(tempFolder, true);
        }
    }
}