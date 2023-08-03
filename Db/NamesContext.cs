using Microsoft.EntityFrameworkCore;

namespace Discarding_2._1.Db
{
    public class NamesContext : DbContext
    {
        public DbSet<Names> Names { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=NamesList.db");
        }
    }
}
