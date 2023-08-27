using NugetVersion.Models;
using NugetVersion.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NugetVersion.Project
{
    public class ProjectFileService
    {
        public IEnumerable<ProjectFile> GetProjectFilesByFilter(string basePath, SearchQueryFilter filter, string projFilePattern = "*.csproj")
        {
            var projFiles = Directory.GetFiles(basePath, projFilePattern, SearchOption.AllDirectories)
                .Select(x => new ProjectFile(x, filter));

            if (!string.IsNullOrEmpty(filter.TargetFramework))
            {
                projFiles = projFiles
                    .Where(x => !string.IsNullOrEmpty(x.TargetFramework)
                                && x.TargetFramework.Equals(filter.TargetFramework, StringComparison.InvariantCultureIgnoreCase)
                                || x.TargetFrameworks != null
                                && x.TargetFrameworks.Contains(filter.TargetFramework));
            }

            projFiles = projFiles.Where(x => x.QueryPackages().Any()).ToList();
            return projFiles;
        }


        public async Task<bool> SetNugetPackageVersions(SearchQueryFilter filter, string setVersion,
            IEnumerable<ProjectFile> projFiles, string strPad, ProjectNugetVersionUpdater tools)
        {
            var nameFilter = filter.Name;
            var versionFilter = filter.Version;

            var numProjectFiles = projFiles.Count();
            if (numProjectFiles < 1)
            {
                ConsoleRender.W("No file(s) matching spec:")
                    .W($"\n{strPad}Name    : {nameFilter}")
                    .W($"\n{strPad}Version : {versionFilter}\n");
                return true;
            }

            ConsoleRender.W($"Are you sure you want to change versions for {numProjectFiles} project files to ")
                .W($"{setVersion}", ConsoleColor.DarkMagenta).W(" ? Y/N: ");

            var inp = Console.ReadLine();
            if (!inp.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                return true;

            // update versions
            await tools.UpdateVersionInProjects(projFiles, nameFilter, versionFilter, setVersion, true);

            ConsoleRender.W($"Updated {numProjectFiles} projects with packages to version {setVersion}");
            return false;
        }
    }
}