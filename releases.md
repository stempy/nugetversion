# Release History

1.5.0

- upgraded .net versions to .net6.0 LTS
- added --suppressrefs option to exclude project references

1.0.6

- Added output to file flag -o [filepath]
- Added output format flag -of [json] - currently just json to render as json
- Added framework filter -fw [targetframework] - ie `net5.0`
- Added project references
- Some code cleanups

1.2.0

- fix: multiple target frameworks crash
- added: getting latest nuget package version.  can be suppressed with `-supver`
- added: setting versions to latest using `-sv latest`