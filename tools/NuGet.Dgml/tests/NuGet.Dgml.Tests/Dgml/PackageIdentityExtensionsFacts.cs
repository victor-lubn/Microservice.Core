using System;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using Xunit;

namespace NuGet.Dgml
{
    public class PackageIdentityExtensionsFacts
    {
        public class VisualizeUpgradeableDependenciesAsyncPackageIdentitySourceRepository
        {
            [Fact]
            public async Task ThrowsOnNullPackage()
            {
                PackageDependencyInfo package = null;
                var repository = StubSourceRepositoryFactory.Create();
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "package",
                    () => package.VisualizeUpgradeableDependenciesAsync(repository));
            }

            [Fact]
            public async Task ThrowsOnNullPackageRepository()
            {
                var package = StubPackageFactory.CreatePackage("A", "1.0.0");
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "repository",
                    () => package.VisualizeUpgradeableDependenciesAsync(null));
            }

            /* No further tests necessary because the method calls already tested methods. */
        }

        public class VisualizeUpgradeableDependenciesAsyncPackageIdentitySourceRepositoryNuGetFramework
        {
            private readonly NuGetFramework _targetFramework;

            public VisualizeUpgradeableDependenciesAsyncPackageIdentitySourceRepositoryNuGetFramework()
                => _targetFramework = new StubNuGetFrameworkFactory().NET45();

            [Fact]
            public async Task ThrowsOnNullPackage()
            {
                PackageDependencyInfo package = null;
                var repository = StubSourceRepositoryFactory.Create();
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "package",
                    () => package.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework));
            }

            [Fact]
            public async Task ThrowsOnNullPackageRepository()
            {
                var package = StubPackageFactory.CreatePackage("A", "1.0.0");
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "repository",
                    () => package.VisualizeUpgradeableDependenciesAsync(null, _targetFramework));
            }

            [Fact]
            public async Task PackageIsNodeWithVersion()
            {
                var package = StubPackageFactory.CreatePackage("A", "1.0.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, });

                var directedGraph = await package.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Single(directedGraph.Nodes);
                Assert.Equal("A 1.0.0", directedGraph.Nodes[0].Label);
                Assert.Null(directedGraph.Links);
            }

            /* No further tests necessary because the method calls already tested methods. */
        }
    }
}
