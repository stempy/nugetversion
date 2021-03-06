# NugetVersion .NET Core Global tool [![Build Status](https://www.myget.org/BuildSource/Badge/websolving?identifier=d8511a4a-1125-401b-bf4c-fde985065cd4)]

.NET Core Global Tool for querying and updating Nuget project versions across multiple projects within a directory

### <a name="dotnet-nugetversion-tool"></a> Global tool for .NET Core 2.1 and later

.NET Core 2.1 is required for [global tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools),
meaning that you can install `nugetversion` using the .NET CLI and use it everywhere.

> :warning: To use global tools, .Net Core SDK 2.1.300 or later is required. 

Install `nugetversion` as a global tool first time

```cmd
dotnet tool install -g nugetversion [--version version]
```

Update `nugetversion` to the latest, or specified

```cmd
dotnet tool update -g nugetversion
```

Uninstall

```cmd
dotnet tool uninstall nugetversion
```

### Query
From this point on, you can run the following to query all projects for nuget package references, with optional name and version filtering

```cmd
nugetversion src-dir-to-project-folders [-n|--name nameSpec] [-v|--version versionSpec]
```

* nameSpec can be a wildcard search like `*something*`
* versionSpec can be a wildcard search, eg `1.*.0`

### Update Versions
To update versions for a specific query, specify the `--set-version <VERSION>` switch, this uses the internal .NET core package command so is generally safe to use, however just be aware it will update according to the query you specify.

This will show a list of items to be updated, and will prompt before starting the update process.

```cmd
nugetversion src-dir-to-project-folders [-n|--name nameSpec] [-v|--version versionSpec] --set-version x.x.x
```

> WARNING: this will update ALL project file(s) and projects that match that criteria to the specific version specified if that version is available for that package.

