using System.Collections.Generic;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;

namespace Lueben.Microservice.Database.Encrypt
{
    public static class AzureKeyVaultProviderHelper
    {
        public static void InitializeAzureKeyVaultProvider(TokenCredential tokenCredential = null)
        {
            var credential = tokenCredential ?? new ManagedIdentityCredential();
            var provider = new SqlColumnEncryptionAzureKeyVaultProvider(credential);

            var providers = new Dictionary<string, SqlColumnEncryptionKeyStoreProvider> { { SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, provider } };

            SqlConnection.RegisterColumnEncryptionKeyStoreProviders(providers);
        }
    }
}