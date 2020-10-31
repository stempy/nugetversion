using System.Collections.Generic;

namespace nugetversion.PackageReference
{
    public class PackageReferenceDic 
		: Dictionary<string,IEnumerable<PackageReference>>,IDictionary<string,IEnumerable<PackageReference>>{}

}
