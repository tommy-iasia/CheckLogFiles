using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogUpdater
{
    public class Configure
    {
        public string Server { get; set; }

        public string Identifier { get; set; }
        public string Version { get; set; }

        private const string FilePath = nameof(CheckLogUpdater) + ".json";
        public static async Task<Configure> LoadAsync()
        {
            var json = await File.ReadAllTextAsync(FilePath);

            return JsonSerializer.Deserialize<Configure>(json);
        }
        public async Task SaveAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);

            await File.WriteAllTextAsync(FilePath, json);
        }
    }
}
