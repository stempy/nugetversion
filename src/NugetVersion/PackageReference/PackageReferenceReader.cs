using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NugetVersion.Extensions;
using NugetVersion.Models;

namespace NugetVersion.PackageReference
{
    internal class PackageReferenceReader
    {
        public IEnumerable<PackageReferenceModel> GetPackages(XDocument doc)
        {
            return doc.Descendants().Elements("PackageReference").Select(x => new PackageReferenceModel()
            {
                Name = XmlXElementExtensions.GetXElementAttributeValueOrNull(x, PackageConstants.PackageNameAttr) ?? XmlXElementExtensions.GetXElementAttributeValueOrNull(x, "Update"),
                Version = XmlXElementExtensions.GetXElementAttributeValueOrNull(x, PackageConstants.PackageVersionAttr) ?? x.Element("Version")?.Value
            });
        }

        public IEnumerable<ProjectReferenceModel> GetProjectReferences(XDocument doc)
        {
            return doc.Descendants().Elements("ProjectReference").Select(x => new ProjectReferenceModel()
            {
                Include = x.GetXElementAttributeValueOrNull("Include")
            });
        }
    }
}