using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NuGet
{
    public class UpgradeWalkerFacts
    {
        public class ConstructorSourceRepository
        {
            [Fact]
            public void ThrowsOnNull() => Assert.Throws<ArgumentNullException>("repository", () => new UpgradeWalker(null));
        }

        public class ConstructorSourceRepositoryNuGetFramework
        {
            [Fact]
            public void ThrowsOnNullPackageRepository()
                => Assert.Throws<ArgumentNullException>("repository", () => new UpgradeWalker(null, new StubNuGetFrameworkFactory().NET45()));
        }

        public class GetPackageUpgrades
        {
            [Fact]
            public async Task ThrowsOnNull()
            {
                var walker = new UpgradeWalker(StubSourceRepositoryFactory.Create());
                await Assert.ThrowsAsync<ArgumentNullException>("package", () => walker.GetPackageUpgradesAsync(null));
            }

            [Fact]
            public async Task SatisfiedVersionSpecWithMatchingInclusiveMinVersionIsNotUpgradeable()
            {
                var package = StubPackageFactory.CreatePackage("Package", "1.0.0", StubPackageDependencyFactory.Create("Dependency", "1.0.0"));
                var dependency = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependency, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.None, upgrades.ElementAt(0).Action);
                Assert.Same(dependency, upgrades.ElementAt(0).Package);
            }

            [Fact]
            public async Task SatisfiedVersionSpecWithNotMatchingInclusiveMinVersionIsUpgradeable()
            {
                var package = StubPackageFactory.CreatePackage("Package", "1.0.0", StubPackageDependencyFactory.Create("Dependency", "1.0.0"));
                var dependency1 = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var dependency2 = StubPackageFactory.CreatePackage("Dependency", "2.0.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependency1, dependency2, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.MinVersion, upgrades.ElementAt(0).Action);
                Assert.Equal(dependency2, upgrades.ElementAt(0).Package);
            }

            [Fact]
            public async Task SatisfiedVersionSpecWithExclusiveMinVersionIsUpgradeable()
            {
                var package = StubPackageFactory.CreatePackage(
                    "Package",
                    "1.0.0",
                    StubPackageDependencyFactory.Create("Dependency", "1.0.0", null, false, false));
                var dependency = StubPackageFactory.CreatePackage("Dependency", "1.0.1-a");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependency, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.MinVersion, upgrades.ElementAt(0).Action);
                Assert.Equal(dependency, upgrades.ElementAt(0).Package);
            }

            [Fact]
            public async Task IdentifiesReleaseToPrerelease()
            {
                var package = StubPackageFactory.CreatePackage("Exact", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0"));
                var dependencyRelease = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var dependencyPrerelease = StubPackageFactory.CreatePackage("Dependency", "1.1.0-pre");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependencyRelease, dependencyPrerelease, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.ReleaseToPrerelease, upgrades.ElementAt(0).Action);
                Assert.Equal(dependencyPrerelease, upgrades.ElementAt(0).Package);
            }

            [Fact]
            public async Task IdentifiesReleaseToRelease()
            {
                var package = StubPackageFactory.CreatePackage("Exact", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0"));
                var dependency10 = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var dependency11 = StubPackageFactory.CreatePackage("Dependency", "1.1.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependency10, dependency11, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.ReleaseToRelease, upgrades.ElementAt(0).Action);
                Assert.Equal(dependency11, upgrades.ElementAt(0).Package);
            }

            [Fact]
            public async Task UnsatisfiedVersionSpecWithExclusiveMaxVersionIsPrerelease()
            {
                var package = StubPackageFactory.CreatePackage(
                    "Package",
                    "1.0.0",
                    StubPackageDependencyFactory.Create("Dependency", null, "1.0.0", false, false));
                var dependency = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependency, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.PrereleaseToRelease, upgrades.ElementAt(0).Action);
                Assert.Equal(dependency, upgrades.ElementAt(0).Package);
            }

            [Fact]
            public async Task IdentifiesPrereleaseToPrerelease()
            {
                var package = StubPackageFactory.CreatePackage("Package", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0-alpha"));
                var dependencyAlpha = StubPackageFactory.CreatePackage("Dependency", "1.0.0-alpha");
                var dependencyBeta = StubPackageFactory.CreatePackage("Dependency", "1.0.0-beta");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependencyAlpha, dependencyBeta, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.PrereleaseToPrerelease, upgrades.ElementAt(0).Action);
                Assert.Equal(dependencyBeta, upgrades.ElementAt(0).Package);
            }

            [Fact]
            public async Task IdentifiesPrereleaseToRelease()
            {
                var package = StubPackageFactory.CreatePackage("Exact", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0-alpha"));
                var dependencyPrerelease = StubPackageFactory.CreatePackage("Dependency", "1.0.0-alpha");
                var dependencyRelease = StubPackageFactory.CreatePackage("Dependency", "1.0.0");
                var repository = StubSourceRepositoryFactory.Create(new[] { package, dependencyPrerelease, dependencyRelease, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.PrereleaseToRelease, upgrades.ElementAt(0).Action);
                Assert.Equal(dependencyRelease, upgrades.ElementAt(0).Package);
            }

            [Fact]
            public async Task UndiscoverablePackageOfPackageDependencyIsUnknownUpgradeAction()
            {
                var package = StubPackageFactory.CreatePackage("Exact", "1.0.0", StubPackageDependencyFactory.CreateExact("Dependency", "1.0.0"));
                var repository = StubSourceRepositoryFactory.Create(new[] { package, });
                var walker = new UpgradeWalker(repository);

                var upgrades = await walker.GetPackageUpgradesAsync(package);

                Assert.Single(upgrades);
                Assert.Equal(PackageUpgradeAction.Unknown, upgrades.ElementAt(0).Action);
                Assert.Null(upgrades.ElementAt(0).Package);
            }
        }
    }
}
