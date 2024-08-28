using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lueben.Microservice.Tools.Database.Encrypt
{
    internal class InMemoryDbContextFactory<T> : IDesignTimeDbContextFactory<T>
        where T : DbContext, new()
    {
        public T CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<T>(new DbContextOptions<T>());
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            return (T)Activator.CreateInstance(typeof(T), args: builder.Options)!;
        }
    }
}
