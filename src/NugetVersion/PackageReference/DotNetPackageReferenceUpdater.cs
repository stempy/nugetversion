using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NugetVersion.Models;
using NugetVersion.Utils;

namespace NugetVersion.PackageReference
{
    public class DotNetPackageReferenceUpdater : IPackageReferenceUpdater
    {
        private readonly PackageReferenceXmlMapper _mapper;

        public DotNetPackageReferenceUpdater()
        {
            _mapper = new PackageReferenceXmlMapper();
        }
        
        
        public IEnumerable<VersionUpdateResult> UpdateVersion(string projectFile, 
            string nameFilter, string versionFilter, string newVersion)
        {
            var doc = XDocument.Parse(File.ReadAllText(projectFile));
            var items = new PackageReferenceXmlReader(doc)
                .GetPackageReferenceElements(nameFilter, versionFilter);
            if (!items.Any())
                return null;

            var pr = _mapper.Map(items);

            var simpleExec = new SimpleExec();

            var versionUpdResults = new List<VersionUpdateResult>();

            foreach (var i in pr)
            {
                // use dotnet to upgrade package, takes care of dependencies than
                var args = $"add \"{projectFile}\" package \"{i.Name}\" -v \"{newVersion}\"";
                var resulting = simpleExec.Exec("dotnet", args);

                versionUpdResults.Add(new VersionUpdateResult()
                {
                    Name = i.Name,
                    OriginalVersion = i.Version,
                    NewVersion = newVersion,
                    Message = $"ExitCode:{resulting}"
                });
            }

            return versionUpdResults;
        }
    }
}