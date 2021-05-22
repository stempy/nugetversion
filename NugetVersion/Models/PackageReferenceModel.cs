using System.Collections.Generic;

namespace NugetVersion.Models
{
    public class PackageReferenceModel
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string PrivateAssets { get; set; }
    }

    public class PackageReferenceItemQuery
    {
        public string Name { get; set; }
        public string Version { get; set; }
        
        public IEnumerable<PackageReferenceModel> PackageReferenceResults { get; set; }
    }

}
