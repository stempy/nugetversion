using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace nugetversion
{
    // query XDocument Package references
    internal class PackageReferenceQuery
    {

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

        /// <summary>
        /// filter package references by name
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="packageNameSpec"></param>
        /// <returns></returns>
        public IEnumerable<XElement> FilterPackageReferences(IEnumerable<XElement> pr, string packageNameSpec, string versionSpec)
        {
            IEnumerable<XElement> l = pr.ToList();
            if (!string.IsNullOrEmpty(packageNameSpec))
            {
                l = FilterByAttribute(l, PackageConstants.PackageNameAttr, packageNameSpec);
            }

            if (!string.IsNullOrEmpty(versionSpec))
            {
                l = FilterByAttribute(l, PackageConstants.PackageVersionAttr, versionSpec);
            }
            return l;
        }

        public IEnumerable<XElement> QueryP(XDocument doc, string nameFilter, string versionFilter)
        {
            var pr = doc.Descendants().Elements("PackageReference");
            if (!string.IsNullOrEmpty(nameFilter) || !string.IsNullOrEmpty(versionFilter))
            {
                pr = FilterPackageReferences(pr, nameFilter, versionFilter);
            }

            return pr;
        }

        public IEnumerable<XElement> QueryP(XDocument doc)
        {
            return QueryP(doc, null, null);
        }
    }

}
