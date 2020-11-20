using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EfCore.Audit.UnitTests
{
    public static class DatabaseFixture
    {
        public static async Task ExecuteTest(Func<ApplicationContext, AuditOptions, Task> action)
        {
            var auditOptions = new AuditOptions
            {
                CurrentDateTime = () => new DateTime(2000, 1, 1, 1, 0, 0),
                TransactionId = () => "7ae1f103-554b-4891-bc1e-4051ccc718b8",
                User = () => "TestUser",
                Client = () => "TestApp"
            };

            var serviceProvider = new ServiceCollection()
                .RegisterEntityFrameworkAuditing(ServiceLifetime.Singleton, options =>
                {
                    options.CurrentDateTime = auditOptions.CurrentDateTime;
                    options.TransactionId = auditOptions.TransactionId;
                    options.User = auditOptions.User;
                    options.Client = auditOptions.Client;
                })
                .AddDbContext<ApplicationContext>(options => { options.UseSqlite("DataSource=:memory:"); })
                .BuildServiceProvider();

            var context = serviceProvider.GetService<ApplicationContext>();
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            await action(context, auditOptions);

            context.Database.EnsureDeleted();
            context.Database.CloseConnection();
        }
    }
}