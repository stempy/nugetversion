using System.IO;
using System.Linq;
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
            var fileName = "sample-csproj.xml";
            var expectedPackage = "Microsoft.NET.Test.Sdk";
            var expectedVersion = "16.7.1";

            var nameFilter = "*Test*";
            var versionFilter = "16.*.1";

            var xmlProjectFile = Path.Combine(SamplesDir, fileName);

            var projectFile = new ProjectFile(xmlProjectFile,nameFilter, versionFilter);
            projectFile.QueryPackages();
            WriteOutput(projectFile);
            
            var fndPackage = projectFile.LastQueriedPackages.First();

            Assert.Equal(expectedPackage,fndPackage.Name);
            Assert.Equal(expectedVersion,fndPackage.Version);
        }

        [Fact]
        public void test_packagename_filter()
        {
            var fileName = "sample-csproj.xml";
            var expectedPackage = "Microsoft.NET.Test.Sdk";
            var nameFilter = "*.NET.Test.*";
            var xmlProjectFile = Path.Combine(SamplesDir, fileName);

            var projectFile = new ProjectFile(xmlProjectFile, nameFilter, "");
            projectFile.QueryPackages();
            WriteOutput(projectFile);
            var fndPackage = projectFile.LastQueriedPackages.First();

            Assert.Equal(expectedPackage,fndPackage.Name);
        }

        [Fact]
        public void test_version_filter()
        {
            var fileName = "sample-csproj.xml";
            var expectedVersion = "16.7.1";
            var versionFilter = "16.7.*";
            var xmlProjectFile = Path.Combine(SamplesDir, fileName);
            var projectFile = new ProjectFile(xmlProjectFile, "",versionFilter);
            projectFile.QueryPackages();
            WriteOutput(projectFile);
            
            Assert.All(projectFile.LastQueriedPackages,
                x=> Assert.True(x.Version.Contains("16.7."),"invalid version"));
        }
    }
}
