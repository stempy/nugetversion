using System;
using System.Collections.Generic;
using System.IO;
using NugetVersion.Models;
using NugetVersion.Project;
using Console = System.Console;

namespace NugetVersion.Renderer
{
    public class ProjectFileRenderer
    {
        public const ConsoleColor ProjectFileNameColor = ConsoleColor.Gray;
        public const ConsoleColor ProjectInfoColor = ConsoleColor.DarkGray;

        public const ConsoleColor TargetFrameworkColor = ConsoleColor.DarkGray;

        public const ConsoleColor ProjectPackageNameColor = ConsoleColor.DarkCyan;
        public const ConsoleColor ProjectPackageVersionColor = ConsoleColor.DarkMagenta;

        public const ConsoleColor HighlightWarning = ConsoleColor.Yellow;
        public const ConsoleColor HighlightError = ConsoleColor.Red;
        
        public void Render(ProjectFile projectFile)
        {
            ConsoleRender.W($"\r\n{Path.GetFileName(projectFile.Filename)}\r\n",ProjectFileNameColor);

            var targetFrameworkColor = TargetFrameworkColor;

            var targetFramework = projectFile.TargetFramework;
            if (targetFramework == "netstandard2.0")
            {
                targetFrameworkColor = HighlightWarning;
                targetFramework += " (recommend 2.1)";
            } else if (targetFramework == "netcoreapp3.0")
            {
                targetFrameworkColor = HighlightWarning;
                targetFramework += " (3.0 is obsolete, use 3.1 or .NET 5.x+)";
            }
            
            ConsoleRender.W($"{targetFramework}",targetFrameworkColor);
            if (!string.IsNullOrEmpty(projectFile.OutputType))
            {
                ConsoleRender.W($" |  OutputType: {projectFile.OutputType}");
            }

            if (!string.IsNullOrEmpty(projectFile.ProjectSdk))
            {
                ConsoleRender.W($" |  Sdk: {projectFile.ProjectSdk} \r\n", ProjectInfoColor);
            }
            
        }

        public void Render(IEnumerable<PackageReferenceModel> items, int startTabPad, int maxNameWidth)
        {
            var tabIdx = startTabPad;
            var tabStr = new string(' ', tabIdx);
            var padRightMax = maxNameWidth + 10;
            foreach (var pr in items)
            {
                ConsoleRender.W($"{tabStr}{pr.Name.PadRight(padRightMax)}", ProjectPackageNameColor)
                    .W($"{pr.Version}\n", ProjectPackageVersionColor);
            }
        }
    }
}