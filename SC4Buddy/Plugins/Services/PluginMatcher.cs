namespace Nihei.SC4Buddy.Plugins.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using Asser.Sc4Buddy.Server.Api.V1.Client;
    using Asser.Sc4Buddy.Server.Api.V1.Models;
    using log4net;
    using MoreLinq;
    using Nihei.SC4Buddy.Model;
    using Plugin = Asser.Sc4Buddy.Server.Api.V1.Models.Plugin;

    public class PluginMatcher : IPluginMatcher
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBuddyServerClient client;

        private IEnumerable<Plugin> plugins;

        private IEnumerable<File> files;

        public PluginMatcher(IBuddyServerClient client)
        {
            this.client = client;

            ReloadData();
        }

        public void ReloadData()
        {
            Log.Info("Fetching all plugins and files from the server.");
            plugins = client.GetAllPlugins().ToList();
            files = client.GetAllFiles().ToList();
        }

        public Plugin GetMostLikelyPluginForGroupOfFiles(IEnumerable<PluginFile> fileInfos)
        {
            var matchedPlugins = new Dictionary<Guid, int>();

            foreach (var fileInfo in fileInfos)
            {
                var fileInfoClosure = fileInfo;
                var matchedPlugin = Guid.Empty;
                foreach (var file in files.Where(x => x.Filename == fileInfoClosure.Filename && x.Checksum == fileInfoClosure.Checksum))
                {
                    matchedPlugin = file.Plugin;
                    break;
                }

                if (matchedPlugin == Guid.Empty)
                {
                    continue;
                }

                if (matchedPlugins.ContainsKey(matchedPlugin))
                {
                    matchedPlugins[matchedPlugin] = matchedPlugins[matchedPlugin]++;
                }
                else
                {
                    matchedPlugins.Add(matchedPlugin, 1);
                }
            }

            return matchedPlugins.Any() ? plugins.First(x => x.Id == matchedPlugins.MaxBy(y => y.Value).Key) : null;
        }

        public IDictionary<PluginFile, Plugin> GetMostLikelyPluginForEachFile(ICollection<PluginFile> inputFiles, BackgroundWorker backgroundWorker)
        {
            var output = new Dictionary<PluginFile, Plugin>();
            var numFiles = inputFiles.Count();
            var filesProcessed = 0.0;

            foreach (var inputFile in inputFiles)
            {
                if (backgroundWorker.CancellationPending)
                {
                    return output;
                }

                var matchedFile = files.FirstOrDefault(x => x.Filename == inputFile.Filename && x.Checksum == inputFile.Checksum);
                if (matchedFile != null)
                {
                    var plugin = plugins.First(x => x.Id == matchedFile.Plugin);
                    output.Add(inputFile, plugin);
                }

                filesProcessed++;

                backgroundWorker.ReportProgress(
                    (int)Math.Floor(filesProcessed / numFiles * 95),
                    string.Format("Checked {0} out of {1} files", filesProcessed, numFiles));
            }

            return output;
        }
    }
}
