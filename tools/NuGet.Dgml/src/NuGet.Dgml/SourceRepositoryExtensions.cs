using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuGet
{
    /// <summary>
    /// Provides static extension methods for <see cref="SourceRepository"/>.
    /// </summary>
    public static class SourceRepositoryExtensions
    {
        /// <summary>
        /// Gets the recent packages of the specified repository.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="logger">The logger for actions performed on the repository.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The recent packages of the repository.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyCollection<PackageIdentity>> GetRecentPackagesAsync(this SourceRepository repository, string searchTerm = "", ILogger logger = null, CancellationToken? cancellationToken = null)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            var searchCancellationToken = cancellationToken ?? CancellationToken.None;
            logger = logger ?? NullLogger.Instance;

            var packageSearch = await repository.GetResourceAsync<PackageSearchResource>(searchCancellationToken).ConfigureAwait(false);
            var searchFilter = new SearchFilter(includePrerelease: true);

            var packages = new List<PackageIdentity>();
            List<IPackageSearchMetadata> searchResults;
            const int batchSize = 50;
            var index = 0;
            do
            {
                searchResults = (await packageSearch.SearchAsync(searchTerm, searchFilter, skip: index, take: batchSize, logger, searchCancellationToken).ConfigureAwait(false)).ToList();
                packages.AddRange(searchResults.Select(p => p.Identity));
                index += batchSize;
            }
            while (searchResults.Count > 0);

            return packages;
        }
    }
}
