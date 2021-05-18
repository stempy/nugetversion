using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NugetVersion.PackageReference
{
    // update xdocument package reference versions
    public class PackageReferenceVersionUpdate
    {

        private void Log(string msg)
        {
            //msg.Dump();
        }

        private void LogDebug(string msg)
        {
            //msg.Dump();
        }


        public IEnumerable<XElement> SetPackageVersion(IEnumerable<XElement> pr, string newVersion, out IEnumerable<VersionUpdateResult> updateResults, bool ignoreNullVersions)
        {
            pr = pr.ToList();
            var updateResultsList = new List<VersionUpdateResult>();

            foreach (var e in pr)
            {
                var name = e.Attribute(PackageConstants.PackageNameAttr)?.Value;
                var attr = e.Attribute(PackageConstants.PackageVersionAttr);
                if (attr == null)
                {
                    if (ignoreNullVersions)
                    {
                        continue;
                    }
                    else
                    {
                        throw new NullReferenceException($"No original version Specified on {name} element");
                    }
                }

                var originalValue = attr.Value;
                attr.Value = newVersion;

                updateResultsList.Add(new VersionUpdateResult()
                {
                    Name = name,
                    OriginalVersion = originalValue,
                    NewVersion = newVersion
                });
                Log($"{name} version {originalValue} ==> {newVersion}");
            }

            updateResults = updateResultsList;
            return pr;
        }
    }

}
