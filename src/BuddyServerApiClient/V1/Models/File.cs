namespace Asser.Sc4Buddy.Server.Api.V1.Models;

using System;

public class File
{
    public Guid Id { get; set; }

    public string Filename { get; set; }

    public string Checksum { get; set; }

    public Guid Plugin { get; set; }
}