using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace nugetversion
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption();

            var basePathOption = app.Option("-b|--base <PATH>", "Base Path", CommandOptionType.SingleValue);
            var optionName = app.Option("-n|--name <NAME>","Package Name filter",CommandOptionType.SingleValue);
            var optionVersionFilter = app.Option("-v|--version <VERSION>","Version filter",CommandOptionType.SingleValue);
            var optionSetVersion = app.Option("-sv|--set-version <VERSION>","Update versions of query to new version",CommandOptionType.SingleValue);

            var basePath = ".";
            var optShift = args.ToList();
            //app.ThrowOnUnexpectedArgument = false;

            app.OnExecute(()=>{
                var basePathValue = basePathOption.HasValue() ? basePathOption.Value() : null;
                var nameFilter = optionName.HasValue()? optionName.Value():null;
                var versionFilter = optionVersionFilter.HasValue()? optionVersionFilter.Value():null;
                var setVersion = optionSetVersion.HasValue() ? optionSetVersion.Value() : null;

                var remaining = app.RemainingArguments;

                if (!string.IsNullOrEmpty(basePathValue))
                    basePath = basePathValue;

                Execute(basePath,nameFilter,versionFilter,setVersion);
            });

            app.Execute(optShift.ToArray());

            Console.WriteLine("Complete!");

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
        }

        static void Execute(string basePath, string nameFilter, string versionFilter, string setVersion)
        {
                var tools = new ProjectNugetVersionTools();
                var renderer  = new PackageReferenceConsoleTableRenderer();
                basePath = Path.GetFullPath(basePath);
                var pkgs=tools.QueryProjectFilesByBasePath(basePath,nameFilter,versionFilter);
                
                var startTabPad =10;
                var strPad = new string(' ',startTabPad);
                if (!string.IsNullOrEmpty(setVersion))
                {
                    renderer.RenderProjectResults(startTabPad,pkgs);
                    bool suppressPrompts = false;

                    var numProjectFiles = pkgs.Count();
                    if (numProjectFiles < 1)
                    {
                        ConsoleRender.W("No file(s) matching spec:")
                            .W($"\n{strPad}Name    : {nameFilter}")
                            .W($"\n{strPad}Version : {versionFilter}\n");
                        return;
                    }

                    ConsoleRender.W($"Are you sure you want to change versions for {numProjectFiles} project files to ")
                        .W($"{setVersion}", ConsoleColor.DarkMagenta).W(" ? Y/N: ");

                    var inp = Console.ReadLine();
                    if (!inp.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                        return;

                    // update versions
                    tools.UpdateVersionInProjects(pkgs.Select(x=>x.Key),nameFilter,versionFilter,setVersion,true);

                    ConsoleRender.W($"Updated {numProjectFiles} projects with packages to version {setVersion}");
                } else {
                    renderer.RenderProjectResults(startTabPad,pkgs);
                }
        }
    }

}
