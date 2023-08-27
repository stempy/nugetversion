using NugetVersion.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NugetVersion.Project;

public enum NetProjectType
{
    Unknown,
    ClassLibrary,
    Console,
    Web,
    UnitTest,
    AzureFunction
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

            var azureFunctionsVersion = propGroup.FindElementKeyValueOrNull("AzureFunctionsVersion");
            var assemblyVersion = propGroup.FindElementKeyValueOrNull("AssemblyVersion");
            var fileVersion = propGroup.FindElementKeyValueOrNull("FileVersion");
            var description = propGroup.FindElementKeyValueOrNull("Description");
            var version = propGroup.FindElementKeyValueOrNull("Version");
            var isPackable = propGroup.FindElementKeyValueOrNull("IsPackable");
            var packAsTool = propGroup.FindElementKeyValueOrNull("PackAsTool");
            var outputType = propGroup.FindElementKeyValueOrNull("OutputType");
            var dto = new ProjectFileDto()
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
                Version = version,
                AzureFunctionsVersion = azureFunctionsVersion
            };

            dto.DeterminedProjectType = DetermineProjectType(dto);

            return dto;
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to parse {projectFile}", ex);
        }
    }

    static NetProjectType DetermineProjectType(ProjectFileDto dto)
    {
        if (IsAzureFunction(dto))
        {
            return NetProjectType.AzureFunction;
        }
        else if (IsWebApp(dto))
        {
            return NetProjectType.Web;
        }
        else if (IsUnitTest(dto))
        {
            return NetProjectType.UnitTest;
        }
        else if (IsConsole(dto))
        {
            return NetProjectType.Console;
        }

        return NetProjectType.Unknown;
    }

    private static XDocument GetXDoc(string filename)
    {
        if (!File.Exists(filename))
            throw new FileNotFoundException(filename);

        return XDocument.Parse(File.ReadAllText(filename));
    }


    static bool IsAzureFunction(ProjectFileDto proj)
        => proj.AzureFunctionsVersion != null;

    static bool IsConsole(ProjectFileDto proj)
        => proj.OutputType == "Exe" && proj.AzureFunctionsVersion == null;

    static bool IsWebApp(ProjectFileDto proj)
        => proj.ProjectSdk == "Microsoft.NET.Sdk.Web";

    static bool IsUnitTest(ProjectFileDto proj)
        => proj.OutputType == null
           && (proj.Filename.EndsWith("Tests.csproj")
               || proj.Filename.Contains("Testing.csproj"));
}