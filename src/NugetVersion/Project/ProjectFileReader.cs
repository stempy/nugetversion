using NugetVersion.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NugetVersion.Project;

internal class ProjectFilePackageHelpers
{
}


internal static class ProjectFileReader
{
    internal static ProjectFileDto ReadProjectInfo(string projectFile)
    {
        try
        {
            var doc = GetXDoc(projectFile);
            string projectSdk = null;

            var projElk = doc.Elements("Project").FirstOrDefault();
            if (projElk != null)
            {
                projectSdk = projElk.GetXElementAttributeValueOrNull("Sdk");
            }

            var propGroup = doc.Descendants("PropertyGroup").Elements();
            var targetFramework = propGroup.FindElementKeyValueOrNull("TargetFramework");
            string targetFrameworks = null;
            if (targetFramework == null)
            {
                targetFrameworks = propGroup.FindElementKeyValueOrNull("TargetFrameworks");
            }


            var assemblyVersion = propGroup.FindElementKeyValueOrNull("AssemblyVersion");
            var fileVersion = propGroup.FindElementKeyValueOrNull("FileVersion");
            var description = propGroup.FindElementKeyValueOrNull("Description");
            var version = propGroup.FindElementKeyValueOrNull("Version");
            var isPackable = propGroup.FindElementKeyValueOrNull("IsPackable");
            var packAsTool = propGroup.FindElementKeyValueOrNull("PackAsTool");
            var outputType = propGroup.FindElementKeyValueOrNull("OutputType");

            return new ProjectFileDto()
            {
                ProjectSdk = projectSdk,
                TargetFramework = targetFramework,
                TargetFrameworks = targetFrameworks?.Split(';'),
                AssemblyVersion = assemblyVersion,
                Description = description,
                FileVersion = fileVersion,
                Filename = projectFile,
                IsPackable = isPackable,
                PackAsTool = packAsTool,
                OutputType = outputType,
                Version = version
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to parse {projectFile}", ex);
        }
    }

    private static XDocument GetXDoc(string filename)
    {
        if (!File.Exists(filename))
            throw new FileNotFoundException(filename);

        return XDocument.Parse(File.ReadAllText(filename));
    }
}