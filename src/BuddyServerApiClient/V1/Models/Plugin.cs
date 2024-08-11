namespace Asser.Sc4Buddy.Server.Api.V1.Models;

using System;
using System.Collections.Generic;

public class Plugin
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Author { get; set; }

    public string Link { get; set; }

    public string Description { get; set; }

    public List<File> Files { get; set; }

    public List<Guid> Dependencies { get; set; }
}