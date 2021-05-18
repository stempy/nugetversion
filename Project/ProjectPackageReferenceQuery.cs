using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NugetVersion.PackageReference;

namespace NugetVersion
{
    public class ProjectPackageReferenceQuery
    {
        private readonly PackageReferenceXElementMapper _mapper;
        public bool IncludeEmptyResults { get; set; }

        public ProjectPackageReferenceQuery()
        {
            _mapper = new PackageReferenceXElementMapper();
        }

        public PackageReferenceDic QueryProjectFilesByBasePath(string basePath, string nameFilter, string versionFilter)
        {
            Console.WriteLine($"Querying Projects in {basePath}");
            var files = Directory.GetFiles(basePath, "*.csproj", SearchOption.AllDirectories);

            var d = new PackageReferenceDic();
            foreach (var f in files)
            {
                var results = QueryProjectFile(f, nameFilter, versionFilter);
                if (results == null || !results.Any())
                {
                    if (!IncludeEmptyResults)
                        continue;
                }
                d.Add(f, results);
            }
            return d;
        }

        private IEnumerable<PackageReferenceModel> QueryProjectFile(string file, string nameFilter, string versionFilter)
        {
            var projInfo = new ProjectFile(file);
            
            
            var doc = XDocument.Parse(File.ReadAllText(file));
            var results = new PackageReferenceQuery(doc).QueryPackages(nameFilter, versionFilter);
            return _mapper.Map(results);
        }
    }
}