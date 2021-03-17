using System;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogServer.Models
{
    public class ArraySaver<T> : JsonSaver<T[]>
    {
        public ArraySaver(string filePath) : base(filePath) { }

        public override async Task<T[]> LoadAsync()
        {
            await base.LoadAsync();

            if (Value == null)
            {
                Value = Array.Empty<T>();
            }

            return Value;
        }

        public void Add(T item)
        {
            if (!Loaded)
            {
                throw new InvalidOperationException();
            }

            Value = Value.Append(item).ToArray();
        }
    }
}
