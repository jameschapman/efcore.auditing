using System;
using System.Threading.Tasks;
using EfCore.Audit.UnitTests.Models;
using Xunit;

namespace EfCore.Audit.UnitTests;

// Tests to make sure auditing works with Moq.EntityFrameworkCore
public class MoqEfCoreTests
{
    [Fact]
    public async Task TestReview_ReviewAssets_PackagePassed()
    { 
        var person = new Person() { Id = Guid.NewGuid() };
        await DatabaseFixture.ExecuteTest(async (context, _) =>
        {
            context.People.Add(person);

            await context.SaveChangesAsyncWithHistory();
        });
    }
}