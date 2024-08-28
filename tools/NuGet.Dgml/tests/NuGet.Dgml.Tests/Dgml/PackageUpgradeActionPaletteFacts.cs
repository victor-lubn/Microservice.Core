using Xunit;

namespace NuGet.Dgml
{
    public class PackageUpgradeActionPaletteFacts
    {
        public class Indexer
        {
            private readonly PackageUpgradeActionPalette _palette;

            public Indexer() => _palette = new PackageUpgradeActionPalette();

            [Theory]
            [InlineData(PackageUpgradeAction.PrereleaseToPrerelease)]
            [InlineData((PackageUpgradeAction)254)]
            public void StoresValue(PackageUpgradeAction action)
            {
                const string expected = "myColor";

                _palette[action] = expected;

                Assert.Equal(expected, _palette[action]);
            }

            [Fact]
            public void ReturnsNullForUnsetEnumValue()
            {
                Assert.Null(_palette[PackageUpgradeAction.MinVersion]);
                Assert.Null(_palette[(PackageUpgradeAction)254]);
            }
        }
    }
}
