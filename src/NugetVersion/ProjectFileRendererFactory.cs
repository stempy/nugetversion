using System;
using NugetVersion.Models;
using NugetVersion.Renderer;

namespace NugetVersion;

public class ProjectFileRendererFactory
{
    private readonly NugetVersionOptions _options;

    public ProjectFileRendererFactory(NugetVersionOptions options)
    {
        _options = options;
    }

    public IProjectFileResultsRenderer CreateProjectFileRenderer()
    {
        OutputFileFormat outputFormat = !string.IsNullOrEmpty(_options.OutputFileFormat)
            ? Enum.Parse<OutputFileFormat>(_options.OutputFileFormat, true)
            : OutputFileFormat.Default;

        switch (outputFormat)
        {
            case OutputFileFormat.Json:
                return new ProjectFileJsonRenderer();
            default:
                return new ProjectFileConsoleRenderer(_options);
        }
    }
}