using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using Xunit;

namespace NuGet.Dgml
{
    public class IEnumerablePackageIdentityExtensionsFacts
    {
        public class VisualizeUpgradeableDependenciesAsyncIEnumerablePackageIdentitySourceRepository
        {
            [Fact]
            public async Task ThrowsOnNullPackages()
            {
                IEnumerable<PackageDependencyInfo> packages = null;
                var repository = StubSourceRepositoryFactory.Create();
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "packages",
                    () => packages.VisualizeUpgradeableDependenciesAsync(repository));
            }

            [Fact]
            public async Task ThrowsOnNullPackageRepository()
            {
                var packages = Enumerable.Empty<PackageDependencyInfo>();
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "repository",
                    () => packages.VisualizeUpgradeableDependenciesAsync(null));
            }

            /* No further tests necessary because the method calls already tested methods. */
        }

        public class VisualizeUpgradeableDependenciesAsyncIEnumerablePackageIdentitySourceRepositoryNuGetFramework
        {
            private readonly NuGetFramework _targetFramework;

            public VisualizeUpgradeableDependenciesAsyncIEnumerablePackageIdentitySourceRepositoryNuGetFramework()
                => _targetFramework = new StubNuGetFrameworkFactory().NET45();

            [Fact]
            public async Task ThrowsOnNullPackages()
            {
                IEnumerable<PackageDependencyInfo> packages = null;
                var repository = StubSourceRepositoryFactory.Create();
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "packages",
                    () => packages.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework));
            }

            [Fact]
            public async Task ThrowsOnNullPackageRepository()
            {
                var packages = Enumerable.Empty<PackageDependencyInfo>();
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "repository",
                    () => packages.VisualizeUpgradeableDependenciesAsync(null, _targetFramework));
            }

            [Fact]
            public async Task PackagesAreNodesWithVersion()
            {
                var packageBuilder = new StubPackageBuilder();
                packageBuilder.AddPackageDefinition("A", "1.0.0");
                packageBuilder.AddPackageDefinition("B", "2.1.0-beta");
                var packages = packageBuilder.BuildPackages();
                var repository = StubSourceRepositoryFactory.Create(packages);

                var directedGraph = await packages.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Equal(2, directedGraph.Nodes.Length);
                Assert.Equal("A 1.0.0", directedGraph.Nodes[0].Label);
                Assert.Equal("B 2.1.0-beta", directedGraph.Nodes[1].Label);
                Assert.Null(directedGraph.Links);
            }

            [Fact]
            public async Task NoneUpgradeActionIsBlackLink()
            {
                var package = StubPackageFactory.CreatePackage("Package", "1.0.0", StubPackageDependencyFactory.Create("Dependency", "1.0.0"));
                var dependency = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependency, });

                var directedGraph = await new[] { package, }.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Equal("Black", directedGraph.Links[0].Stroke);
            }

            [Fact]
            public async Task MinVersionUpgradeActionIsForestGreenLink()
            {
                var package = StubPackageFactory.CreatePackage("Package", "1.0.0", StubPackageDependencyFactory.Create("Dependency", "1.0.0"));
                var dependency1 = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var dependency2 = StubPackageFactory.CreatePackage("Dependency", "2.0.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependency1, dependency2, });

                var directedGraph = await new[] { package, }.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Equal("ForestGreen", directedGraph.Links[0].Stroke);
            }

            [Fact]
            public async Task ReleaseToReleaseUpgradeActionIsGoldenrodLink()
            {
                var package = StubPackageFactory.CreatePackage("Exact", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0"));
                var dependency10 = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var dependency11 = StubPackageFactory.CreatePackage("Dependency", "1.1.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependency10, dependency11, });

                var directedGraph = await new[] { package, }.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Equal("Goldenrod", directedGraph.Links[0].Stroke);
            }

            [Fact]
            public async Task PrereleaseToReleaseUpgradeActionIsDarkOrangeLink()
            {
                var package = StubPackageFactory.CreatePackage("Exact", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0-alpha"));
                var dependencyPrerelease = StubPackageFactory.CreatePackage("Dependency", "1.0.0-alpha");
                var dependencyRelease = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependencyPrerelease, dependencyRelease, });

                var directedGraph = await new[] { package, }.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Equal("DarkOrange", directedGraph.Links[0].Stroke);
            }

            [Fact]
            public async Task PrereleaseToPrereleaseUpgradeActionIsOrangeRedLink()
            {
                var package = StubPackageFactory.CreatePackage("Package", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0-alpha"));
                var dependencyAlpha = StubPackageFactory.CreatePackage("Dependency", "1.0.0-alpha");
                var dependencyBeta = StubPackageFactory.CreatePackage("Dependency", "1.0.0-beta");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependencyAlpha, dependencyBeta, });

                var directedGraph = await new[] { package, }.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Equal("OrangeRed", directedGraph.Links[0].Stroke);
            }

            [Fact]
            public async Task ReleaseToPrereleaseUpgradeActionIsFirebrickLink()
            {
                var package = StubPackageFactory.CreatePackage("Exact", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0"));
                var dependencyRelease = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var dependencyPrerelease = StubPackageFactory.CreatePackage("Dependency", "1.1.0-pre");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependencyRelease, dependencyPrerelease, });

                var directedGraph = await new[] { package, }.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Equal("Firebrick", directedGraph.Links[0].Stroke);
            }

            [Fact]
            public async Task UnkownUpgradeActionIsDarkGrayLink()
            {
                var package = StubPackageFactory.CreatePackage("Exact", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0"));
                var repository = StubSourceRepositoryFactory.Create(new[] { package, });

                var directedGraph = await new[] { package, }.VisualizeUpgradeableDependenciesAsync(repository, _targetFramework);

                Assert.Equal("DarkGray", directedGraph.Links[0].Stroke);
            }

            /* No further tests necessary because the method calls already tested methods. */
        }
    }
}
