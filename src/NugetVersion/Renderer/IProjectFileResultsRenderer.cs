using NuGet.Versioning;
using NugetVersion.Models;
using NugetVersion.Project;
using System.Collections.Generic;

namespace NugetVersion.Renderer
{
    public interface IProjectFileResultsRenderer
    {
        OutputFileFormat Format { get; }
        void RenderResults(string basePath, SearchQueryFilter filter, IEnumerable<ProjectFile> projFiles,
            IDictionary<string, NuGetVersion> latestPackageVersions);
    }
}