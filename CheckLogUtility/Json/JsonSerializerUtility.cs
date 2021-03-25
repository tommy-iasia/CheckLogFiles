using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace CheckLogUtility.Json
{
    public class JsonSerializerUtility
    {
        public static string Combine(params string[] jsons)
        {
            if (!jsons.Any())
            {
                return "{}";
            }

            var dictionaries = jsons.Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t));
            
            var output = dictionaries.Aggregate(new Dictionary<string, object>(), (total, current) =>
            {
                foreach (var (key, value) in current)
                {
                    total[key] = value;
                }

                return total;
            });

            return JsonSerializer.Serialize(output);
        }
    }
}
