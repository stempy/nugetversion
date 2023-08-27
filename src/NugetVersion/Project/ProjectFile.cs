using NugetVersion.Models;
using NugetVersion.PackageReference;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace NugetVersion.Project
{
    public class ProjectFile : ProjectFileDto
    {
        private readonly string _filename;
        private readonly string _packageNameFilter;
        private readonly string _packageVersionFilter;

        private readonly PackageReferenceXmlMapper _mapper;


        public new string Filename => _filename;
        public string NameFilter => _packageNameFilter;
        public string VersionFilter => _packageVersionFilter;

        public IEnumerable<PackageReferenceModel> LastQueriedPackages { get; private set; }


        public ProjectFile(string filename, SearchQueryFilter filter)
        {
            _filename = filename;
            _mapper = new PackageReferenceXmlMapper();
            _packageNameFilter = filter.Name;
            _packageVersionFilter = filter.Version;

            LoadProjectFileData();
        }

        private void LoadProjectFileData()
        {
            var projInfo = ProjectFileReader.ReadProjectInfo(_filename);
            AssemblyVersion = projInfo.AssemblyVersion;
            TargetFramework = projInfo.TargetFramework;
            TargetFrameworks = projInfo.TargetFrameworks;
            Description = projInfo.Description;
            FileVersion = projInfo.FileVersion;
            IsPackable = projInfo.IsPackable;
            PackAsTool = projInfo.PackAsTool;
            OutputType = projInfo.OutputType;
            Version = projInfo.Version;
        }

        private XDocument GetXDoc()
        {
            if (!File.Exists(_filename))
                throw new FileNotFoundException(_filename);

            return XDocument.Parse(File.ReadAllText(_filename));
        }

        public IEnumerable<PackageReferenceModel> QueryPackages()
        {
            LastQueriedPackages ??=
                        _mapper.Map(new PackageReferenceXmlReader(GetXDoc()).GetPackageReferenceElements(_packageNameFilter, _packageVersionFilter));

            PackageReferences = LastQueriedPackages;
            return LastQueriedPackages;
        }

        public IEnumerable<ProjectReferenceModel> GetProjectReferences()
        {
            return ProjectReferences ??= new PackageReferenceReader().GetProjectReferences(GetXDoc());
        }
    }
}