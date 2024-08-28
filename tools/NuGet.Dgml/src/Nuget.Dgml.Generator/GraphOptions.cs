namespace Nuget.Dgml.Generator
{
    public class GraphOptions
    {
        public string PackageSourceName { get; set; } = "Lueben.Core";

        public string SearchTerm { get; set; } = "Lueben.";

        public bool ShowOnlyUpgradableLinks { get; set; } = false;

        public bool ShowOnlySearchTermNodes { get; set; } = false;
    }
}
