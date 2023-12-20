using EfCore.Audit.UnitTests.Models;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Audit.UnitTests
{
    public class ApplicationContext : DbContext
    {
        // Used by unit tests
        public ApplicationContext()
        {
            
        }
        
        public ApplicationContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Person> People { get; set; }

        public DbSet<ImportantDate> ImportantDates { get; set; }

        public DbSet<Primary> Primary { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.EnableAuditing();
        }
    }
}