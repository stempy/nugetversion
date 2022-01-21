using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NugetVersion.Models;
using NugetVersion.Project;

namespace NugetVersion.Renderer
{
    public class ProjectFileJsonRenderer : IProjectFileResultsRenderer
    {
        public OutputFileFormat Format => OutputFileFormat.Json;

        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true,
            WriteIndented = true
        };

        public void RenderResults(string basePath, SearchQueryFilter filter, IEnumerable<ProjectFile> projFiles)
        {
            var outPutModel = projFiles.Select(x => new 
            {
                ProjectFile = x.Filename.Replace(basePath+@"\",""),
                Version = x.Version,
                OutputType = x.OutputType,
                Sdk = x.ProjectSdk,
                TargetFramework = x.TargetFramework,
                ProjectReferences = x.GetProjectReferences()?.Select(p=>p.Include),
                PackageReferences = x.LastQueriedPackages,
            });

            var total = outPutModel.Count();
            var curr = 0;

            ConsoleRender.W("\n[\n");
            foreach (var projectFile in outPutModel)
            {
                curr++;
                var json = JsonSerializer.Serialize(projectFile, JsonSerializerOptions);
                ConsoleRender.W(json);
                if (curr < total) ConsoleRender.W(",");
                ConsoleRender.W("\n");
            }
            ConsoleRender.W("]\n");
        }
    }
}