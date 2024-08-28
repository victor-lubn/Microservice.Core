using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;

namespace NuGet
{
    internal class StubPackageBuilder
    {
        private readonly IList<Tuple<string, string, IEnumerable<PackageDependency>>> _packageDefinitions =
            new List<Tuple<string, string, IEnumerable<PackageDependency>>>();

        public void AddPackageDefinitions(string id, params string[] versions)
        {
            foreach (var version in versions)
                AddPackageDefinition(id, version);
        }

        public void AddPackageDefinition(string id, string version)
            => AddPackageDefinition(id, version, Enumerable.Empty<PackageDependency>());

        public void AddPackageDefinition(string id, string version, PackageDependency packageDependency)
            => AddPackageDefinition(id, version, new[] { packageDependency, });

        public void AddPackageDefinition(string id, string version, IEnumerable<PackageDependency> packageDependencies)
            => _packageDefinitions.Add(Tuple.Create(id, version, packageDependencies));

        public IEnumerable<PackageDependencyInfo> BuildPackages()
            => _packageDefinitions.Select(definition => StubPackageFactory.CreatePackage(definition.Item1, definition.Item2, definition.Item3));
    }
}
