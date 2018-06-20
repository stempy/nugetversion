using System;
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

            if (string.IsNullOrEmpty(basePath)){
                throw new Exception("no base path");
            }
            var optShift = args.ToList();
            optShift.RemoveAt(0);

            var optionName = app.Option("-n|--name <NAME>","Package Name filter",CommandOptionType.SingleValue);
            var optionVersionFilter = app.Option("-v|--version <VERSION>","Version filter",CommandOptionType.SingleValue);
            var optionSetVersion = app.Option("-sv|--set-version <VERSION>","Update versions of query to new version",CommandOptionType.SingleValue);
            app.OnExecute(()=>{
                
                var nameFilter = optionName.HasValue()? optionName.Value():null;
                var versionFilter = optionVersionFilter.Value();
                Execute(basePath,nameFilter,versionFilter,optionSetVersion.HasValue()?optionSetVersion.Value():null);
            });

            app.Execute(optShift.ToArray());
            Console.WriteLine("Complete!");
        }

        static void Execute(string basePath, string nameFilter, string versionFilter, string setVersion)
        {
                var tools = new ProjectNugetVersionTools();
                var renderer  = new PackageReferenceConsoleTableRenderer();
                var pkgs=tools.QueryProjectFilesByBasePath(basePath,nameFilter,versionFilter);
                
                var startTabPad =10;
                if (!string.IsNullOrEmpty(setVersion))
                {
                    renderer.RenderProjectResults(startTabPad,pkgs);
                    
                    // update versions
                    tools.UpdateVersionInProjects(pkgs.Select(x=>x.Key),nameFilter,versionFilter,setVersion);
                } else {
                    renderer.RenderProjectResults(startTabPad,pkgs);
                }
        }
    }

}
