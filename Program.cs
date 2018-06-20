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
            app.OnExecute(()=>{
                
                var nameFilter = optionName.HasValue()? optionName.Value():null;
                var versionFilter = optionVersionFilter.Value();

                var tools = new ProjectNugetVersionTools();
                var pkgs=tools.QueryProjectFilesByBasePath(basePath,nameFilter,versionFilter);
                var renderer = new PackageReferenceConsoleTableRenderer();
                renderer.RenderProjectResults(20,pkgs);
            });


            app.Execute(optShift.ToArray());
            Console.WriteLine("Complete!");

        }
    }

}
