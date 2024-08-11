using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using SimCity4.ModManager.App.Model;
using SimCity4.ModManager.App.UserFolders.Control;
using SimCity4.ModManager.App.UserFolders.DataAccess;
using Xunit;

namespace SimCity4.ModManager.Control.UserFolders;

public class TheUserFoldersControllerClass
{
    public class TheValidatePathMethod : TheUserFoldersControllerClass
    {
        public static IEnumerable<object?[]> InvalidData()
        {
            yield return [null, Guid.Empty];
            yield return [string.Empty, Guid.Empty];
            yield return [" ", Guid.Empty];
            yield return [null, Guid.NewGuid()];
            yield return [string.Empty, Guid.NewGuid()];
            yield return [" ", Guid.NewGuid()];
        }

        [Theory]
        [MemberData(nameof(InvalidData))]
        public void ShouldReturnFalseForInvalidPathOrId(string? path, Guid currentId)
        {
            var instance = new TheUserFoldersController(Substitute.For<IUserFoldersDataAccess>(), Substitute.For<IUserFolderController>());

            var result = instance.ValidatePath(path, currentId);

            result.Should().BeFalse();
        }

        [Fact(DisplayName = "ValidatePath(), Whitespace as string & with current id, Return False")]
        public void ValidatePath6()
        {
            var instance = new TheUserFoldersController(
                Substitute.For<IUserFoldersDataAccess>(),
                Substitute.For<IUserFolderController>());

            const string Path = " ";
            var currentId = Guid.NewGuid();

            var result = instance.ValidatePath(Path, currentId);

            result.Should().BeFalse();
        }

        [Fact]
        public void ValidatePath_InvalidPath_NoCurrentId_ReturnFalse()
        {
            var userFoldersDataAccess = Substitute.For<IUserFoldersDataAccess>();
            var userFolderController = Substitute.For<IUserFolderController>();
            var instance = new TheUserFoldersController(
                userFoldersDataAccess,
                userFolderController);

            const string Path = "example";

            var result = instance.ValidatePath(Path, Guid.Empty);

            result.Should().BeFalse();
        }

        [Fact]
        public void ValidatePath_InvalidPath_CurrentId_ReturnFalse()
        {
            var instance = new TheUserFoldersController(Substitute.For<IUserFoldersDataAccess>(), Substitute.For<IUserFolderController>());

            const string Path = "example";
            var id = Guid.NewGuid();

            var result = instance.ValidatePath(Path, id);

            result.Should().BeFalse();
        }

        [Fact]
        public void ValidatePath_ValidPath_NoCurrentId_ReturnTrue()
        {
            var userFoldersDataAccess = Substitute.For<IUserFoldersDataAccess>();
            userFoldersDataAccess.LoadUserFolders().Returns([]);

            var instance = new TheUserFoldersController(userFoldersDataAccess, Substitute.For<IUserFolderController>());

            const string Path = @"C:\Windows";

            var result = instance.ValidatePath(Path, Guid.Empty);

            result.Should().BeTrue();
            userFoldersDataAccess.Received().LoadUserFolders();
        }

        [Fact]
        public void ValidatePath_ValidPath_CurrentId_PopulatedRegistryWithMatchOnIdButNotPath_ReturnTrue()
        {
            var id = Guid.NewGuid();

            var userFoldersDataAccess = Substitute.For<IUserFoldersDataAccess>();
            userFoldersDataAccess.LoadUserFolders()
                .Returns(
                [
                    new UserFolder(id)
                    {
                        Alias = "example",
                        FolderPath = @"C:\example",
                    },
                ]);
            var userFolderController = Substitute.For<IUserFolderController>();
            var instance = new TheUserFoldersController(userFoldersDataAccess, userFolderController);

            const string Path = @"C:\Windows";

            var result = instance.ValidatePath(Path, id);

            result.Should().BeTrue();
            userFoldersDataAccess.Received().LoadUserFolders();
        }

        [Fact]
        public void ValidatePath_ValidPath_CurrentId_PopulatedRegistryWithMatchInPathButNotId_ReturnFalse()
        {
            var userFoldersDataAccess = Substitute.For<IUserFoldersDataAccess>();
            userFoldersDataAccess.LoadUserFolders()
                .Returns(
                [
                    new UserFolder
                    {
                        Alias = "bar",
                        FolderPath = @"C:\Windows",
                    },
                ]);
            var instance = new TheUserFoldersController(userFoldersDataAccess, Substitute.For<IUserFolderController>());

            const string Path = @"C:\Windows";
            var id = Guid.NewGuid();

            var result = instance.ValidatePath(Path, id);

            result.Should().BeFalse();
            userFoldersDataAccess.Received().LoadUserFolders();
        }

        [Fact]
        public void ValidatePath_ValidPath_CurrentId_PopulatedRegistryWithMatchInPathAndId_ReturnTrue()
        {
            var id = Guid.NewGuid();

            var userFoldersDataAccess = Substitute.For<IUserFoldersDataAccess>();
            userFoldersDataAccess.LoadUserFolders()
                .Returns(
                [
                    new UserFolder(id)
                    {
                        Alias = "bar",
                        FolderPath = @"C:\Windows",
                    },
                ]);

            var instance = new TheUserFoldersController(userFoldersDataAccess, Substitute.For<IUserFolderController>());

            const string Path = @"C:\Windows";

            var result = instance.ValidatePath(Path, id);

            result.Should().BeTrue();
            userFoldersDataAccess.Received().LoadUserFolders();
        }
    }
}
