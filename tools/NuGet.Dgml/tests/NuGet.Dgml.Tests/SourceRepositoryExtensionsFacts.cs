using System;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using Xunit;

namespace NuGet
{
    public class SourceRepositoryExtensionsFacts
    {
        public class GetRecentPackages
        {
            [Fact]
            public async Task ThrowsOnNull()
            {
                SourceRepository repository = null;
                await Assert.ThrowsAsync<ArgumentNullException>("repository", () => repository.GetRecentPackagesAsync());
            }

            [Fact]
            public async Task ReturnsEmptyForEmptyPackageRepository()
            {
                var repository = StubSourceRepositoryFactory.Create();
                Assert.Empty(await repository.GetRecentPackagesAsync());
            }

            [Theory]
            [InlineData("1.0.0", "1.0.0")]
            [InlineData("2.0.0-beta", "1.0.0;2.0.0-beta")]
            [InlineData("2.0.0-rc", "1.0.0;2.0.0-beta;2.0.0-rc")]
            [InlineData("2.0.0", "1.0.0;2.0.0-beta;2.0.0-rc;2.0.0")]
            [InlineData("2.0.1", "1.0.0;2.0.0-beta;2.0.0-rc;2.0.0;2.0.1")]
            public async Task ReturnsRecentVersionOfPackage(string expected, string versions)
            {
                var packages = StubPackageFactory.CreatePackages("Package", versions.Split(';'));
                var repository = StubSourceRepositoryFactory.Create(packages);

                var recentPackages = await repository.GetRecentPackagesAsync();

                Assert.Equal(expected, recentPackages.Single().Version.ToString());
            }
        }
    }
}
