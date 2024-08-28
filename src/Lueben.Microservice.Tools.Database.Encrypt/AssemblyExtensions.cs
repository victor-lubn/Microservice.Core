using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lueben.Microservice.Tools.Database.Encrypt
{
    public static class AssemblyExtensions
    {
        public static List<Type> FindContextTypes(this Assembly assembly)
        {
            var contexts = new List<Type>();

            var contextFactories = assembly.GetConstructableTypes()
                .Where(t => typeof(IDesignTimeDbContextFactory<DbContext>).IsAssignableFrom(t));

            foreach (var factory in contextFactories)
            {
                var manufacturedContexts =
                    from i in factory.ImplementedInterfaces
                    where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>)
                    select i.GenericTypeArguments[0];
                contexts.AddRange(manufacturedContexts);
            }

            return contexts;
        }

        private static IEnumerable<TypeInfo> GetConstructableTypes(this Assembly assembly)
            => assembly.GetLoadableDefinedTypes()
                .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false });

        private static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo!);
            }
        }
    }
}
