using Microsoft.Extensions.Configuration;
using NuGet.Configuration;
using NuGet.Dgml;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Nuget.Dgml.Generator
{
    internal class Program
    {
        private const string NoneUpgradeActionColor = "Black";

        private static async Task Main()
        {
            var options = new GraphOptions();
            var configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            configuration.Bind(options);

            var defaultSettings = Settings.LoadDefaultSettings(null);
            var provider = new PackageSourceProvider(defaultSettings);
            var coreSource = provider.GetPackageSourceByName(options.PackageSourceName);

            if (coreSource == null)
            {
                Console.WriteLine($"Package source with name '{options.PackageSourceName}' not found.");
                return;
            }

            var repository = Repository.Factory.GetCoreV3(coreSource);
            var directedGraph = await repository.VisualizeUpgradeableDependenciesAsync(options.SearchTerm).ConfigureAwait(false);

            if (directedGraph.Links == null)
            {
                Console.WriteLine("No packages found.");
                return;
            }

            if (options.ShowOnlyUpgradableLinks)
            {
                directedGraph.Links = directedGraph.Links.Where(l => l.Stroke != NoneUpgradeActionColor).ToArray();
            }

            if (options.ShowOnlySearchTermNodes)
            {
                directedGraph.Links = directedGraph.Links.Where(l => l.Target.Contains(options.SearchTerm)).ToArray();
                directedGraph.Nodes = directedGraph.Nodes.Where(n => n.Id.Contains(options.SearchTerm)).ToArray();
            }

            var fileName = options.PackageSourceName.Replace(".", "") + ".dgml";
            directedGraph.AsXDocument().Save(fileName);

            Console.WriteLine($"Graph '{fileName}' generated.");
        }
    }
}
