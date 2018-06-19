using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace nugetversion
{
    public static class PackageReferenceDicKeyExtensions
{
	public static IEnumerable<string> AsProjectList(this IEnumerable<KeyValuePair<string,IEnumerable<PackageReference>>> kv)
	{
		return kv.Select(x=>x.Key);
	}

	public static string AsJson(this IEnumerable<KeyValuePair<string,IEnumerable<PackageReference>>> kv)
	{
		return JsonConvert.SerializeObject(kv, Newtonsoft.Json.Formatting.Indented);
	}
}

}
