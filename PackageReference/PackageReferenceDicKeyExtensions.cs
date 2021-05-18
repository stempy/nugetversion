using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace NugetVersion.PackageReference
{
    public static class PackageReferenceDicKeyExtensions
    {
        public static IEnumerable<string> AsProjectList(this IEnumerable<KeyValuePair<string, IEnumerable<PackageReferenceModel>>> kv)
        {
            return kv.Select(x => x.Key);
        }

        public static string AsJson(this IEnumerable<KeyValuePair<string, IEnumerable<PackageReferenceModel>>> kv)
        {
            return JsonSerializer.Serialize(kv, new JsonSerializerOptions() { WriteIndented = true });
        }
    }

}
