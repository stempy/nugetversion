using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var optionVersionFilter = app.Option("-v|--version <VERSION>", "Version filter", CommandOptionType.SingleValue);
            var optionSetVersion = app.Option("-sv|--set-version <VERSION>", "Update versions of query to new version", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var basePathValue = basePathOption.HasValue() ? basePathOption.Value() : null;
                var nameFilter = optionName.HasValue() ? optionName.Value() : null;
                var versionFilter = optionVersionFilter.HasValue() ? optionVersionFilter.Value() : null;
                var setVersion = optionSetVersion.HasValue() ? optionSetVersion.Value() : null;
                var remaining = app.RemainingArguments;
                Execute(basePathValue, nameFilter, versionFilter, setVersion);
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


        static IEnumerable<ProjectFile> GetProjectFiles(string basePath, string nameFilter, string versionFilter)
        {
            return Directory.GetFiles(basePath, "*.csproj", SearchOption.AllDirectories)
                .Select(x => new ProjectFile(x, nameFilter, versionFilter));
        }

        private static int GetMaxLength(IEnumerable<PackageReferenceModel> packageReferences)
        {
            return packageReferences.Select(x => x.Name.Trim().Length)
                                    .OrderByDescending(x=>x).First();
        }

        static void Execute(string basePath, string nameFilter, string versionFilter, string setVersion)
        {
            var tools = new ProjectNugetVersionUpdater(new DotNetPackageReferenceUpdater());

            basePath = Path.GetFullPath(basePath);

            var startTabPad = 10;
            var strPad = new string(' ', startTabPad);
            var projFileRenderer = new ProjectFileRenderer();

            var projFiles = GetProjectFiles(basePath, nameFilter, versionFilter)
                                                            .Where(x => x.QueryPackages().Any());

            var allProjects = projFiles.ToList();

            var maxNameWidth = GetMaxLength(allProjects.SelectMany(x => x.LastQueriedPackages));

            foreach (var projectFile in projFiles)
            {
                var packages = projectFile.LastQueriedPackages;

                if (packages != null)
                {
                    projFileRenderer.Render(projectFile);
                    projFileRenderer.Render(packages, startTabPad, maxNameWidth);
                }
            }


            if (!string.IsNullOrEmpty(setVersion))
            {
                if (SetNugetPackageVersions(nameFilter, versionFilter, setVersion, projFiles, strPad, tools)) return;
            }
        }

        private static bool SetNugetPackageVersions(string nameFilter, string versionFilter, string setVersion,
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
