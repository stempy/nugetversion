using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NugetVersion.Extensions;
using NugetVersion.PackageReference;

namespace NugetVersion
{
    // query across projects
    public class ProjectNugetVersionUpdater
    {
        private readonly PackageReferenceVersionUpdate _updater;
        private readonly PackageReferenceConsoleTableRenderer _renderer;
        private readonly PackageReferenceXElementMapper _mapper;
        public bool IncludeEmptyResults { get; set; }

        public ProjectNugetVersionUpdater()
        {
            _updater = new PackageReferenceVersionUpdate();
            _renderer = new PackageReferenceConsoleTableRenderer();
            _mapper = new PackageReferenceXElementMapper();
        }

        private void SaveUpdate(XDocument doc, string file, IEnumerable<VersionUpdateResult> results)
        {
            var tPath = Path.Combine(Path.GetTempPath(), "nuget_version_update");
            if (!Directory.Exists(tPath))
            {
                Directory.CreateDirectory(tPath);
            }
            var newPath = Path.Combine(tPath, Path.GetFileName(file));
            File.WriteAllText(newPath, doc.ToString());
        }

        private IEnumerable<XElement> QueryProject(XDocument doc, string nameFilter, string versionFilter)
        {
            return new PackageReferenceQuery(doc).QueryPackages(nameFilter, versionFilter);
        }

        public IEnumerable<VersionUpdateResult> UpdateVersionInProject(string projectFile, string nameFilter, string versionFilter, string newVersion, bool suppressPrompt=false)
        {
            var doc = XDocument.Parse(File.ReadAllText(projectFile));
            var items = QueryProject(doc, nameFilter, versionFilter);
            if (!items.Any())
                return null;

            var pr = _mapper.Map(items);

            var versionUpdResults = new List<VersionUpdateResult>();

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

            var simpleExec = new SimpleExec();

            foreach (var i in pr)
            {
                // use dotnet to upgrade package, takes care of dependencies than
                var args = $"add \"{projectFile}\" package \"{i.Name}\" -v \"{newVersion}\"";
                var resulting=simpleExec.Exec("dotnet",args);
                
                versionUpdResults.Add(new VersionUpdateResult(){
                    Name = i.Name,
                    OriginalVersion = i.Version,
                    NewVersion = newVersion,
                    Message = $"ExitCode:{resulting}"
                });
            }

            //updater.SetPackageVersion(items, newVersion, out var updResults, true);
            return versionUpdResults;
        }

        public IDictionary<string, IEnumerable<VersionUpdateResult>> UpdateVersionInProjects(IEnumerable<string> projectFiles, string nameFilter, string versionFilter, string newVersion, bool suppressPrompts=false)
        {
            var d =new Dictionary<string,IEnumerable<VersionUpdateResult>>();
            foreach (var f in projectFiles)
            {
                var results=UpdateVersionInProject(f, nameFilter, versionFilter, newVersion,suppressPrompts);
                d.Add(f,results);
            }
            return d;
        }

        public IDictionary<string, IEnumerable<VersionUpdateResult>> SetVersionForProjectFiles(IEnumerable<string> csProjFiles, string nameFilter, string versionFilter, string newVersion)
        {
            var d = new Dictionary<string, IEnumerable<VersionUpdateResult>>();
            foreach (var f in csProjFiles)
            {
                var doc = XDocument.Parse(File.ReadAllText(f));
                var fndResults = QueryProject(doc, nameFilter, versionFilter);
                var result = _updater.SetPackageVersion(fndResults, newVersion, out var updResults, true);
                d.Add(f, updResults);

                if (updResults.Any())
                {
                    SaveUpdate(doc, f, updResults);
                }
            }
            return d;
        }
    }
}