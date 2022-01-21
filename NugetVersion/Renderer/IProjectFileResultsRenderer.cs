using System.Collections.Generic;
using NugetVersion.Models;
using NugetVersion.Project;

namespace NugetVersion.Renderer
{
    public interface IProjectFileResultsRenderer
    {
        OutputFileFormat Format { get; }
        void RenderResults(string basePath, SearchQueryFilter filter, IEnumerable<ProjectFile> projFiles);
    }
}