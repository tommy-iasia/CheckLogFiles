using CheckLogUtility.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace CheckLogUpdater
{
    public static class Prepare
    {
        public static async Task RunAsync(Logger logger)
        {
            var configure = await Configure.LoadAsync();

            using var client = new HttpClient();
            var updates = await client.GetFromJsonAsync<Update[]>($"{configure.Server}/Update/After?identifier={configure.Identifier}&version={configure.Version}");

            if (!updates.Any())
            {
                await logger.InfoAsync("It is the most updated version now");
                return;
            }

            var update = updates.OrderBy(t => t.Version).First();
            await logger.InfoAsync($"Update {update.Version} of {update.Id} will be applied...");

            await DownloadAsync(configure.Server, update, logger);

            await RunProcessAsync(update, logger);
        }

        public static string GetDirectory(string version) => Path.Combine("Update", version);
        private static async Task DownloadAsync(string server, Update update, Logger logger)
        {
            using var client = new HttpClient();
            var uri = $"{server}/Update/Download?id={update.Id}";
            var names = await client.GetFromJsonAsync<string[]>(uri);

            await logger.InfoAsync($"{names.Length} files to be downloaded...");

            var updateDirectory = GetDirectory(update.Version);

            foreach (var name in names)
            {
                await logger.InfoAsync($"Download {name}");
                var bytes = await client.GetByteArrayAsync($"{uri}&name={HttpUtility.UrlEncode(name)}");

                var path = Path.Combine(updateDirectory, name);

                var fileDirectory = Path.GetDirectoryName(path);
                Directory.CreateDirectory(fileDirectory);

                await File.WriteAllBytesAsync(path, bytes);
            }

            await logger.InfoAsync("Place files...");

            foreach (var name in names)
            {
                var path = Path.Combine(updateDirectory, name);

                await logger.InfoAsync($"Place {name}");
                File.Copy(path, name);
            }
        }

        private static async Task RunProcessAsync(Update update, Logger logger)
        {
            var argument = $"{nameof(Continue)} {update.Version}";
            await logger.InfoAsync($"Run command: {update.Command} {argument}");

            var workingDirectory = Path.GetFullPath(".");
            await logger.InfoAsync($"Working directory: {workingDirectory}");

            var processInfo = new ProcessStartInfo(update.Command, argument)
            {
                WorkingDirectory = workingDirectory
            };

            Process.Start(processInfo);
        }
    }
}
