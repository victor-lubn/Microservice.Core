namespace Lueben.Microservice.Tools.Database.Encrypt.Models
{
    internal class EncryptionData
    {
        public EncryptionData(List<Table> tables)
        {
            Tables = tables;
        }

        public List<Table> Tables { get; set; }
    }
}