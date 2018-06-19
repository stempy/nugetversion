using System.Xml.Linq;

namespace nugetversion
{
    public static class XmlXElementExtensions
	{
		public static string GetXElementAttributeValueOrNull(this XElement el, string attributeName)
		{
			return el.Attribute(attributeName)!=null? el.Attribute(attributeName).Value:null;
		}
	}
}
