using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SimCity4.ModManager.IO;

public static class Md5ChecksumUtility
{
    /// <summary>
    /// Converts a byte array to a hex representation string.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <param name="upperCase">Indicates if the hex representation should be in upper or lowercase.</param>
    /// <returns>The hex representation of the supplied byte array.</returns>
    public static string ToHex(this byte[] bytes, bool upperCase = false)
    {
        var result = new StringBuilder(bytes.Length * 2);

        for (var index = 0; index < bytes.Length; index++)
        {
            result.Append(bytes[index].ToString(upperCase ? "X2" : "x2"));
        }

        return result.ToString();
    }

    /// <summary>
    /// Calculates the md5 checksum for a file.
    /// </summary>
    /// <param name="file">The file the checksum should be calculated for.</param>
    /// <returns>The checksum as a byte array.</returns>
    public static byte[] CalculateChecksum(FileInfo file)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(file.FullName))
            {
                return md5.ComputeHash(stream);
            }
        }
    }

    /// <summary>
    /// Calculates the md5 checksum for a file.
    /// </summary>
    /// <param name="path">The path to the file the checksum should be calculated for.</param>
    /// <returns>The checksum as a byte array.</returns>
    public static byte[] CalculateChecksum(string path)
    {
        return CalculateChecksum(new FileInfo(path));
    }
}