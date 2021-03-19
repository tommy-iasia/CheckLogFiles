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
        public static async Task RunAsync()
        {
            var configure = await Configure.LoadAsync();

            using var client = new HttpClient();
            var updates = await client.GetFromJsonAsync<Update[]>($"{configure.Address}/Update/After?lastId={configure.Version}");

            if (!updates.Any())
            {
                Console.WriteLine("It is the most updated version now");
                return;
            }

            var update = updates.OrderBy(t => t.Version).First();
            Console.WriteLine($"Update {update.Version} of {update.Id} will be applied...");

            await DownloadAsync(configure.Address, update);

            RunProcess(update);
        }

        public static string GetDirectory(string version) => Path.Combine("Update", version);
        private static async Task DownloadAsync(string server, Update update)
        {
            using var client = new HttpClient();
            var uri = $"{server}/Update/Download?id={update.Id}";
            var names = await client.GetFromJsonAsync<string[]>(uri);

            Console.WriteLine($"{names.Length} files to be downloaded...");

            var updateDirectory = GetDirectory(update.Version);

            foreach (var name in names)
            {
                Console.WriteLine($"Download {name}");
                var bytes = await client.GetByteArrayAsync($"{uri}&name={HttpUtility.UrlEncode(name)}");

                var path = Path.Combine(updateDirectory, name);

                var fileDirectory = Path.GetDirectoryName(path);
                Directory.CreateDirectory(fileDirectory);

                await File.WriteAllBytesAsync(path, bytes);
            }

            Console.WriteLine("Place files...");

            foreach (var name in names)
            {
                var path = Path.Combine(updateDirectory, name);

                Console.WriteLine($"Place {name}");
                File.Copy(path, name);
            }
        }

        private static void RunProcess(Update update)
        {
            var argument = $"{nameof(Continue)} {update.Version}";
            Console.WriteLine($"Run command: {update.Command} {argument}");

            var workingDirectory = Path.GetFullPath(".");
            Console.WriteLine($"Working directory: {workingDirectory}");

            var processInfo = new ProcessStartInfo(update.Command, argument)
            {
                WorkingDirectory = workingDirectory
            };

            Process.Start(processInfo);
        }
    }
}
