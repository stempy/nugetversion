using NugetVersion.Models;
using NugetVersion.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NugetVersion.Renderer
{
    public class ProjectFileConsoleRenderer : IProjectFileResultsRenderer
    {
        private readonly NugetVersionOptions _nugetOptions;

        public ProjectFileConsoleRenderer(NugetVersionOptions nugetVersionOptions)
        {
            _nugetOptions = nugetVersionOptions;
        }

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

            var json = JsonSerializer.Serialize(filter,
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

            ConsoleRender.W($"{basePath}\n");
            ConsoleRender.W($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n");
            ConsoleRender.W($"Filter: {json}\n\n");

            foreach (var projectFile in projFiles)
            {
                var packages = projectFile.LastQueriedPackages;

                if (packages != null)
                {
                    Render(projectFile);
                    Render(packages, StartTabPad, maxNameWidth);
                    if (_nugetOptions.RenderProjectReferences)
                    {
                        var projectRefs = projectFile.GetProjectReferences();
                        if (projectRefs.Any())
                        {
                            Render(projectRefs, StartTabPad, maxNameWidth);
                        }
                    }
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

        private Dictionary<string, string> oldToNewFrameworks = new Dictionary<string, string>()
        {
            ["netstandard2.0"] = "net6.0",
            ["netcoreapp2."] = "net6.0",
            ["netcoreapp3."] = "net6.0",
        };

        public void Render(ProjectFile projectFile)
        {
            ConsoleRender.W($"\r\n{Path.GetFileName(projectFile.Filename)}\r\n", ProjectFileNameColor);

            var targetFrameworkColor = TargetFrameworkColor;

            var targetFramework = projectFile.TargetFramework;
            if (targetFramework != null)
            {
                foreach (var oldToNewFramework in oldToNewFrameworks)
                {
                    if (targetFramework.Contains(oldToNewFramework.Key))
                    {
                        targetFrameworkColor = HighlightWarning;
                        targetFramework += $"(obsolete - recommend {oldToNewFramework.Value}+)";
                    }
                }
            }
            else if (projectFile.TargetFrameworks != null)
            {
                targetFramework = string.Join(';', projectFile.TargetFrameworks);
            }


            ConsoleRender.W($"{targetFramework}", targetFrameworkColor);
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