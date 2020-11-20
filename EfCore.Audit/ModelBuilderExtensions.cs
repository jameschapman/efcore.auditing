using Microsoft.EntityFrameworkCore;

namespace EfCore.Audit
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder EnableAuditing(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Audit>(b =>
            {
                b.Property(c => c.RowId).IsRequired();
                b.Property(c => c.TableName).IsRequired();
            });

            return modelBuilder;
        }
    }
}