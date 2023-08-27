# NugetVersion .NET Core Global tool 

.NET Core Global Tool for querying and updating Nuget project versions across multiple projects within a directory

Global tool for .NET 6.x+ and later

.NET 6.x+ is required for [global tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools),
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
nugetversion src-dir-to-project-folders [-n|--name nameSpec] [-v|--version versionSpec] [-o filepath] [-of json] [-fw targetFramework]
```

* nameSpec can be a wildcard search like `*something*`
* versionSpec can be a wildcard search, eg `1.*.0`

**Arguments**

- `-n nameSpec` - nuget name filter with wildcards ie `-n Microsoft.*`
- `-v versionSpec` - version filter ie `-v 1.0.*`
- `-o filepath` - output results to file `-o somefile.txt`, now with `-of` flag this would suit `-of somefile.json`
- `-of output-format` - format to output, ie `-of json` to output json, combine with `-o` flag to write file
- `-fw targetFramework` - filter by target framework, ie `-fw net5.0`
- `-sv|--set-version` - to set new version for queried packages, can use `-sv latest` to set to latest available version
- `-supver|--suppress-version-checks` - suppress remote version checks. Can speed up results, but will not load latest versions.


### Update Versions
To update versions for a specific query, specify the `--set-version <VERSION>|latest` switch, this uses the internal .NET core package command so is generally safe to use, however just be aware it will update according to the query you specify.

This will show a list of items to be updated, and will prompt before starting the update process.

```cmd
nugetversion src-dir-to-project-folders [-n|--name nameSpec] [-v|--version versionSpec] --set-version x.x.x
```

> WARNING: this will update ALL project file(s) and projects that match that criteria to the specific version specified if that version is available for that package.

Examples:

**Set to latest available**

```sh
nugetversion mypath-to-src -n Microsoft.Extensions* -sv latest
```

**Set to specific version**

```sh
nugetversion mypath-to-src -n Microsoft.Extensions* -sv 6.0.0
```




## Release History

1.6.0

- fix: multiple target frameworks crash
- added: getting latest nuget package version.  can be suppressed with `-supver`
- added: setting versions to latest using `-sv latest`

1.5.0

- upgraded .net versions to .net6.0 LTS
- added --suppressrefs option to exclude project references

1.0.6

- Added output to file flag -o [filepath]
- Added output format flag -of [json] - currently just json to render as json
- Added framework filter -fw [targetframework] - ie `net5.0`
- Added project references
- Some code cleanups

