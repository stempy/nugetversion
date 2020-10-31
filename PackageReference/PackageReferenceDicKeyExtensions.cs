using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace nugetversion.PackageReference
{
    public static class PackageReferenceDicKeyExtensions
    {
        public static IEnumerable<string> AsProjectList(this IEnumerable<KeyValuePair<string, IEnumerable<PackageReference>>> kv)
        {
            return kv.Select(x => x.Key);
        }

        public static string AsJson(this IEnumerable<KeyValuePair<string, IEnumerable<PackageReference>>> kv)
        {
            return JsonSerializer.Serialize(kv, new JsonSerializerOptions() { WriteIndented = true });
        }
    }

}
