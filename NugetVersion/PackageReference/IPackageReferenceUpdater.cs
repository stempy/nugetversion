using System.Collections.Generic;
using NugetVersion.Models;

namespace NugetVersion.PackageReference
{
    public interface IPackageReferenceUpdater
    {
        IEnumerable<VersionUpdateResult> UpdateVersion(string projectFile, 
            string nameFilter, string versionFilter, string newVersion);
    }
}