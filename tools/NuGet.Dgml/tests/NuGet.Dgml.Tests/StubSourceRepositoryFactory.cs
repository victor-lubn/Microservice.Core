using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuGet
{
    internal static class StubSourceRepositoryFactory
    {
        internal static SourceRepository Create() => Create(Enumerable.Empty<PackageDependencyInfo>());

        internal static SourceRepository Create(StubPackageBuilder packageBuilder)
        {
            var packages = packageBuilder.BuildPackages();
            return Create(packages);
        }

        internal static SourceRepository Create(IEnumerable<PackageDependencyInfo> packages)
        {
            var dependencyInfo = Substitute.For<DependencyInfoResource>();
            var packageSearch = Substitute.For<PackageSearchResource>();

            var repository = new SourceRepository(new PackageSource(string.Empty), new INuGetResourceProvider[]
            {
                new StubNuGetResourceProvider(dependencyInfo),
                new StubNuGetResourceProvider(packageSearch),
            });

            foreach (var package in packages)
            {
                dependencyInfo.ResolvePackage(package, Arg.Any<NuGetFramework>(), Arg.Any<SourceCacheContext>(), Arg.Any<ILogger>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(new SourcePackageDependencyInfo(package.Id, package.Version, package.Dependencies, true, repository)));
                packageSearch.SearchAsync(package.Id, Arg.Any<SearchFilter>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ILogger>(), Arg.Any<CancellationToken>())
                    .Returns(new[] { PackageSearchMetadataBuilder.FromIdentity(package).Build() });
            }

            packageSearch.SearchAsync(string.Empty, Arg.Any<SearchFilter>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ILogger>(), Arg.Any<CancellationToken>())
                    .Returns(callInfo =>
                        packages
                            .GroupBy(p => p.Id)
                            .Select(g =>
                            {
                                var recentVersion = g.OrderByDescending(p => p.Version).First();
                                return new PackageDependencyInfo(g.Key, recentVersion.Version, recentVersion.Dependencies);
                            })
                            .Select(package => PackageSearchMetadataBuilder.FromIdentity(package).Build())
                            .Skip(callInfo.ArgAt<int>(2))
                            .Take(callInfo.ArgAt<int>(3))
                            .ToList());

            return repository;
        }
    }
}
