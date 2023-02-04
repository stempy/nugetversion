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

        [Theory]
        [InlineData("net5.0",1)]
        [InlineData("netcoreapp3.1", 2)]
        public void test_framework_filter(string netFramework, int projectCount)
        {
            var query = new SearchQueryFilter() { TargetFramework = netFramework };

            var queryResults = _projectFileService.GetProjectFilesByFilter(
                GetSamplesDir(), 
                query, "*-csproj.xml");

            Assert.Equal(projectCount, queryResults.Count());
        }
    }
}