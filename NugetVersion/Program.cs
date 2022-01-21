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
            var app = PrepCommandLineApplication();

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
                FileOutput.EndRedirection();
            }
            else
            {
                app.ShowHelp();
            }

            //Console.WriteLine("Completed.");
            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        private static CommandLineApplication PrepCommandLineApplication()
        {
            var app = new CommandLineApplication();
            app.HelpOption();

            var basePathOption = app.Option("-b|--base <PATH>", "Base Path", CommandOptionType.SingleValue);
            var optionName = app.Option("-n|--name <NAME>", "Package Name filter", CommandOptionType.SingleValue);
            var outputFile = app.Option("-o|--output <FILE>", "Send output to file", CommandOptionType.SingleValue);
            var outputFileFormartOption =
                app.Option("-of|--output-format", "File output format [json]", CommandOptionType.SingleValue);

            var optionTargetFrameworkFilter = app.Option("-fw|--framework <TARGETFRAMEWORK>", "Target framework filter",
                CommandOptionType.SingleValue);
            var optionVersionFilter = app.Option("-v|--version <VERSION>", "Version filter", CommandOptionType.SingleValue);
            var optionSetVersion = app.Option("-sv|--set-version <VERSION>", "Update versions of query to new version",
                CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var basePathValue = basePathOption.HasValue() ? basePathOption.Value() : null;
                var fwFilter = optionTargetFrameworkFilter.HasValue() ? optionTargetFrameworkFilter.Value() : null;
                var nameFilter = optionName.HasValue() ? optionName.Value() : null;
                var versionFilter = optionVersionFilter.HasValue() ? optionVersionFilter.Value() : null;
                var setVersion = optionSetVersion.HasValue() ? optionSetVersion.Value() : null;
                var outputFileOption = outputFile.HasValue() ? outputFile.Value() : null;
                var outputFileFormatVal = outputFileFormartOption.HasValue() ? outputFileFormartOption.Value(): null;
                
                var searchQuery = new SearchQueryFilter()
                {
                    TargetFramework = fwFilter,
                    Name = nameFilter,
                    Version = versionFilter
                };


                var remaining = app.RemainingArguments;
                Execute(basePathValue, searchQuery, setVersion, outputFileOption, outputFileFormatVal);
            });
            return app;
        }

        static Version GetVersion()
        {
            return typeof(Program).Assembly.GetName().Version;
        }

        static void Execute(string basePath, SearchQueryFilter filter, string? setVersion, string outputFile,
            string outputFileFormatStr)
        {
            var tools = new ProjectNugetVersionUpdater(new DotNetPackageReferenceUpdater());
            var projFileService = new ProjectFileService();

            OutputFileFormat outputFormat = !string.IsNullOrEmpty(outputFileFormatStr) ?
                Enum.Parse<OutputFileFormat>(outputFileFormatStr, true)
                : OutputFileFormat.Default;

            IProjectFileResultsRenderer projectFileRenderer;

            basePath = Path.GetFullPath(basePath);
            var projFiles = projFileService.GetProjectFilesByFilter(basePath, filter);

            if (!string.IsNullOrEmpty(outputFile))
            {
                var outputFilePath = Path.GetFullPath(outputFile);

                Console.Write($"Writing output to {outputFilePath}");
                if (!string.IsNullOrEmpty(outputFileFormatStr))
                {
                    Console.Write($" format: {outputFileFormatStr}");
                }
                Console.WriteLine();

                // no format so just redirect
                FileOutput.RedirectConsoleToFile(outputFilePath);
            }

            switch (outputFormat)
            {
                case OutputFileFormat.Json:
                    projectFileRenderer = new ProjectFileJsonRenderer();
                    break;
                default:
                    projectFileRenderer = new ProjectFileConsoleRenderer();
                    break;
            }

            projectFileRenderer.RenderResults(basePath, filter, projFiles);

            if (!string.IsNullOrEmpty(setVersion))
            {
                var startTabPad = 10;
                var strPad = new string(' ', startTabPad);
                if (projFileService.SetNugetPackageVersions(filter, setVersion, projFiles, strPad, tools)) return;
            }
        }
    }
}
