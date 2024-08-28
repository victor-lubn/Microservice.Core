# NuGet.Dgml

The aim of the repository is to provide tools for analyzing and visualizing
dependencies of NuGet packages leveraging directed graphs
([DGML](https://en.wikipedia.org/wiki/DGML)).

The library was indented to manage and maintain local repositories at home or
in business. It's not recommended to execute the functions against a large
repository like <nuget.org>.

## Usage

You only have to import the namespace *NuGet.Dgml* and look for new extension
methods on the NuGet types.

### Sample:

```c#
using NuGet.Dgml;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

var repository = Repository.Factory.GetCoreV3(@"N:\My Package Repository\");
var directedGraph = await repository.VisualizeUpgradeableDependenciesAsync().ConfigureAwait(false);
directedGraph.AsXDocument().Save(@"C:\My Package Repository.dgml");
```

## Functions

The functions are implemented as extension methods. Every part of the library is public.

| Extension method for type: | SourceRepository | PackageIdentity |
|:--|:-:|:-:|
| VisualizeUpgradeableDependencies | Implemented  | Implemented |

## Applications supporting DGML

- Visual Studio 2010(+): viewer and visual editor

## Contributing

Just follow the [GitHub Flow](https://guides.github.com/introduction/flow/).
