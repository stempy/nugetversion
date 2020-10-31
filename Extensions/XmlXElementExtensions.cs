using System.Xml.Linq;

namespace nugetversion.Extensions
{
    public static class XmlXElementExtensions
	{
		public static string GetXElementAttributeValueOrNull(this XElement el, string attributeName)
		{
			return el.Attribute(attributeName)!=null? el.Attribute(attributeName).Value:null;
		}
	}
}
