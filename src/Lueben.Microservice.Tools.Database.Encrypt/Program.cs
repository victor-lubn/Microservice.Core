using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lueben.Microservice.Database.Encrypt;
using Lueben.Microservice.Tools.Database.Encrypt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lueben.Microservice.Tools.Database.Encrypt
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: tool path_to_dll_with_dbcontext");
                return;
            }

            var assemblyPath = args[0];
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"File '{assemblyPath} doesn't exists.'");
                return;
            }

            var assembly = Assembly.LoadFrom(assemblyPath);
            var types = assembly.FindContextTypes();
            var contextType = types.First();

            var dbContext = CreateContextInMemory(contextType);
            var tables = BuildListOfTables(dbContext);
            WriteResults(tables);
        }

        private static List<Table> BuildListOfTables(DbContext dbContext)
        {
            var encryptedTables = new List<Table>();
            foreach (var entityType in dbContext.Model.GetEntityTypes())
            {
                var columns = entityType.GetDeclaredProperties()
                    .Where(x => Attribute.IsDefined(x.PropertyInfo!, typeof(EncryptedAttribute)))
                    .Select(x =>
                    {
                        var attr = (EncryptedAttribute)x.PropertyInfo!.GetCustomAttributes(typeof(EncryptedAttribute),
                            false)[0];
                        return new Column(x.GetColumnBaseName())
                        {
                            EncryptionType = attr.Type
                        };
                    }).ToList();

                if (columns.Count == 0)
                {
                    continue;
                }

                var table = new Table(entityType.GetTableName()!, columns);
                encryptedTables.Add(table);
            }

            return encryptedTables;
        }

        private static DbContext CreateContextInMemory(Type contextType)
        {
            var factoryType = typeof(InMemoryDbContextFactory<>).MakeGenericType(contextType);

            return (DbContext)typeof(IDesignTimeDbContextFactory<>).MakeGenericType(contextType)
                .GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>.CreateDbContext), new[] { typeof(string[]) })!
                .Invoke(Activator.CreateInstance(factoryType), new object[] { Array.Empty<string>() })!;
        }

        private static void WriteResults(List<Table> tables)
        {
            if (tables.Count <= 0) return;

            var data = new EncryptionData(tables);

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            settings.Converters.Add(new StringEnumConverter
            {
                AllowIntegerValues = false
            });

            var json = JsonConvert.SerializeObject(data, settings);
            Console.WriteLine(json);
        }
    }
}