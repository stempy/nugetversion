using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NugetVersion.Extensions;
using NugetVersion.PackageReference;

namespace NugetVersion
{
    internal class PackageReferenceXElementMapper
    {
        public IEnumerable<PackageReferenceModel> Map(IEnumerable<XElement> items)
        {
            return items.Select(x => new PackageReferenceModel()
            {
                Name = XmlXElementExtensions.GetXElementAttributeValueOrNull(x, PackageConstants.PackageNameAttr) ?? XmlXElementExtensions.GetXElementAttributeValueOrNull(x, "Update"),
                Version = XmlXElementExtensions.GetXElementAttributeValueOrNull(x, PackageConstants.PackageVersionAttr) ?? x.Element("Version")?.Value
            });
        }

    }
}