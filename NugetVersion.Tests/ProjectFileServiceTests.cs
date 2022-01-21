using System.Linq;
using NugetVersion.Models;
using NugetVersion.Project;
using Xunit;
using Xunit.Abstractions;

namespace NugetVersion.Tests
{
    public class ProjectFileServiceTests : TestBase
    {
        private readonly ProjectFileService _projectFileService;

        public ProjectFileServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            _projectFileService = new ProjectFileService();
        }

        [Fact]
        public void test_framework_filter()
        {
            var net5query = new SearchQueryFilter() { TargetFramework = "net5.0" };
            var net3query = new SearchQueryFilter() { TargetFramework = "netcoreapp3.1" };

            var expectedNet5ProjectCount = 1;
            var expectedNet3ProjectCount = 2;

            var net5queryResults = _projectFileService.GetProjectFilesByFilter(GetSamplesDir(), net5query, "*-csproj.xml");
            Assert.Equal(expectedNet5ProjectCount, net5queryResults.Count());

            var net3queryResults = _projectFileService.GetProjectFilesByFilter(GetSamplesDir(), net3query, "*-csproj.xml");
            Assert.Equal(expectedNet3ProjectCount, net3queryResults.Count());
        }
    }
}