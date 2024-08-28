using Xunit;

namespace NuGet
{
    public class PackageUpgradeFacts
    {
        public class Constructor
        {
            [Fact]
            public void AcceptsNull() => new PackageUpgrade(null, PackageUpgradeAction.None, null);
        }

        public class PackageDependency
        {
            [Fact]
            public void ReturnsConstructorParameter()
            {
                var packageDependency = StubPackageDependencyFactory.CreateExact("A", "1.0.0");
                var packageUpgrade = new PackageUpgrade(packageDependency, PackageUpgradeAction.None, null);
                Assert.Same(packageDependency, packageUpgrade.PackageDependency);
            }
        }

        public class Action
        {
            [Fact]
            public void ReturnsConstructorParameter()
            {
                var packageUpgrade = new PackageUpgrade(null, PackageUpgradeAction.PrereleaseToRelease, null);
                Assert.Equal(PackageUpgradeAction.PrereleaseToRelease, packageUpgrade.Action);
            }
        }

        public class Package
        {
            [Fact]
            public void ReturnsConstructorParameter()
            {
                var package = StubPackageFactory.CreatePackage("A", "1.0.0");
                var packageUpgrade = new PackageUpgrade(null, PackageUpgradeAction.None, package);
                Assert.Equal(package, packageUpgrade.Package);
            }
        }

        public new class ToString
        {
            [Fact]
            public void ConsistsOfPackageDependencyAndAction()
            {
                PackageUpgrade packageUpgrade;

                var packageDependency = StubPackageDependencyFactory.CreateExact("A", "1.0.0");
                var package = StubPackageFactory.CreatePackage("A", "1.2.0");
                packageUpgrade = new PackageUpgrade(packageDependency, PackageUpgradeAction.MinVersion, package);
                Assert.Equal("A [1.0.0, 1.0.0] MinVersion -> A 1.2.0", packageUpgrade.ToString());

                packageDependency = StubPackageDependencyFactory.Create("B", "1.0.0", "2.0.0");
                package = StubPackageFactory.CreatePackage("B", "2.1.0-beta2");
                packageUpgrade = new PackageUpgrade(packageDependency, PackageUpgradeAction.ReleaseToPrerelease, package);
                Assert.Equal("B [1.0.0, 2.0.0) ReleaseToPrerelease -> B 2.1.0-beta2", packageUpgrade.ToString());
            }
        }
    }
}
