using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using NugetVersion.Extensions;

namespace NugetVersion.PackageReference
{
    internal class ProjectFile
    {
        private XDocument _xdoc;
        private readonly string _filename;
        private readonly PackageReferenceXElementMapper _mapper;

        public string OutputType { get; set; }
        public string IsPackable { get; set; }
        public string PackAsTool { get; set; }
        public string TargetFramework { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public ProjectFile(string filename)
        {
            _filename = filename;
            ReadProjectInfo();
            _mapper = new PackageReferenceXElementMapper();
        }

        public ProjectFile(XDocument xdoc)
        {
            _xdoc = xdoc;
            ReadProjectInfo();
            _mapper = new PackageReferenceXElementMapper();
        }

        private XDocument GetXDoc()
        {
            if (_xdoc == null)
            {
                if (!string.IsNullOrEmpty(_filename))
                {
                    _xdoc = XDocument.Parse(File.ReadAllText(_filename));
                }
            }

            return _xdoc;
        }


        private void ReadProjectInfo()
        {
            var propGroup = GetXDoc().Descendants("PropertyGroup").Elements();
            TargetFramework = propGroup.FindElementKeyValueOrNull("TargetFramework");
            AssemblyVersion = propGroup.FindElementKeyValueOrNull("AssemblyVersion");
            FileVersion = propGroup.FindElementKeyValueOrNull("FileVersion");
        }


        public IEnumerable<PackageReferenceModel> QueryPackages(string nameFilter, string versionFilter)
        {
            var pkgRef = new PackageReferenceQuery(GetXDoc()).QueryPackages(nameFilter, versionFilter);
            return _mapper.Map(pkgRef);
        }
    }
}