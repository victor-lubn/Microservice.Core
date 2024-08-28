using NuGet.Versioning;

namespace NuGet
{
    /// <summary>
    /// Defines base information of a package.
    /// </summary>
    public class Package
    {
        /// <summary>
        /// Gets or sets the id of the package.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the package.
        /// </summary>
        public NuGetVersion Version { get; set; } = new NuGetVersion(0, 0, 1);
    }
}
