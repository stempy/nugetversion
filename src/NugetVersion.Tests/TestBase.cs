using System.IO;
using System.Reflection;
using System.Text.Json;
using Xunit.Abstractions;

namespace NugetVersion.Tests
{
    public abstract class TestBase
    {
        private string _baseDor;
        private readonly ITestOutputHelper _output;

        protected TestBase(ITestOutputHelper outputHelper)
        {
            _output = outputHelper;
        }

        protected string GetSamplesDir()
        {
            return Path.Combine(GetTestDir(), "samples");
        }

        protected string GetTestDir()
        {
            return _baseDor ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        protected void WriteOutput(object model)
        {
            _output.WriteLine(JsonSerializer.Serialize(model, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
        }
    }
}