using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NugetVersion.Models;
using NugetVersion.Project;
using Console = System.Console;

namespace NugetVersion.Renderer
{
    public class ProjectFileConsoleRenderer : IProjectFileResultsRenderer
    {
        public OutputFileFormat Format => OutputFileFormat.Default;
        
        
        public const ConsoleColor ProjectFileNameColor = ConsoleColor.Gray;
        public const ConsoleColor ProjectInfoColor = ConsoleColor.DarkGray;

        public const ConsoleColor TargetFrameworkColor = ConsoleColor.DarkGray;

        public const ConsoleColor ProjectPackageNameColor = ConsoleColor.DarkCyan;
        public const ConsoleColor ProjectPackageVersionColor = ConsoleColor.DarkMagenta;

        public const ConsoleColor HighlightWarning = ConsoleColor.Yellow;
        public const ConsoleColor HighlightError = ConsoleColor.Red;

        public int StartTabPad { get; set; } = 5;


        public void RenderResults(string basePath, SearchQueryFilter filter, 
            IEnumerable<ProjectFile> projFiles)
        {
            var strPad = new string(' ', StartTabPad);
            var maxNameWidth = GetMaxLength(projFiles.SelectMany(x => x.LastQueriedPackages));

            ConsoleRender.W($"{basePath}\n");
            ConsoleRender.W($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n");
            ConsoleRender.W(
                $"Filter: {JsonSerializer.Serialize(filter, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IgnoreNullValues = true })}\n\n");

            foreach (var projectFile in projFiles)
            {
                var packages = projectFile.LastQueriedPackages;

                if (packages != null)
                {
                    Render(projectFile);
                    Render(packages, StartTabPad, maxNameWidth);
                    Render(projectFile.GetProjectReferences(), StartTabPad, maxNameWidth);
                }
            }

            // render summary
            // RenderSummaryCounts(projFiles, strPad);
        }

        private static int GetMaxLength(IEnumerable<PackageReferenceModel> packageReferences)
        {
            return packageReferences.Select(x => x.Name.Trim().Length)
                .OrderByDescending(x => x).First();
        }

        private static void RenderSummaryCounts(IEnumerable<ProjectFile> projFiles, string strPad)
        {
            var dic = new Dictionary<string, int>();
            foreach (var projectFile in projFiles)
            {
                foreach (var package in projectFile.LastQueriedPackages)
                {
                    if (!dic.ContainsKey(package.Name))
                    {
                        dic[package.Name] = 1;
                    }
                    else
                    {
                        dic[package.Name] = dic[package.Name] + 1;
                    }
                }
            }

            dic = dic.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);

            ConsoleRender.W($"Package Summary Count:\n");
            foreach (var pkgCount in dic)
            {
                ConsoleRender.W($"{strPad}[{pkgCount.Value.ToString().PadLeft(3)}] {pkgCount.Key}\n");
            }
        }

        public void Render(ProjectFile projectFile)
        {
            ConsoleRender.W($"\r\n{Path.GetFileName(projectFile.Filename)}\r\n",ProjectFileNameColor);

            var targetFrameworkColor = TargetFrameworkColor;

            var targetFramework = projectFile.TargetFramework;
            if (targetFramework == "netstandard2.0")
            {
                targetFrameworkColor = HighlightWarning;
                targetFramework += " (recommend 2.1)";
            } else if (targetFramework == "netcoreapp3.0")
            {
                targetFrameworkColor = HighlightWarning;
                targetFramework += " (3.0 is obsolete, use 3.1 or .NET 5.x+)";
            }
            
            ConsoleRender.W($"{targetFramework}",targetFrameworkColor);
            if (!string.IsNullOrEmpty(projectFile.OutputType))
            {
                ConsoleRender.W($" |  OutputType: {projectFile.OutputType}");
            }

            if (!string.IsNullOrEmpty(projectFile.ProjectSdk))
            {
                ConsoleRender.W($" |  Sdk: {projectFile.ProjectSdk} \r\n", ProjectInfoColor);
            }
            
        }

        public void Render(IEnumerable<ProjectReferenceModel> items, int startTabPad, int maxNameWidth)
        {
            var tabIdx = startTabPad;
            var tabStr = new string(' ', tabIdx);
            var padRightMax = maxNameWidth + 10;

            ConsoleRender.W($"{tabStr}ProjectReferences:\n");
            foreach (var pr in items)
            {
                ConsoleRender.W($"{tabStr}{tabStr}{pr.Include.PadRight(padRightMax)}\n", ProjectPackageNameColor);
            }
        }

        public void Render(IEnumerable<PackageReferenceModel> items, int startTabPad, int maxNameWidth)
        {
            var tabIdx = startTabPad;
            var tabStr = new string(' ', tabIdx);
            var padRightMax = maxNameWidth + 10;

            ConsoleRender.W($"{tabStr}Nuget:\n");
            foreach (var pr in items)
            {
                ConsoleRender.W($"{tabStr}{tabStr}{pr.Name.PadRight(padRightMax)}", ProjectPackageNameColor)
                    .W($"{pr.Version}\n", ProjectPackageVersionColor);
            }
        }
    }
}