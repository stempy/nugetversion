using System.IO;
using System.Linq;
using NugetVersion.Models;
using NugetVersion.Project;
using Xunit;
using Xunit.Abstractions;

namespace NugetVersion.Tests
{
    public class ProjectPackageQueryTests : TestBase
    {
        private string SamplesDir => GetSamplesDir();
        
        
        public ProjectPackageQueryTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void test_packagename_and_version_filter_combined()
        {
            // 1. arrange
            var fileName = "sample-csproj.xml";
            var expectedPackage = "Microsoft.NET.Test.Sdk";
            var expectedVersion = "16.7.1";

            var nameFilter = "*Test*";
            var versionFilter = "16.*.1";

            var xmlProjectFile = Path.Combine(SamplesDir, fileName);

            var filter = new SearchQueryFilter()
            {
                Name = nameFilter,
                Version = versionFilter
            };

            var projectFile = new ProjectFile(xmlProjectFile,filter);
            
            // 2. act
            projectFile.QueryPackages();

            // 3. assert
            WriteOutput(projectFile);
            var fndPackage = projectFile.LastQueriedPackages.First();
            Assert.Equal(expectedPackage,fndPackage.Name);
            Assert.Equal(expectedVersion,fndPackage.Version);
        }

        [Fact]
        public void test_packagename_filter()
        {
            // 1. arrange
            var fileName = "sample-csproj.xml";
            var expectedPackage = "Microsoft.NET.Test.Sdk";
            var nameFilter = "*.NET.Test.*";
            var xmlProjectFile = Path.Combine(SamplesDir, fileName);

            var filter = new SearchQueryFilter()
            {
                Name = nameFilter,
                Version = ""
            };

            // 2. act
            var projectFile = new ProjectFile(xmlProjectFile, filter);
            projectFile.QueryPackages();

            // 3. assert
            WriteOutput(projectFile);
            var fndPackage = projectFile.LastQueriedPackages.First();
            Assert.Equal(expectedPackage,fndPackage.Name);
        }

        [Fact]
        public void test_version_filter()
        {
            // 1. arrange
            var fileName = "sample-csproj.xml";
            var expectedVersionContains = "16.7.";
            var versionFilter = "16.7.*";
            var xmlProjectFile = Path.Combine(SamplesDir, fileName);

            var filter = new SearchQueryFilter()
            {
                Name = "",
                Version = versionFilter
            };

            // 2. act
            var projectFile = new ProjectFile(xmlProjectFile, filter);
            projectFile.QueryPackages();

            // 3. assert
            WriteOutput(projectFile);
            Assert.All(projectFile.LastQueriedPackages,
                x=> Assert.True(x.Version.Contains(expectedVersionContains),"invalid version"));
        }
    }
}
