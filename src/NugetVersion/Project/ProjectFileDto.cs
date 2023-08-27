using NugetVersion.Models;
using System.Collections.Generic;

namespace NugetVersion.Project
{
    public class ProjectFileDto
    {
        public string Filename { get; set; }
        public IEnumerable<ProjectReferenceModel> ProjectReferences { get; set; }
        public IEnumerable<string> ReferencedByProjects { get; set; }
        public IEnumerable<PackageReferenceModel> PackageReferences { get; set; }

        public string TargetFramework { get; set; }
        public IEnumerable<string> TargetFrameworks { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string ProjectSdk { get; set; }
        public string OutputType { get; set; }
        public string IsPackable { get; set; }
        public string PackAsTool { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
    }
}