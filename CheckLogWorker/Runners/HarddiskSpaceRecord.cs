using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class HarddiskSpaceRecord
    {
        public string Name { get; set; }

        public long AvailableFreeSpace { get; set; }
        public long TotalFreeSpace { get; set; }
        public long TotalSize { get; set; }

        public DateTime Time { get; set; }

        public override string ToString() => $"Drive {Name} with {AvailableFreeSpace:#,##0}B available within total {TotalSize:#,##0}B at {Time}";

        public const string FileName = nameof(HarddiskSpaceRecord) + "s.json";
        public static async Task<HarddiskSpaceRecord[]> LoadAsync()
        {
            if (!File.Exists(FileName))
            {
                return Array.Empty<HarddiskSpaceRecord>();
            }

            var json = await File.ReadAllTextAsync(FileName);
            return JsonSerializer.Deserialize<HarddiskSpaceRecord[]>(json);
        }
        public static async Task SaveAsync(HarddiskSpaceRecord[] records)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(records, options);

            await File.WriteAllTextAsync(FileName, json);
        }
    }
}
