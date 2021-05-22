using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NugetVersion.Models;
using NugetVersion.PackageReference;
using NugetVersion.Renderer;

namespace NugetVersion.Project
{
    // query across projects
    public class ProjectNugetVersionUpdater
    {
        private readonly PackageReferenceConsoleTableRenderer _renderer;
        private readonly PackageReferenceXmlMapper _mapper;
        private readonly IPackageReferenceUpdater _packageReferenceUpdater;
        public bool IncludeEmptyResults { get; set; }

        public ProjectNugetVersionUpdater(IPackageReferenceUpdater packageReferenceUpdater)
        {
            _renderer = new PackageReferenceConsoleTableRenderer();
            _mapper = new PackageReferenceXmlMapper();
            _packageReferenceUpdater = packageReferenceUpdater;
        }

        public IDictionary<string, IEnumerable<VersionUpdateResult>> UpdateVersionInProjects(IEnumerable<ProjectFile> projectFiles, string nameFilter, string versionFilter, string newVersion, bool suppressPrompts = false)
        {
            var d = new Dictionary<string, IEnumerable<VersionUpdateResult>>();
            foreach (var f in projectFiles)
            {
                var results = UpdateVersionInProject(f.Filename, nameFilter, versionFilter, newVersion, suppressPrompts);
                d.Add(f.Filename, results);
            }
            return d;
        }

        public IEnumerable<VersionUpdateResult> UpdateVersionInProject(string projectFile, 
                                                string nameFilter, string versionFilter, 
                                                string newVersion, bool suppressPrompt=false)
        {
            var doc = XDocument.Parse(File.ReadAllText(projectFile));
            var items = new PackageReferenceXmlReader(doc)
                                                .GetPackageReferenceElements(nameFilter, versionFilter);
            if (!items.Any())
                return null;

            var pr = _mapper.Map(items);

            // ok lets provide confirmation
            _renderer.RenderProjectResults(0, _renderer.GetMaxNumPad(pr), projectFile, pr);

            if (!suppressPrompt)
            {
                ConsoleRender.W($"Are you sure you want to change versions for {pr.Count()} packages to ")
                    .W($"{newVersion}", ConsoleColor.DarkMagenta).W(" ? Y/N: ");

                var inp = Console.ReadLine();
                if (!inp.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                    return null;
            }

            return _packageReferenceUpdater.UpdateVersion(projectFile, nameFilter, versionFilter, newVersion);
        }
    }
}