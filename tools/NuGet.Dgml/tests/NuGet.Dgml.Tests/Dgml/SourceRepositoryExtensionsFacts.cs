using System;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Protocol.Core.Types;
using Xunit;

namespace NuGet.Dgml
{
    public class SourceRepositoryExtensionsFacts
    {
        public class VisualizeUpgradeableDependenciesAsyncSourceRepository
        {
            [Fact]
            public async Task ThrowsOnNullPackageRepository()
            {
                SourceRepository repository = null;
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "repository",
                    () => repository.VisualizeUpgradeableDependenciesAsync());
            }

            /* No further tests necessary because the method calls already tested methods. */
        }

        public class VisualizeUpgradeableDependenciesAsyncSourceRepositoryNuGetFramework
        {
            private readonly NuGetFramework _targetFramework;

            public VisualizeUpgradeableDependenciesAsyncSourceRepositoryNuGetFramework() 
                => _targetFramework = new StubNuGetFrameworkFactory().NET45();

            [Fact]
            public async Task ThrowsOnNullPackageRepository()
            {
                SourceRepository repository = null;
                await Assert.ThrowsAsync<ArgumentNullException>(
                    "repository",
                    () => repository.VisualizeUpgradeableDependenciesAsync(_targetFramework));
            }

            [Fact]
            public async Task RecentPackagesAreNodesWithVersion()
            {
                var packageBuilder = new StubPackageBuilder();
                packageBuilder.AddPackageDefinitions("PackageA", "0.1.0", "0.2.0-rc1", "1.0.0");
                packageBuilder.AddPackageDefinitions("PackageB", "1.0.1", "1.0.2", "1.0.3-beta");
                var repository = StubSourceRepositoryFactory.Create(packageBuilder);

                var directedGraph = await repository.VisualizeUpgradeableDependenciesAsync(_targetFramework);

                Assert.Equal(2, directedGraph.Nodes.Length);
                Assert.Equal("PackageA 1.0.0", directedGraph.Nodes[0].Label);
                Assert.Equal("PackageB 1.0.3-beta", directedGraph.Nodes[1].Label);
                Assert.Null(directedGraph.Links);
            }

            /* No further tests necessary because the method calls already tested methods. */
        }
    }
}
