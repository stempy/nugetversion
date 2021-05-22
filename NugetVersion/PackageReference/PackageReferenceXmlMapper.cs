using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NugetVersion.Extensions;
using NugetVersion.Models;

namespace NugetVersion.PackageReference
{
    internal class PackageReferenceXmlMapper
    {
        public IEnumerable<PackageReferenceModel> Map(IEnumerable<XElement> items)
        {
            return items.Select(x => new PackageReferenceModel()
            {
                Name = x.GetXElementAttributeValueOrNull(PackageConstants.PackageNameAttr) ?? x.GetXElementAttributeValueOrNull("Update"),
                Version = x.GetXElementAttributeValueOrNull(PackageConstants.PackageVersionAttr) ?? x.Element("Version")?.Value
            });
        }

    }
}