using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NugetVersion.Extensions;
using NugetVersion.Models;
using NugetVersion.PackageReference;

namespace NugetVersion.Project
{
    public class ProjectFile : ProjectFileDto
    {
        private readonly string _filename;
        private readonly string _nameFilter;
        private readonly string _versionFilter;

        private readonly PackageReferenceXmlMapper _mapper;


        public string Filename => _filename;
        public string NameFilter => _nameFilter;
        public string VersionFilter => _versionFilter;

        public IEnumerable<PackageReferenceModel> LastQueriedPackages { get; private set; }


        public ProjectFile(string filename, SearchQueryFilter filter)
        {
            _filename = filename;
            _mapper = new PackageReferenceXmlMapper();
            _nameFilter = filter.Name;
            _versionFilter = filter.Version;

            ReadProjectInfo();
        }

        private XDocument GetXDoc()
        {
            if (!File.Exists(_filename))
                throw new FileNotFoundException(_filename);
            
            return XDocument.Parse(File.ReadAllText(_filename));
        }

        private void ReadProjectInfo()
        {
            try
            {
                var doc = GetXDoc();

                var projElk = doc.Elements("Project").FirstOrDefault();
                if (projElk != null)
                {
                    ProjectSdk = projElk.GetXElementAttributeValueOrNull("Sdk");
                }

                var propGroup = doc.Descendants("PropertyGroup").Elements();
                TargetFramework = propGroup.FindElementKeyValueOrNull("TargetFramework");
                AssemblyVersion = propGroup.FindElementKeyValueOrNull("AssemblyVersion");
                FileVersion = propGroup.FindElementKeyValueOrNull("FileVersion");
                Description = propGroup.FindElementKeyValueOrNull("Description");
                Version = propGroup.FindElementKeyValueOrNull("Version");
                IsPackable = propGroup.FindElementKeyValueOrNull("IsPackable");
                PackAsTool = propGroup.FindElementKeyValueOrNull("PackAsTool");
                OutputType = propGroup.FindElementKeyValueOrNull("OutputType");

            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse {_filename}",ex);
            }
        }

        public IEnumerable<PackageReferenceModel> QueryPackages()
        {
            LastQueriedPackages ??= 
                        _mapper.Map(new PackageReferenceXmlReader(GetXDoc()).GetPackageReferenceElements(_nameFilter, _versionFilter));

            PackageReferences = LastQueriedPackages;
            return LastQueriedPackages;
        }

        public IEnumerable<ProjectReferenceModel> GetProjectReferences()
        {
            return ProjectReferences ??= new PackageReferenceReader().GetProjectReferences(GetXDoc());
        }
    }
}