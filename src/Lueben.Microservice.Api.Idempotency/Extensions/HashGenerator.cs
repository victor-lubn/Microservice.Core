using System;
using System.Security.Cryptography;

namespace Lueben.Microservice.Api.Idempotency.Extensions
{
    public static class HashGenerator
    {
        public static string CreateShaHash(byte[] phraseAsByte)
        {
            using var hashTool = SHA512.Create();
            var encryptedBytes = hashTool.ComputeHash(phraseAsByte);
            hashTool.Clear();

            return Convert.ToBase64String(encryptedBytes);
        }
    }
}
