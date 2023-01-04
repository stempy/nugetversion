using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NugetVersion.Models;

namespace NugetVersion.PackageReference
{
    public class XmlPackageReferenceUpdater : IPackageReferenceUpdater
    {
        public bool IgnoreNullVersions { get; set; } = true;

        public XmlPackageReferenceUpdater()
        {
            
        }
        
        public IEnumerable<VersionUpdateResult> UpdateVersion(string projectFile, string nameFilter, string versionFilter, 
            string newVersion)
        {
            var doc = XDocument.Parse(File.ReadAllText(projectFile));
            var updResults = UpdateVersion(doc, nameFilter, versionFilter, newVersion);
            using (XmlTextWriter xtw = new XmlTextWriter(projectFile, Encoding.UTF8))
            {
                xtw.Formatting = Formatting.Indented; // optional, if you want it to look nice
                doc.WriteTo(xtw);
            }
            return updResults;
        }

        public IEnumerable<VersionUpdateResult> UpdateVersion(XDocument doc, string nameFilter, string versionFilter,
            string newVersion)
        {
            var xmlReader = new PackageReferenceXmlReader(doc);
            var fndResults = xmlReader.GetPackageReferenceElements(nameFilter, versionFilter);
            var result = SetPackageVersion(fndResults, newVersion, out var updResults);
            return updResults;
        }

        
        private IEnumerable<XElement> SetPackageVersion(IEnumerable<XElement> packageRefEl,
            string newVersion,
            out IEnumerable<VersionUpdateResult> updateResults)
        {
            packageRefEl = packageRefEl.ToList();
            var updateResultsList = new List<VersionUpdateResult>();

            foreach (var element in packageRefEl)
            {
                var result = UpdateElementPackageVersion(newVersion, IgnoreNullVersions, element);
                if (result != null)
                {
                    updateResultsList.Add(result);
                }
            }

            updateResults = updateResultsList;
            return packageRefEl;
        }

        private VersionUpdateResult UpdateElementPackageVersion(string newVersion, bool ignoreNullVersions, XElement element)
        {
            var name = element.Attribute(PackageConstants.PackageNameAttr)?.Value;
            var attr = element.Attribute(PackageConstants.PackageVersionAttr);
            if (attr == null)
            {
                if (ignoreNullVersions)
                {
                    return null;
                }
                else
                {
                    throw new NullReferenceException($"No original version Specified on {name} element");
                }
            }

            var originalValue = attr.Value;
            attr.Value = newVersion;

            Log($"{name} version {originalValue} ==> {newVersion}");
            return new VersionUpdateResult()
            {
                Name = name,
                OriginalVersion = originalValue,
                NewVersion = newVersion
            };
        }

        

        private void Log(string msg)
        {
            //msg.Dump();
        }

        private void LogDebug(string msg)
        {
            //msg.Dump();
        }


    }
}