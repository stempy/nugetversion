using System.IO;
using System.Text.RegularExpressions;

namespace nugetversion
{
    // parse filenames for nuget data
    public class PackageGitVersionParser
    {
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
