using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using NugetVersion.GetNugetVersions;
using NugetVersion.Models;
using NugetVersion.PackageReference;
using NugetVersion.Project;
using NugetVersion.Renderer;
using NugetVersion.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NugetVersion;

public class NugetVersionTool
{
    private readonly NugetVersionOptions _nugetVersionOptions;
    private readonly ProjectNugetVersionUpdater _projectNugetVersionUpdater;
    private readonly ProjectFileService _projFileService;
    private readonly IProjectFileResultsRenderer _projFileResultsRenderer;
    private readonly NugetPackageVersionUtil _nugetVersionUtil;
    private readonly ILogger<NugetVersionTool> _logger;

    public NugetVersionTool(NugetVersionOptions nugetVersionOptions,
                            NugetPackageVersionUtil nugetVersionUtil,
                            IPackageReferenceUpdater packageReferenceUpdater,
                            ILogger<NugetVersionTool> logger)
    {
        _logger = logger;
        _nugetVersionOptions = nugetVersionOptions;
        _projectNugetVersionUpdater = new ProjectNugetVersionUpdater(packageReferenceUpdater);
        _projFileService = new ProjectFileService();
        var rendererFactory = new ProjectFileRendererFactory(_nugetVersionOptions);
        _projFileResultsRenderer = rendererFactory.CreateProjectFileRenderer();
        _nugetVersionUtil = nugetVersionUtil;
    }

    public async Task<IDictionary<string, NuGetVersion>> GetLatestNugetPackageVersionsDictionary(IEnumerable<PackageReferenceModel> packages)
    {
        // get latest version(s) for all packages
        var latestPackageVersions = new Dictionary<string, NuGetVersion>();
        foreach (var packageReferenceModel in packages)
        {
            var packageName = packageReferenceModel.Name;
            // fetch latest
            if (!latestPackageVersions.ContainsKey(packageName))
            {
                var latestVersion = await _nugetVersionUtil.GetLatestNugetPackageVersionAsync(packageName);
                if (latestVersion != null)
                {
                    latestPackageVersions[packageName] = latestVersion;
                }
            }
        }
        return latestPackageVersions;
    }


    public async Task ExecuteAsync()
    {
        var basePath = Path.GetFullPath(_nugetVersionOptions.BasePath);
        var projFiles = _projFileService.GetProjectFilesByFilter(basePath, _nugetVersionOptions.SearchFilter);

        if (!projFiles.Any())
        {
            Console.WriteLine($"No project file matches for {basePath}");
            return;
        }

        if (!string.IsNullOrEmpty(_nugetVersionOptions.OutputFile))
        {
            var outputFilePath = Path.GetFullPath(_nugetVersionOptions.OutputFile);

            Console.Write($"Writing output to {outputFilePath}");
            if (!string.IsNullOrEmpty(_nugetVersionOptions.OutputFileFormat))
            {
                Console.Write($" format: {_nugetVersionOptions.OutputFileFormat}");
            }
            Console.WriteLine();

            // no format so just redirect
            ConsoleFileOutput.RedirectConsoleToFile(outputFilePath);
        }

        Console.WriteLine($"Querying {_nugetVersionOptions.BasePath} ...");


        IDictionary<string, NuGetVersion> latestPackageVersions = new Dictionary<string, NuGetVersion>();

        // get latest version(s) for all packages
        if (_nugetVersionOptions.LoadVersionChecks)
        {
            _logger.LogInformation("Fetching latest nuget package versions..");
            latestPackageVersions =
                await GetLatestNugetPackageVersionsDictionary(projFiles.SelectMany(p => p.LastQueriedPackages));
        }

        _projFileResultsRenderer.RenderResults(basePath, _nugetVersionOptions.SearchFilter, projFiles, latestPackageVersions);

        if (!string.IsNullOrEmpty(_nugetVersionOptions.SetNewVersionTo))
        {
            const int startTabPad = 10;
            var strPad = new string(' ', startTabPad);
            if (await _projFileService.SetNugetPackageVersions(_nugetVersionOptions.SearchFilter,
                    _nugetVersionOptions.SetNewVersionTo, projFiles, strPad, _projectNugetVersionUpdater))
            {
                return;
            }
        }
    }
}