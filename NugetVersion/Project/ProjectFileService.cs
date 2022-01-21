using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NugetVersion.Models;
using NugetVersion.Renderer;

namespace NugetVersion.Project
{
    public class ProjectFileService
    {
        public IEnumerable<ProjectFile> GetProjectFilesByFilter(string basePath, SearchQueryFilter filter)
        {
            var projFiles = Directory.GetFiles(basePath, "*.csproj", SearchOption.AllDirectories)
                .Select(x => new ProjectFile(x, filter));

            if (!string.IsNullOrEmpty(filter.TargetFramework))
            {
                projFiles = projFiles.Where(x => x.TargetFramework == filter.TargetFramework);
            }

            projFiles = projFiles.Where(x => x.QueryPackages().Any()).ToList();

            //DetermineProjectReferences(projFiles);

            return projFiles;
        }

        private void DetermineProjectReferences(IEnumerable<ProjectFile> projFiles)
        {
            throw new NotImplementedException();
        }

        public bool SetNugetPackageVersions(string nameFilter, string versionFilter, string setVersion,
            IEnumerable<ProjectFile> projFiles, string strPad, ProjectNugetVersionUpdater tools)
        {
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
            tools.UpdateVersionInProjects(projFiles, nameFilter, versionFilter, setVersion, true);

            ConsoleRender.W($"Updated {numProjectFiles} projects with packages to version {setVersion}");
            return false;
        }
    }
}