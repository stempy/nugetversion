using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NugetVersion.Extensions
{
    public static class XmlXElementExtensions
	{
		public static string GetXElementAttributeValueOrNull(this XElement el, string attributeName)
		{
			return el?.Attribute(attributeName)?.Value;
		}

        public static string FindElementKeyValueOrNull(this IEnumerable<XElement> els, string key)
        {
            return els.FirstOrDefault(x => x.Name == key)?.Value;
        }


	}
}
