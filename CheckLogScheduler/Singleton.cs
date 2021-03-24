using System;
using System.IO;
using System.Threading.Tasks;

namespace CheckLogScheduler
{
    public class Singleton
    {
        public readonly Guid Guid = Guid.NewGuid();

        private const string fileName = nameof(CheckLogScheduler) + "." + nameof(Singleton) + ".guid";

        public static async Task<Singleton> GainAsync()
        {
            var singleton = new Singleton();

            await File.WriteAllTextAsync(fileName, singleton.Guid.ToString());

            return singleton;
        }
        public async Task<(bool valid, Guid guid)> ValidateAsync()
        {
            var guidText = await File.ReadAllTextAsync(fileName);
            var guid = Guid.Parse(guidText);

            return (this.Guid == guid, guid);
        }
    }
}
