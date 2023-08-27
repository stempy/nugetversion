using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace NugetVersion.GetNugetVersions
{
    public class NugetPackageVersionUtil
    {
        private static ConcurrentDictionary<string, NuGetVersion> _nugetVersionLatest = new();
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(20);


        public NugetPackageVersionUtil(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        private string GetPathForLocalNugetCache()
        {
            return Path.Combine(Path.GetTempPath(), "_nugetversionsCache");
        }


        private string SaveNugetVersionCache(string packageId, NuGetVersion version)
        {
            var cachePath = GetPathForLocalNugetCache();
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            var file = Path.Combine(cachePath, $"{packageId}.json");
            var content = version?.ToFullString();
            File.WriteAllText(file, content);
            return file;
        }

        private NuGetVersion LoadNugetVersionCacheOrNull(string packageId)
        {
            var cachePath = GetPathForLocalNugetCache();
            var file = Path.Combine(cachePath, $"{packageId}.json");
            if (!File.Exists(file))
            {
                return null;
            }

            // has it expired
            if (new FileInfo(file).CreationTime.Add(_cacheTimeSpan) < DateTime.Now)
            {
                return null;
            }

            var fullVersionString = File.ReadAllText(file);

            // ok if version string is empty.... lets set it to 0.0.0.0 which means we have checked remote server for it (ie not null)
            // but no actual result found, but we dont want to fetch from server again soon, as it uses a request.
            if (string.IsNullOrWhiteSpace(fullVersionString))
            {
                return new NuGetVersion(0, 0, 0);
            }

            var model = new NuGetVersion(fullVersionString);
            return model;
        }

        public async Task<NuGetVersion> GetLatestNugetPackageVersionAsync(string packageId)
        {
            if (!_nugetVersionLatest.ContainsKey(packageId))
            {
                try
                {
                    // 1. try file cache
                    NuGetVersion version = LoadNugetVersionCacheOrNull(packageId);
                    if (version == null)
                    {
                        // 2. try remote source
                        var versions = await GetNugetVersionsForPackageAsync(packageId);
                        version = versions.OrderByDescending(x => x.Version)
                            .FirstOrDefault(x => !x.IsPrerelease);

                        // 3. save to cache, null or otherwise, null so we don't try fetch remote again
                        SaveNugetVersionCache(packageId, version);
                    }

                    _nugetVersionLatest.AddOrUpdate(packageId, version, (key, oldValue) => version);
                    _logger.LogInformation("Package: {PackageId} latest version {version} added to cache", packageId, version);

                }
                catch (Exception ex)
                {
                    _logger.LogError("NugetCheck-Failed: {PackageId} - {Message}",
                        packageId, ex.Message);
                }
            }

            return _nugetVersionLatest[packageId];
        }

        public async Task<IEnumerable<NuGetVersion>> GetNugetVersionsForPackageAsync(string packageId)
        {
            CancellationToken cancellationToken = CancellationToken.None;
            NuGet.Common.ILogger logger = NullLogger.Instance;
            var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            var versions = await resource.GetAllVersionsAsync(
                packageId,
                cache,
                logger,
                cancellationToken);
            return versions;
        }

    }


}
