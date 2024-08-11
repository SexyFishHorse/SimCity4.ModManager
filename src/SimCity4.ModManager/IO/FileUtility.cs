using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace SimCity4.ModManager.IO;

using SearchOption = System.IO.SearchOption;

public static class FileUtility
{
    /// <summary>
    /// Copies a directory and of its content from one location to another.
    /// Allows for copying across volumes.
    /// </summary>
    /// <param name="source">The path where the files are located.</param>
    /// <param name="target">The path the folder will be copied to.</param>
    public static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
    {
        // Check if the target directory exists, if not, create it.
        if (Directory.Exists(target.FullName) == false)
        {
            Directory.CreateDirectory(target.FullName);
        }

        // Copy each file into it’s new directory.
        foreach (var file in source.GetFiles())
        {
            file.CopyTo(Path.Combine(target.ToString(), file.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (var directory in source.GetDirectories())
        {
            var nextTargetSubDir =
                target.CreateSubdirectory(directory.Name);
            CopyFolder(directory, nextTargetSubDir);
        }
    }

    /// <summary>
    /// Deletes a directory and all of its content.
    /// </summary>
    /// <param name="path">
    /// The directory to delete.
    /// </param>
    /// <param name="sendToRecycleBin">
    /// If set to true the deleted files and folders will be sent to 
    /// the recycle bin instead of being permanently deleted.
    /// </param>
    public static void DeleteFolder(string path, bool sendToRecycleBin = false)
    {
        DeleteFolder(new DirectoryInfo(path), sendToRecycleBin);
    }

    /// <summary>
    /// Deletes a directory and all of its content.
    /// </summary>
    /// <param name="path">
    /// The directory to delete.
    /// </param>
    /// <param name="sendToRecycleBin">
    /// If set to true the deleted files and folders will be sent to 
    /// the recycle bin instead of being permanently deleted.
    /// </param>
    public static void DeleteFolder(DirectoryInfo path, bool sendToRecycleBin = false)
    {
        var files = path.EnumerateFiles("*", SearchOption.AllDirectories);
        var directories = path.EnumerateDirectories("*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            file.Attributes = FileAttributes.Normal;
            FileSystem.DeleteFile(
                file.FullName,
                UIOption.OnlyErrorDialogs,
                sendToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently);
        }

        foreach (var directory in directories)
        {
            directory.Delete(true);
        }

        path.Delete();
    }
}