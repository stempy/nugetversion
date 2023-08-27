using McMaster.Extensions.CommandLineUtils;
using NugetVersion.Models;
using NugetVersion.Utils;
using System;
using System.Diagnostics;
using System.Linq;

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
                ConsoleFileOutput.EndRedirection();
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
            var optionVersionFilter =
                app.Option("-v|--version <VERSION>", "Version filter", CommandOptionType.SingleValue);
            var optionSetVersion = app.Option("-sv|--set-version <VERSION>", "Update versions of query to new version",
                CommandOptionType.SingleValue);

            var suppressProjectReferences = app.Option<bool>("-srefs|--suppressrefs", "Suppress Project References",
                CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                var basePathValue = basePathOption.HasValue() ? basePathOption.Value() : null;
                var fwFilter = optionTargetFrameworkFilter.HasValue() ? optionTargetFrameworkFilter.Value() : null;
                var nameFilter = optionName.HasValue() ? optionName.Value() : null;
                var versionFilter = optionVersionFilter.HasValue() ? optionVersionFilter.Value() : null;
                var setVersion = optionSetVersion.HasValue() ? optionSetVersion.Value() : null;
                var outputFileOption = outputFile.HasValue() ? outputFile.Value() : null;
                var outputFileFormatVal = outputFileFormartOption.HasValue() ? outputFileFormartOption.Value() : null;
                var suppressRefs = suppressProjectReferences.HasValue() ? suppressProjectReferences.ParsedValue : false;

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
                    SetNewVersionTo = setVersion
                };

                var nugetTool = new NugetVersionTool(nugetVersionOptions);
                nugetTool.Execute();

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
