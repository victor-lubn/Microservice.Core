using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Lueben.Microservice.Options
{
    [ExcludeFromCodeCoverage]
    public static class Helpers
    {
        private const string DefaultGlobalPrefix = "Global";

        public static string GetDefaultGlobalPrefix()
        {
            var assembly = Assembly.GetCallingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var productName = fvi.ProductName ?? DefaultGlobalPrefix;
            return productName;
        }

        public static string GetDefaultApplicationPrefix() => Assembly.GetCallingAssembly().GetName().Name;
    }
}