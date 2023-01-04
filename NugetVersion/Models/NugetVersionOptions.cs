namespace NugetVersion.Models;

public class NugetVersionOptions
{
    public string BasePath { get; set; }
    public string OutputFile { get; set; }
    public SearchQueryFilter SearchFilter { get; set; }

    public string SetNewVersionTo { get; set; }

    public bool IsSetNewVersion =>
        !string.IsNullOrEmpty(SetNewVersionTo);

    public string OutputFileFormat { get; set; }

    public bool RenderProjectReferences { get; set; } = true;
}