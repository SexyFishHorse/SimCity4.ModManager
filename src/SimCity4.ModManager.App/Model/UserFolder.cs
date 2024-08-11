using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace SimCity4.ModManager.App.Model;

[JsonObject(MemberSerialization.OptIn)]
public class UserFolder(Guid? id = null) : ModelBase(id)
{
    public const string PluginFolderName = "Plugins";

    public static IEqualityComparer<UserFolder> AliasComparer { get; } = new AliasEqualityComparer();

    [JsonProperty]
    public string Alias { get; set; }

    [JsonProperty]
    public string FolderPath { get; set; }

    [JsonProperty]
    public bool IsMainFolder { get; set; }

    [JsonProperty]
    public bool IsStartupFolder { get; set; }

    public string PluginFolderPath => Path.Combine(FolderPath, PluginFolderName);

    public ICollection<Plugin> Plugins { get; set; } = new Collection<Plugin>();

    private sealed class AliasEqualityComparer : IEqualityComparer<UserFolder>
    {
        public bool Equals(UserFolder x, UserFolder y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return string.Equals(x.Alias, y.Alias);
        }

        public int GetHashCode(UserFolder obj)
        {
            return obj.Alias != null ? obj.Alias.GetHashCode() : 0;
        }
    }
}