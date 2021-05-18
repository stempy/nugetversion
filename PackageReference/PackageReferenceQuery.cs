using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NugetVersion.PackageReference
{
    // query XDocument Package references
    internal class PackageReferenceQuery
    {
        private readonly string _filename;
        private XDocument _xdoc;

        public PackageReferenceQuery(string filename)
        {
            _filename = filename;
        }

        public PackageReferenceQuery(XDocument xdoc)
        {
            _xdoc = xdoc;
        }

        public IEnumerable<XElement> QueryPackages(string nameFilter, string versionFilter)
        {
            var pr = GetXDoc().Descendants().Elements("PackageReference");
            if (!string.IsNullOrEmpty(nameFilter) || !string.IsNullOrEmpty(versionFilter))
            {
                pr = FilterPackageReferences(pr, nameFilter, versionFilter);
            }
            return pr;
        }

        private XDocument GetXDoc()
        {
            if (_xdoc == null)
            {
                if (!string.IsNullOrEmpty(_filename))
                {
                    _xdoc = XDocument.Parse(File.ReadAllText(_filename));
                }
            }
            
            return _xdoc;
        }

        /// <summary>
        /// filter package references by name
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="packageNameSpec"></param>
        /// <returns></returns>
        private IEnumerable<XElement> FilterPackageReferences(IEnumerable<XElement> pr, string packageNameSpec, string versionSpec)
        {
            IEnumerable<XElement> l = pr.ToList();
            if (!string.IsNullOrEmpty(packageNameSpec))
            {
                l = FilterByAttribute(l, PackageConstants.PackageNameAttr, packageNameSpec);
            }

            if (!string.IsNullOrEmpty(versionSpec))
            {
                l = FilterVersion(l, versionSpec);
            }
            return l;
        }

        private IEnumerable<XElement> FilterVersion(IEnumerable<XElement> pr, string version)
        {
            if (!string.IsNullOrEmpty(version))
            {
                pr = FilterByAttribute(pr, PackageConstants.PackageVersionAttr, version);
                // TODO: this may wipe out before one
                //pr = FilterByChildElement(pr,PackageConstants.PackageVersionAttr,version);
            }
            return pr;
        }

        // If you want to implement both "*" and "?"
        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        private static IEnumerable<XElement> FilterByAttribute(IEnumerable<XElement> pr, string attribName, string attribVal)
        {
            IEnumerable<XElement> newList = pr.ToList();

            if (!string.IsNullOrEmpty(attribVal))
            {
                if (attribVal.Contains("*") || attribVal.Contains("?"))
                {
                    // wildcard name
                    var regexPattern = WildCardToRegular(attribVal);
                    var r = new Regex(regexPattern, RegexOptions.IgnoreCase);
                    newList = newList.Where(u => r.IsMatch(u.Attribute(attribName) != null ? u.Attribute(attribName).Value : ""));
                }
                else
                {
                    // specific name
                    newList = pr.Where(u => u.Attribute(attribName) != null && u.Attribute(attribName).Value.Contains(attribVal));
                }
            }
            return newList;
        }

        private static IEnumerable<XElement> FilterByChildElement(IEnumerable<XElement> pr, string elementName, string elVal)
        {
            if (!string.IsNullOrEmpty(elVal))
            {
                if (elVal.Contains("*") || elVal.Contains("?"))
                {
                    // wildcard name
                    var regexPattern = WildCardToRegular(elVal);
                    var r = new Regex(regexPattern, RegexOptions.IgnoreCase);
                    pr = pr.Where(u => r.IsMatch(u.Element(elementName) != null ? u.Element(elementName).Value : ""));
                }
                else
                {
                    // specific name
                    pr = pr.Where(u => u.Element(elementName) != null && u.Element(elementName).Value.Contains(elVal));
                }
            }
            return pr;
        }



    }
}
