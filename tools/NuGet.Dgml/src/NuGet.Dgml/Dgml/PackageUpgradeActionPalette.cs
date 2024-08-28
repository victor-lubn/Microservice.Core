using System.Collections.Generic;

namespace NuGet.Dgml
{
    /// <summary>
    /// Provides a color palette to visualize <see cref="PackageUpgradeAction"/> values.
    /// </summary>
    public class PackageUpgradeActionPalette
    {
        private readonly IDictionary<PackageUpgradeAction, string> _colors;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageUpgradeActionPalette"/> class.
        /// </summary>
        public PackageUpgradeActionPalette() => _colors = new Dictionary<PackageUpgradeAction, string>();

        /// <summary>
        /// Gets or sets the color of the specified <see cref="PackageUpgradeAction"/>.
        /// </summary>
        /// <param name="action">The enumeration value.</param>
        /// <returns>A default color palette with the specified <see cref="PackageUpgradeActionPalette"/>.</returns>
        public string this[PackageUpgradeAction action]
        {
            get
            {
                _colors.TryGetValue(action, out var color);
                return color;
            }
            set => _colors[action] = value;
        }
    }
}
