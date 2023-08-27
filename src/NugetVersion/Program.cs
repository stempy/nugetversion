using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using NugetVersion.GetNugetVersions;
using NugetVersion.Models;
using NugetVersion.PackageReference;
using NugetVersion.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NugetVersion
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"NugetVersion - v{GetVersion().ToString()}");
            var app = PrepCommandLineApplication();

            var argList = args.ToList();
            if (!argList.Any())
            {
                app.ShowHelp();
                return;
            }

            // Some fudge to get command line parameters sorted
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

            var finalArgs = argList.ToArray();

            await app.ExecuteAsync(finalArgs);
            ConsoleFileOutput.EndRedirection();

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
            var optionVersionFilter =
                app.Option("-v|--version <VERSION>", "Version filter", CommandOptionType.SingleValue);
            var optionSetVersion = app.Option("-sv|--set-version <VERSION>", "Update versions of query to new version. can also use latest to use the latest found version",
                CommandOptionType.SingleValue);

            var suppressProjectReferences = app.Option<bool>("-srefs|--suppressrefs", "Suppress Project References",
                CommandOptionType.NoValue);

            var suppressLatestVersionChecks = app.Option<bool>("-supver|--suppress-version-checks",
                "Suppress remote version checks", CommandOptionType.NoValue);


            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole()
                    .AddFilter("NugetVersion.GetNugetVersions.NugetPackageVersionUtil", LogLevel.Warning));

            app.OnExecuteAsync(async ct =>
            {
                var basePathValue = basePathOption.HasValue() ? basePathOption.Value() : null;
                var fwFilter = optionTargetFrameworkFilter.HasValue() ? optionTargetFrameworkFilter.Value() : null;
                var nameFilter = optionName.HasValue() ? optionName.Value() : null;
                var versionFilter = optionVersionFilter.HasValue() ? optionVersionFilter.Value() : null;
                var setVersion = optionSetVersion.HasValue() ? optionSetVersion.Value() : null;
                var outputFileOption = outputFile.HasValue() ? outputFile.Value() : null;
                var outputFileFormatVal = outputFileFormartOption.HasValue() ? outputFileFormartOption.Value() : null;
                var suppressRefs = suppressProjectReferences.HasValue() ? suppressProjectReferences.ParsedValue : false;
                var suppressVersionChecks = suppressLatestVersionChecks.HasValue() ? true : false;

                if (!Path.IsPathFullyQualified(basePathValue))
                {
                    basePathValue = Path.GetFullPath(basePathValue, Directory.GetCurrentDirectory());
                }


                var searchQuery = new SearchQueryFilter()
                {
                    TargetFramework = fwFilter,
                    Name = nameFilter,
                    Version = versionFilter
                };

                var remaining = app.RemainingArguments;

                var nugetVersionOptions = new NugetVersionOptions()
                {
                    SearchFilter = searchQuery,
                    BasePath = basePathValue,
                    OutputFile = outputFileOption,
                    OutputFileFormat = outputFileFormatVal,
                    RenderProjectReferences = !suppressRefs,
                    LoadVersionChecks = !suppressVersionChecks,
                    SetNewVersionTo = setVersion
                };

                //_projectNugetVersionUpdater = new ProjectNugetVersionUpdater(new DotNetPackageReferenceUpdater(nugetVersionUtil));

                var packageVersionUtil =
                    new NugetPackageVersionUtil(loggerFactory.CreateLogger<NugetPackageVersionUtil>());
                IPackageReferenceUpdater packageReferenceUpdater = new DotNetPackageReferenceUpdater(packageVersionUtil,
                    loggerFactory.CreateLogger<DotNetPackageReferenceUpdater>());

                var nugetTool = new NugetVersionTool(nugetVersionOptions, packageVersionUtil, packageReferenceUpdater, loggerFactory.CreateLogger<NugetVersionTool>());
                await nugetTool.ExecuteAsync();

                if (Debugger.IsAttached)
                {
                    Console.Write("Press [ENTER] to continue");
                    Console.ReadLine();
                }


            });
            return app;
        }

        static Version GetVersion()
        {
            return typeof(Program).Assembly.GetName().Version;
        }
    }
}
