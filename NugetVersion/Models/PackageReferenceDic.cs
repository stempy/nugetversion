using System.Collections.Generic;

namespace NugetVersion.Models
{
    public class PackageReferenceDic 
		: Dictionary<string,IEnumerable<PackageReferenceModel>>,IDictionary<string,IEnumerable<PackageReferenceModel>>{}

}
