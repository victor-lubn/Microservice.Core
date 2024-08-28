using Lueben.Microservice.Database.Encrypt;

namespace Lueben.Microservice.Tools.Database.Encrypt.Models
{
    internal class Column
    {
        public Column(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public EncryptionType EncryptionType { get; set; }
    }
}