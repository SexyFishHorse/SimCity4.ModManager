using System;
using Newtonsoft.Json;

namespace SimCity4.ModManager.App.Model;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ModelBase(Guid? id = null)
{
    [JsonProperty]
    public Guid Id { get; } = id ?? Guid.NewGuid();

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is ModelBase other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    protected bool Equals(ModelBase other)
    {
        return Id.Equals(other.Id);
    }
}