using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using McMaster.Extensions.CommandLineUtils;
using NugetVersion.Models;
using NugetVersion.PackageReference;
using NugetVersion.Project;
using NugetVersion.Renderer;

namespace NugetVersion
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"NugetVersion - v{GetVersion().ToString()}");
            var app = new CommandLineApplication();
            app.HelpOption();

            var basePathOption = app.Option("-b|--base <PATH>", "Base Path", CommandOptionType.SingleValue);
            var optionName = app.Option("-n|--name <NAME>", "Package Name filter", CommandOptionType.SingleValue);

            var optionTargetFrameworkFilter = app.Option("-fw|--framework <TARGETFRAMEWORK>", "Target framework filter",
                CommandOptionType.SingleValue);
            var optionVersionFilter = app.Option("-v|--version <VERSION>", "Version filter", CommandOptionType.SingleValue);
            var optionSetVersion = app.Option("-sv|--set-version <VERSION>", "Update versions of query to new version", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var basePathValue = basePathOption.HasValue() ? basePathOption.Value() : null;
                var fwFilter = optionTargetFrameworkFilter.HasValue() ? optionTargetFrameworkFilter.Value() : null;
                var nameFilter = optionName.HasValue() ? optionName.Value() : null;
                var versionFilter = optionVersionFilter.HasValue() ? optionVersionFilter.Value() : null;
                var setVersion = optionSetVersion.HasValue() ? optionSetVersion.Value() : null;

                var searchQuery = new SearchQueryFilter()
                {
                    TargetFramework = fwFilter,
                    Name = nameFilter,
                    Version = versionFilter
                };


                var remaining = app.RemainingArguments;
                Execute(basePathValue, searchQuery, setVersion);
                Console.WriteLine("Complete!");
            });

            // Some fudge to get command line parameters sorted
            var argList = args.ToList();

            if (argList.Any())
            {
                if (!argList.First().Trim().StartsWith("-")
                    && !argList.Any(x => x.Trim().Contains("-b")
                                         || x.Trim().Contains("--base")))
                {
                    var basePath = argList.First();
                    argList[0] = "-b " + basePath;
                }
                else if (argList.Any(x => x.Contains("-")) && argList.First().Trim().StartsWith("-"))
                {
                    argList.Insert(0, "-b .");
                }

                app.Execute(argList.ToArray());

            }
            else
            {
                
                app.ShowHelp();
            }

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        static Version GetVersion()
        {
            return typeof(Program).Assembly.GetName().Version;
        }


        private static int GetMaxLength(IEnumerable<PackageReferenceModel> packageReferences)
        {
            return packageReferences.Select(x => x.Name.Trim().Length)
                                    .OrderByDescending(x=>x).First();
        }

        static void Execute(string basePath, SearchQueryFilter filter, string setVersion)
        {
            var tools = new ProjectNugetVersionUpdater(new DotNetPackageReferenceUpdater());
            var projFileRenderer = new ProjectFileRenderer();
            var projFileService = new ProjectFileService();

            basePath = Path.GetFullPath(basePath);

            var projFiles = projFileService.GetProjectFilesByFilter(basePath, filter);
            var allProjects = projFiles.ToList();

            // render results
            var startTabPad = 10;
            var strPad = new string(' ', startTabPad);
            var maxNameWidth = GetMaxLength(allProjects.SelectMany(x => x.LastQueriedPackages));

            ConsoleRender.W($"{basePath}\n");
            ConsoleRender.W($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n");
            ConsoleRender.W(
                $"Filter: {JsonSerializer.Serialize(filter, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IgnoreNullValues = true})}\n\n");

            foreach (var projectFile in projFiles)
            {
                var packages = projectFile.LastQueriedPackages;

                if (packages != null)
                {
                    projFileRenderer.Render(projectFile);
                    projFileRenderer.Render(packages, startTabPad, maxNameWidth);
                    projFileRenderer.Render(projectFile.GetProjectReferences(), startTabPad, maxNameWidth);
                }
            }

            // render summary
            RenderSummaryCounts(projFiles,strPad);

            if (!string.IsNullOrEmpty(setVersion))
            {
                if (projFileService.SetNugetPackageVersions(filter.Name, filter.Version, setVersion, projFiles, strPad, tools)) return;
            }
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

    }

}
