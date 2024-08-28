namespace Lueben.Microservice.Tools.Database.Encrypt.Models
{
    internal class Table
    {
        public Table(string name, List<Column> columns)
        {
            Name = name;
            Columns = columns;
        }

        public string Name { get; set; }

        public List<Column> Columns { get; set; }
    }
}