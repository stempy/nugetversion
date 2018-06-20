using System;
using System.Diagnostics;
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
            var basePath = args.Length>0? args[0]:null;
            var optShift = args.ToList();
            
            if (args.Any()) optShift.RemoveAt(0);

            var optionName = app.Option("-n|--name <NAME>","Package Name filter",CommandOptionType.SingleValue);
            var optionVersionFilter = app.Option("-v|--version <VERSION>","Version filter",CommandOptionType.SingleValue);
            var optionSetVersion = app.Option("-sv|--set-version <VERSION>","Update versions of query to new version",CommandOptionType.SingleValue);
            app.OnExecute(()=>{
                if (string.IsNullOrEmpty(basePath)){
                    app.ShowHelp();
                    return;
                }

                var nameFilter = optionName.HasValue()? optionName.Value():null;
                var versionFilter = optionVersionFilter.Value();
                Execute(basePath,nameFilter,versionFilter,optionSetVersion.HasValue()?optionSetVersion.Value():null);
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
