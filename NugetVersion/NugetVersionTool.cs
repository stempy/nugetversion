using System;
using System.IO;
using System.Linq;
using NugetVersion.Models;
using NugetVersion.PackageReference;
using NugetVersion.Project;
using NugetVersion.Renderer;
using NugetVersion.Utils;

namespace NugetVersion;

public class NugetVersionTool
{
    private readonly NugetVersionOptions _nugetVersionOptions;
    private readonly ProjectNugetVersionUpdater _projectNugetVersionUpdater;
    private readonly ProjectFileService _projFileService;
    private readonly IProjectFileResultsRenderer _projFileResultsRenderer;

    public NugetVersionTool(NugetVersionOptions nugetVersionOptions)
    {
        _nugetVersionOptions = nugetVersionOptions;
        _projectNugetVersionUpdater = new ProjectNugetVersionUpdater(new DotNetPackageReferenceUpdater());
        _projFileService = new ProjectFileService();
        _projFileResultsRenderer = CreateProjectFileRenderer();
    }

    private IProjectFileResultsRenderer CreateProjectFileRenderer()
    {
        OutputFileFormat outputFormat = !string.IsNullOrEmpty(_nugetVersionOptions.OutputFileFormat)
            ? Enum.Parse<OutputFileFormat>(_nugetVersionOptions.OutputFileFormat, true)
            : OutputFileFormat.Default;

        switch (outputFormat)
        {
            case OutputFileFormat.Json:
                return new ProjectFileJsonRenderer();
            default:
                return new ProjectFileConsoleRenderer(_nugetVersionOptions);
        }
    }

    public void Execute()
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

        _projFileResultsRenderer.RenderResults(basePath, _nugetVersionOptions.SearchFilter, projFiles);

        if (!string.IsNullOrEmpty(_nugetVersionOptions.SetNewVersionTo))
        {
            const int startTabPad = 10;
            var strPad = new string(' ', startTabPad);
            if (_projFileService.SetNugetPackageVersions(_nugetVersionOptions.SearchFilter,
                    _nugetVersionOptions.SetNewVersionTo, projFiles, strPad, _projectNugetVersionUpdater))
            {
                return;
            }
        }
    }
}