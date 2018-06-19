using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace nugetversion
{
    // parse filenames for nuget data
    public class PackageGitVersionParser
    {
        public class ParsedNugetResult
        {
            public string Name { get; set; }
            public string NugetVersion { get; set; }
            public string FullPath { get; set; }

            public bool IsPrerelease
            {
                get
                {
                    return NugetVersion != null && NugetVersion.Contains("-");
                }
            }

            public string GitFlowBranch()
            {
                if (!IsPrerelease)
                    return "master";

                if (NugetVersion.Contains("unstable"))
                    return "develop";

                return null;
            }
        }

        public ParsedNugetResult ParseFilename(string file)
        {
            var fileOnly = Path.GetFileNameWithoutExtension(file);
            var isPrerelease = fileOnly.Contains("-");
            var result = new ParsedNugetResult() { FullPath = file };

            var hyphenIdx = fileOnly.IndexOf('-');
            var matchVer = Regex.Match(fileOnly, @"([a-zA-Z\.]*)([0-9\.-]*.*)");

            var name = matchVer.Groups[1].Value.Trim('.');
            var versionNum = matchVer.Groups[2].Value.Trim('.');

            result.Name = name;
            result.NugetVersion = versionNum;
            return result;
        }
    }

}
