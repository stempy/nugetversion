using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NugetVersion.GetNugetVersions
{
    internal class NugetPackageVersionUtil
    {
        private static ConcurrentDictionary<string, NuGetVersion> _nugetVersionLatest = new();
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public NugetPackageVersionUtil(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public async Task<NuGetVersion> GetLatestNugetPackageVersionAsync(string packageId)
        {
            if (!_nugetVersionLatest.ContainsKey(packageId))
            {
                try
                {
                    var versions = await GetNugetVersionsForPackageAsync(packageId);
                    var version = versions.OrderByDescending(x => x.Version)
                        .FirstOrDefault(x => !x.IsPrerelease);

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
