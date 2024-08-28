using NuGet.Packaging.Core;

namespace NuGet
{
    /// <summary>
    /// Specifies how a <see cref="PackageDependency"/> can be upgraded.
    /// </summary>
    public enum PackageUpgradeAction
    {
        /// <summary>
        /// No upgrade is possible.
        /// </summary>
        None = 0,

        /// <summary>
        /// The minimum version of the satisfied version specification can be upgraded to the recent version.
        /// </summary>
        MinVersion = 1,

        /// <summary>
        /// The unsatisfied version specification can be upgraded from release to a recent release.
        /// </summary>
        ReleaseToRelease = 2,

        /// <summary>
        /// The unsatisfied version specification can be upgraded from prerelease to a recent release.
        /// </summary>
        PrereleaseToRelease = 3,

        /// <summary>
        /// The unsatisfied version specification can be upgraded from prerelease to a recent prerelease.
        /// </summary>
        PrereleaseToPrerelease = 4,

        /// <summary>
        /// The unsatisfied version specification can be upgraded from release to a recent prerelease.
        /// </summary>
        ReleaseToPrerelease = 5,

        /// <summary>
        /// The satisfaction of the version specification can't be determined.
        /// </summary>
        Unknown = 255,
    }
}
