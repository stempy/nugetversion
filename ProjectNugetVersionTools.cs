using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace nugetversion
{


    // query across projects
    public class ProjectNugetVersionTools
    {
        private PackageReferenceQuery query;
        private PackageReferenceVersionUpdate updater;
        private PackageReferenceConsoleTableRenderer renderer;
        public bool IncludeEmptyResults { get; set; }

        public ProjectNugetVersionTools()
        {
            query = new PackageReferenceQuery();
            updater = new PackageReferenceVersionUpdate();
            renderer = new PackageReferenceConsoleTableRenderer();
        }

        private IEnumerable<PackageReference> Map(IEnumerable<XElement> items)
        {
            return items.Select(x => new PackageReference()
            {
                Name = x.GetXElementAttributeValueOrNull(PackageConstants.PackageNameAttr),
                Version = x.GetXElementAttributeValueOrNull(PackageConstants.PackageVersionAttr) ?? x.Element("Version")?.Value
            });
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

        public IEnumerable<VersionUpdateResult> UpdateVersionInProject(string projectFile, string nameFilter, string versionFilter, string newVersion)
        {
            var doc = XDocument.Parse(File.ReadAllText(projectFile));
            var items = QueryProject(doc, nameFilter, versionFilter);
            if (!items.Any())
                return null;

            var pr = Map(items);

            var versionUpdResults = new List<VersionUpdateResult>();

            // ok lets provide confirmation
            renderer.RenderProjectResults(0, renderer.GetMaxNumPad(pr), projectFile, pr);

            ConsoleRender.W($"Are you sure you want to change versions for {pr.Count()} packages to ")
                         .W($"{newVersion}", ConsoleColor.DarkMagenta).W(" ? Y/N: ");

            var inp = Console.ReadLine();
            if (!inp.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                return null;

            var simpleExec = new SimpleExec();

            foreach (var i in pr)
            {
                // use dotnet to upgrade package, takes care of dependencies than
                var args = $"add \"{projectFile}\" package \"{i.Name}\" -v \"{newVersion}\"";
                var result=simpleExec.Exec("dotnet",args, out var exitCode);
                versionUpdResults.Add(new VersionUpdateResult(){
                    Name = i.Name,
                    OriginalVersion = i.Version,
                    NewVersion = newVersion,
                    Message = $"ExitCode:{exitCode} - "+ result
                });
            }

            //updater.SetPackageVersion(items, newVersion, out var updResults, true);
            return versionUpdResults;
        }

        public IDictionary<string, IEnumerable<VersionUpdateResult>> UpdateVersionInProjects(IEnumerable<string> projectFiles, string nameFilter, string versionFilter, string newVersion)
        {
            var d =new Dictionary<string,IEnumerable<VersionUpdateResult>>();
            foreach (var f in projectFiles)
            {
                var results=UpdateVersionInProject(f, nameFilter, versionFilter, newVersion);
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
                //Console.Read();
                var result = updater.SetPackageVersion(fndResults, newVersion, out var updResults, true);
                d.Add(f, updResults);

                if (updResults.Any())
                {
                    // update actual file
                    // TODO: UPDATE FILE ITSELF
                    SaveUpdate(doc, f, updResults);
                }
            }
            return d;
        }

        public IEnumerable<XElement> QueryProject(XDocument doc, string nameFilter, string versionFilter)
        {
            return query.QueryP(doc, nameFilter, versionFilter);
        }

        public IEnumerable<PackageReference> QueryProjectFile(string file, string nameFilter, string versionFilter)
        {
            var doc = XDocument.Parse(File.ReadAllText(file));
            return Map(QueryProject(doc, nameFilter, versionFilter));
        }

        public PackageReferenceDic QueryProjectFiles(IEnumerable<string> csProjFiles, string nameFilter, string versionFilter)
        {
            var d = new PackageReferenceDic();
            foreach (var f in csProjFiles)
            {
                var results = QueryProjectFile(f, nameFilter, versionFilter);
                if (results == null || !results.Any())
                {
                    if (!IncludeEmptyResults)
                        continue;
                }
                d.Add(f, results);
            }
            return d;
        }

        public PackageReferenceDic QueryProjectFilesByBasePath(string basePath, string nameFilter, string versionFilter)
        {
            var files = Directory.GetFiles(basePath, "*.csproj", SearchOption.AllDirectories);
            return QueryProjectFiles(files, nameFilter, versionFilter);
        }
    }
}