using System;
using System.Diagnostics.CodeAnalysis;

namespace Lueben.Microservice.Database.Encrypt
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property)]
    public class EncryptedAttribute : Attribute
    {
        public EncryptedAttribute() : this(EncryptionType.Deterministic)
        {
        }

        public EncryptedAttribute(EncryptionType encryptionType)
        {
            Type = encryptionType;
        }

        public EncryptionType Type { get; set; }
    }
}