using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NugetVersion.PackageReference;
using NugetVersion.Project;
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


            //using (var ms = new MemoryStream())
            //{
            //    using (var xtw = new XmlTextWriter(ms,Encoding.UTF8))
            //    {
            //        xtw.Formatting = Formatting.Indented; // optional, if you want it to look nice
            //        doc.WriteTo(xtw);
            //    }
            //    resultDoc = Encoding.UTF8.GetString(ms.ToArray());
            //}

            WriteOutput(resultDoc);
        }
    }
    
    //public class ProjectPackageReferenceUpdaterTests : TestBase
    //{
    //    private IPackageReferenceUpdater _packageReferenceUpdater;
        
    //    public ProjectPackageReferenceUpdaterTests(ITestOutputHelper outputHelper) : base(outputHelper)
    //    {
    //    }

    //    IPackageReferenceUpdater CreatePackageReferenceUpdater()
    //    {
    //        return _packageReferenceUpdater ??= new XmlPackageReferenceUpdater();
    //    }

    //    /// <summary>
    //    /// TODO: fix unit tests up for xml package reference
    //    /// </summary>
    //    [Fact]
    //    public void test_project_version_update_for_single_package()
    //    {
    //        var fileName = "sample-csproj.xml";
    //        var expectedPackage = "Microsoft.NET.Test.Sdk";
    //        var expectedVersion = "16.7.1";

    //        var newVersion = "9.9.9";

    //        var xmlProjectFile = Path.Combine(GetSamplesDir(), fileName);

    //        //get initial project file info
    //        var projectFile = new ProjectFile(xmlProjectFile, expectedPackage, expectedVersion);
    //        projectFile.QueryPackages();
    //        WriteOutput(projectFile);
    //        var pkg = projectFile.LastQueriedPackages.First();
    //        Assert.Equal(expectedPackage, pkg.Name);
    //        Assert.Equal(expectedVersion, pkg.Version);

    //        // // now update project and get updated info
    //        //var versionUpdater = new ProjectNugetVersionUpdater(CreatePackageReferenceUpdater());
    //        //versionUpdater.UpdateVersionInProject(xmlProjectFile, expectedPackage, expectedVersion, newVersion);

    //        //projectFile = new ProjectFile(xmlProjectFile, expectedPackage, expectedVersion);
    //        //projectFile.QueryPackages();
    //        //WriteOutput(projectFile);
    //        //pkg = projectFile.LastQueriedPackages.First();
    //        //Assert.Equal(expectedPackage, pkg.Name);
    //        //Assert.Equal(expectedVersion, newVersion);


    //    }
    //}
}