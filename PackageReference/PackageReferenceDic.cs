using System.Collections.Generic;

namespace nugetversion
{
    public class PackageReferenceDic 
		: Dictionary<string,IEnumerable<PackageReference>>,IDictionary<string,IEnumerable<PackageReference>>{}

}
