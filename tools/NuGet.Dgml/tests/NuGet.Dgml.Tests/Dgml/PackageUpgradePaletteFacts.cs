using System;
using Xunit;

namespace NuGet.Dgml
{
    public class PackageUpgradePaletteFacts
    {
        public class Contructor
        {
            [Fact]
            public void InitializesADefaultUpgradeActionPalette()
            {
                var palette = new PackageUpgradePalette();
                Assert.NotNull(palette.UpgradeActionPalette);
            }
        }

        public class ContructorPackageUpgradeActionPalette
        {
            [Fact]
            public void ThrowsOnNull()
                => Assert.Throws<ArgumentNullException>("upgradeActionPalette", () => new PackageUpgradePalette(null));
        }

        public class PrereleaseColor
        {
            [Fact]
            public void StoresValue()
            {
                var palette = new PackageUpgradePalette();
                const string expected = "myColor";
                palette.PrereleaseColor = expected;
                Assert.Equal(expected, palette.PrereleaseColor);
            }

            [Fact]
            public void DefaultValueIsNull()
            {
                var palette = new PackageUpgradePalette();
                Assert.Null(palette.PrereleaseColor);
            }
        }

        public class MissingPackageColor
        {
            [Fact]
            public void StoresValue()
            {
                var palette = new PackageUpgradePalette();
                const string expected = "myColor";
                palette.MissingPackageColor = expected;
                Assert.Equal(expected, palette.MissingPackageColor);
            }

            [Fact]
            public void DefaultValueIsNull()
            {
                var palette = new PackageUpgradePalette();
                Assert.Null(palette.MissingPackageColor);
            }
        }

        public class UpgradeActionPalette
        {
            [Fact]
            public void ReturnsConstructorParameter()
            {
                var upgradeActionPalette = new PackageUpgradeActionPalette();
                var palette = new PackageUpgradePalette(upgradeActionPalette);
                Assert.Same(upgradeActionPalette, palette.UpgradeActionPalette);
            }
        }
    }
}
