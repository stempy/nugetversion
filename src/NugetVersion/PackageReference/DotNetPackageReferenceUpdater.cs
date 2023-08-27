using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using NugetVersion.GetNugetVersions;
using NugetVersion.Models;
using NugetVersion.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NugetVersion.PackageReference
{
    public class DotNetPackageReferenceUpdater : IPackageReferenceUpdater
    {
        private readonly PackageReferenceXmlMapper _mapper;
        private readonly NugetPackageVersionUtil _nugetPackageVersionUtil;
        private readonly ILogger<DotNetPackageReferenceUpdater> _logger;

        public DotNetPackageReferenceUpdater(NugetPackageVersionUtil nugetPackageVersionUtil, ILogger<DotNetPackageReferenceUpdater> logger)
        {
            _mapper = new PackageReferenceXmlMapper();
            _nugetPackageVersionUtil = nugetPackageVersionUtil;
            _logger = logger;
        }


        public async Task<IEnumerable<VersionUpdateResult>> UpdateVersion(string projectFile,
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
                string versionToUpdateTo = newVersion;

                if (newVersion.Equals("latest", StringComparison.InvariantCultureIgnoreCase))
                {
                    // TODO: 27/08/2023 - APPLY LATEST VERSION update here.
                    var latestVersion = await _nugetPackageVersionUtil.GetLatestNugetPackageVersionAsync(i.Name);
                    var currentVersion = new NuGetVersion(i.Version);

                    if (currentVersion < latestVersion)
                    {
                        versionToUpdateTo = latestVersion.ToFullString();
                    }
                    else
                    {
                        versionToUpdateTo = i.Version;
                    }
                }

                var message = "";
                if (versionToUpdateTo != i.Version)
                {
                    // use dotnet to upgrade package, takes care of dependencies than
                    var args = $"add \"{projectFile}\" package \"{i.Name}\" -v \"{versionToUpdateTo}\"";
                    message += "ExitCode:" + simpleExec.Exec("dotnet", args);
                }
                else
                {
                    message = "SAME_VERSION";
                }


                versionUpdResults.Add(new VersionUpdateResult()
                {
                    Name = i.Name,
                    OriginalVersion = i.Version,
                    NewVersion = newVersion,
                    Message = message
                });
            }

            return versionUpdResults;
        }
    }
}