using System.IO;
using System.Xml.Linq;
using NugetVersion.PackageReference;
using Xunit;
using Xunit.Abstractions;

namespace NugetVersion.Tests
{
    public class XmlPackageReferenceUpdaterTests : TestBase
    {
        public XmlPackageReferenceUpdaterTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void test_xml_updater()
        {
            var fileName = "sample-csproj.xml";
            var expectedPackage = "Microsoft.NET.Test.Sdk";
            var expectedVersion = "16.7.1";

            var newVersion = "9.9.9";

            var xmlProjectFile = Path.Combine(GetSamplesDir(), fileName);

            var doc = XDocument.Parse(File.ReadAllText(xmlProjectFile));
            
            var updater = new XmlPackageReferenceUpdater();
            updater.UpdateVersion(doc, expectedPackage, expectedVersion, newVersion);

            string resultDoc;

            using (var writer = new StringWriter())
            {
                doc.Save(writer);
                resultDoc= writer.ToString();
            }

            WriteOutput(resultDoc);
        }
    }
    
    
}