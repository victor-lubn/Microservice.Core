using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Lueben.Microservice.AzureTableRepository
{
    public static class Helpers
    {
        public static string GetTableName<T>(AzureTableRepositoryOptions tableOptions)
        {
            if (!string.IsNullOrEmpty(tableOptions?.TableName) && !Regex.IsMatch(tableOptions.TableName, Constants.TableNameRegex))
            {
                throw new InvalidOperationException($"Table name '{tableOptions.TableName}' doesn't match regex constraint '{Constants.TableNameRegex}'");
            }

            return tableOptions?.TableName ?? typeof(T).Name;
        }

        public static string GetTableServiceClientName(AzureTableRepositoryOptions tableOptions)
        {
            return GetTableServiceClientName(tableOptions?.Connection);
        }

        public static string GetTableServiceClientName(string connectionName)
        {
            return connectionName ?? Constants.DefaultTableServiceClientName;
        }

        public static string GetConnectionStringWithFallBack(this IConfiguration configuration, string connectionName)
        {
            var connectionString = connectionName == null ? configuration.GetValue<string>(Constants.DefaultConnectionName) : configuration.GetConnectionString(connectionName);
            return connectionString;
        }
    }
}