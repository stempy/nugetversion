using NugetVersion.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NugetVersion.PackageReference
{
    public interface IPackageReferenceUpdater
    {
        Task<IEnumerable<VersionUpdateResult>> UpdateVersion(string projectFile,
            string nameFilter, string versionFilter, string newVersion);
    }
}