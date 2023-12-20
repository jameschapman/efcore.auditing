using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfCore.Audit.UnitTests.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace EfCore.Audit.UnitTests;

// Tests to make sure auditing works with Moq.EntityFrameworkCore
public class MoqEfCoreTests
{
    private readonly Mock<ApplicationContext> _mockContext;

    public MoqEfCoreTests()
    {
        _mockContext = new Mock<ApplicationContext>();
    }

    
    [Fact]
    public async Task TestReview_ReviewAssets_PackagePassed()
    {
        var person = new Person() { Id = Guid.NewGuid() };
        _mockContext.Setup(m => m.People).ReturnsDbSet(new List<Person> { person });
        _mockContext.Setup(x => x.GetService<AuditOptions>()).Returns(new AuditOptions
        {
            TestMode = () => true
        });

        var context = _mockContext.Object;

        await context.SaveChangesAsyncWithHistory();
    }
}