using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogRunner
{
    public class Configure
    {
        public string Time { get; set; }
        public string[] Arguments { get; set; }

        public static async Task<Configure[]> LoadAsync(IEnumerable<string> filePaths)
        {
            var loads = filePaths.Select(LoadAsync).ToArray();
            var configures = await Task.WhenAll(loads);

            return configures.SelectMany(t => t).ToArray();
        }
        public static async Task<Configure[]> LoadAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<Configure[]>(json);
        }
    }
}
