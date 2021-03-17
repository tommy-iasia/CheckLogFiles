using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogServer.Models
{
    public class JsonSaver<T>
    {
        public JsonSaver(string filePath) => this.filePath = filePath;
        private readonly string filePath;

        private string TransactionPath => $"{filePath}.transaction";

        protected bool Loaded { get; private set; } = false;
        public virtual async Task<T> LoadAsync()
        {
            if (Loaded)
            {
                return Value;
            }
            else
            {
                if (File.Exists(TransactionPath))
                {
                    await LoadAsync(TransactionPath);
                }
                else if (File.Exists(filePath))
                {
                    await LoadAsync(filePath);
                }
                else
                {
                    Value = default;
                    Loaded = true;
                }

                return Value;
            }
        }
        private async Task LoadAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);

            Value = JsonSerializer.Deserialize<T>(json);

            Loaded = true;
        }

        protected T Value { get; set; }
        public async Task SaveAsync()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException();
            }

            await SaveAsync(Value);
        }
        public async Task SaveAsync(T value)
        {
            this.Value = value;
            Loaded = true;

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions();
            options.WriteIndented = true;

            var json = JsonSerializer.Serialize(value, options);

            await File.WriteAllTextAsync(TransactionPath, json);

            await File.WriteAllTextAsync(filePath, json);

            File.Delete(TransactionPath);
        }
    }
}
