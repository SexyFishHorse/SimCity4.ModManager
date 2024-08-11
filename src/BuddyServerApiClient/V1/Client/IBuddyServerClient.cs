namespace Asser.Sc4Buddy.Server.Api.V1.Client;

using System;
using System.Collections.Generic;
using Asser.Sc4Buddy.Server.Api.V1.Models;

public interface IBuddyServerClient
{
    IEnumerable<File> GetAllFiles();

    Plugin GetPlugin(Guid pluginId);

    IEnumerable<Plugin> GetAllPlugins();
}