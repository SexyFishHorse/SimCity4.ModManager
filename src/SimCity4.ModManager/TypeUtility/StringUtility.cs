using System;
using System.Text;

namespace SimCity4.ModManager.TypeUtility;

public static class StringUtility
{
    private static readonly Random Random = new((int)DateTime.Now.Ticks);

    /// <summary>
    /// Generates a random alphanumeric string of the letters a-z, A-Z and the numbers 0-9.
    /// </summary>
    /// <param name="length">The length of the generated string.</param>
    /// <returns>A random alphanumeric string.</returns>
    public static string GenerateRandomAlphaNumericString(int length)
    {
        const string Characters = "abcdefghijklnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var builder = new StringBuilder();
        for (var i = 0; i < length; i++)
        {
            var randomValue = Random.Next(0, Characters.Length);
            builder.Append(Characters.Substring(randomValue, 1));
        }

        return builder.ToString();
    }
}