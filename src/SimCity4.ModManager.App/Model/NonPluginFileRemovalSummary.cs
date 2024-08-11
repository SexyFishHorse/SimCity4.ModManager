using System;
using System.Collections.Generic;

namespace SimCity4.ModManager.App.Model;

public class NonPluginFileRemovalSummary
{
    public int NumFilesRemoved { get; set; }

    public int NumFoldersRemoved { get; set; }

    public Dictionary<string, Exception> Errors { get; set; }
}