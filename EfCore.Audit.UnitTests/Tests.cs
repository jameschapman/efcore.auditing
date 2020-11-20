using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EfCore.Audit.Party.Domain;
using EfCore.Audit.UnitTests.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace EfCore.Audit.UnitTests
{
    public class Tests
    {
        private readonly ITestOutputHelper _output;

        public Tests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Calling_SaveChangesAsyncWithHistory_ShouldCallOnSaveCallback()
        {
            // ARRANGE
            var birthday = new ImportantDate {Description = "Birthday", Date = new DateTime(2000, 1, 1)};
            var person = new Person {Name = "John", ImportantDates = new List<ImportantDate> {birthday}};

            // ACT
            var success = false;
            await DatabaseFixture.ExecuteTest(async (context, time) =>
            {
                context.People.Add(person);

                await context.SaveChangesAsyncWithHistory((c, list) =>
                {
                    c.ShouldBe(context);

                    list.Count.ShouldBe(2);

                    success = true;
                });
            });

            // ASSERT
            success.ShouldBe(true);
        }


        [Fact]
        public async Task Calling_SaveChangesAsyncWithHistory_ShouldPersistAssociatedData()
        {
            // ARRANGE
            var birthday = new ImportantDate {Description = "Birthday", Date = new DateTime(2000, 1, 1)};
            var person = new Person {Name = "John", ImportantDates = new List<ImportantDate> {birthday}};

            await DatabaseFixture.ExecuteTest(async (context, options) =>
            {
                context.People.Add(person);

                // ACT
                await context.SaveChangesAsyncWithHistory();

                // ASSERT
                var auditItem = context.Set<Audit>().Where(x => true).ToList().ToDictionary(x => x.TableName);

                // Validate the Person
                auditItem["People"].Action.ShouldBe("Added");
                auditItem["People"].CreatedBy.ShouldBe("TestUser");
                auditItem["People"].Client.ShouldBe("TestApp");
                auditItem["People"].CreateDate.ShouldBe(options.CurrentDateTime());

                var changes =
                    JsonConvert.DeserializeObject<Dictionary<string, AuditEntry.Change>>(auditItem["People"].Data);
                changes["name"].NewValue.ShouldBe("John");
                changes["name"].OldValue.ShouldBe(null);

                var key = JObject.Parse(auditItem["People"].RowId);
                var personId = Guid.Parse(key["id"].ToString());
                personId.ShouldNotBe(Guid.Empty);

                // Validate the important date
                auditItem["ImportantDates"].Action.ShouldBe("Added");
                auditItem["ImportantDates"].CreatedBy.ShouldBe("TestUser");
                auditItem["ImportantDates"].Client.ShouldBe("TestApp");
                auditItem["ImportantDates"].CreateDate.ShouldBe(options.CurrentDateTime());

                changes = JsonConvert.DeserializeObject<Dictionary<string, AuditEntry.Change>>(
                    auditItem["ImportantDates"]
                        .Data);
                changes["personId"].NewValue.ShouldBe(personId.ToString());
                DateTime.Parse(changes["date"].NewValue.ToString()).ShouldBe(birthday.Date);
                changes["date"].OldValue.ShouldBeNull();
                changes["description"].NewValue.ShouldBe("Birthday");
                changes["description"].OldValue.ShouldBeNull();

                key = JObject.Parse(auditItem["ImportantDates"].RowId);
                Guid.Parse(key["id"].ToString()).ShouldNotBe(Guid.Empty);
            });
        }


        [Fact]
        public async Task Calling_SaveChangesAsyncWithHistory_ShouldPersistChangesToAuditTable()
        {
            // ARRANGE
            await DatabaseFixture.ExecuteTest(async (context, options) =>
            {
                var testEntity = new Person {Name = "John"};
                context.People.Add(testEntity);

                // ACT
                await context.SaveChangesAsyncWithHistory();

                // ASSERT
                var auditItem = await context.Set<Audit>().FirstAsync();

                auditItem.CreatedBy.ShouldBe("TestUser");
                auditItem.TableName.ShouldBe("People");
                auditItem.Action.ShouldBe("Added");
                auditItem.CreateDate.ShouldBe(options.CurrentDateTime());

                var changes = JsonConvert.DeserializeObject<Dictionary<string, AuditEntry.Change>>(auditItem.Data);
                changes["name"].NewValue.ShouldBe("John");
                changes["name"].OldValue.ShouldBe(null);

                var key = JObject.Parse(auditItem.RowId);
                Guid.Parse(key["id"].ToString()).ShouldNotBe(Guid.Empty);
            });
        }

        [Fact]
        public async Task Calling_SaveChangesAsyncWithHistory_ShouldPersistDatabase()
        {
            // ARRANGE
            await DatabaseFixture.ExecuteTest(async (context, options) =>
            {
                var testEntity = new Person {Name = "John"};
                context.People.Add(testEntity);
                await context.SaveChangesAsyncWithHistory();

                // ACT
                await context.SaveChangesAsyncWithHistory();

                // ASSERT
                var testItem = await context.Set<Person>().FirstAsync();

                testItem.Name.ShouldBe("John");
            });
        }

        [Fact]
        public async Task Calling_SaveChangesAsyncWithHistory_ShouldPersistUpdates()
        {
            // ARRANGE
            await DatabaseFixture.ExecuteTest(async (context, options) =>
            {
                var testEntity = new Person {Name = "John"};
                context.People.Add(testEntity);
                await context.SaveChangesAsync();

                // ACT
                var person = context.People.First();
                person.Name = "James";
                context.People.Attach(person);
                await context.SaveChangesAsyncWithHistory();

                // ASSERT
                var auditItem = await context.Set<Audit>().FirstAsync();

                var changes = JsonConvert.DeserializeObject<Dictionary<string, AuditEntry.Change>>(auditItem.Data);
                changes["name"].NewValue.ShouldBe("James");
                changes["name"].OldValue.ShouldBe("John");
            });
        }

        [Fact]
        public async Task Calling_SaveChangesAsyncWithHistory_ShouldStoreTransactionIdAcrossTableChanges()
        {
            // ARRANGE
            var birthday = new ImportantDate {Description = "Birthday", Date = new DateTime(2000, 1, 1)};
            var person = new Person {Name = "John", ImportantDates = new List<ImportantDate> {birthday}};

            await DatabaseFixture.ExecuteTest(async (context, options) =>
            {
                context.People.Add(person);

                // ACT
                await context.SaveChangesAsyncWithHistory();

                // ASSERT
                var auditItem = context.Set<Audit>().Where(x => true).ToList().ToDictionary(x => x.TableName);

                auditItem["People"].TransactionId.ShouldBe(options.TransactionId());
                auditItem["ImportantDates"].TransactionId.ShouldBe(options.TransactionId());
            });
        }

        /// <summary>
        ///     EF change tracking will detect changes even if the value remains the same, e.g. resetting
        ///     the name from John to John will result in EF detecting a change - We should ignore these cases
        ///     where the vale remains the same
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Calling_SaveChangesAsyncWithHistory_ShouldIgnoreEqualUpdates()
        {
            // ARRANGE
            await DatabaseFixture.ExecuteTest(async (context, options) =>
            {
                var testEntity = new Person {Name = "James", DateOfBirth = new DateTime(1970, 1, 1)};
                context.People.Add(testEntity);
                await context.SaveChangesAsync();

                // ACT
                var person = context.People.First();
                context.People.Attach(person);
                person.Name = "James";
                person.DateOfBirth = new DateTime(1970, 1, 1);

                await context.SaveChangesAsyncWithHistory();

                // ASSERT
                var auditItem = context.Set<Audit>().Where(x => true);

                auditItem.Count().ShouldBe(0);
            });
        }

        [Fact(Skip = "Run manually as required")]
        public async Task PerformanceTest()
        {
            await DatabaseFixture.ExecuteTest(async (context, options) =>
            {
                var people = new List<Person>();
                for (var i = 0; i < 1000; i++) people.Add(new Person {Name = i.ToString()});

                // SaveChangesAsync 
                var stopWatch = Stopwatch.StartNew();
                await context.People.AddRangeAsync(people);
                var results = await context.SaveChangesAsync();
                stopWatch.Stop();
                var timespan = stopWatch.Elapsed;

                _output.WriteLine(
                    $"SaveChangesAsync took {timespan.Milliseconds} milliseconds to save {results} records");

                // SaveChangesAsyncWithHistory
                people = new List<Person>();
                for (var i = 0; i < 1000; i++) people.Add(new Person {Name = i.ToString()});

                stopWatch = Stopwatch.StartNew();
                await context.People.AddRangeAsync(people);
                results = await context.SaveChangesAsyncWithHistory();
                stopWatch.Stop();
                timespan = stopWatch.Elapsed;

                _output.WriteLine(
                    $"SaveChangesAsyncWithHistory took {timespan.Milliseconds} milliseconds to save {results} records");

                context.Set<Audit>().Count().ShouldBe(1000);
                context.People.LongCount().ShouldBe(2000);
            });
        }

        [Fact]
        public async Task Calling_SaveChangesAsyncWithHistoryForOwnedEntities_ShouldPersistChangesToAuditTable()
        {
            // ARRANGE
            await DatabaseFixture.ExecuteTest(async (context, options) =>
            {
                var testEntity = new Primary
                    {A = "1", OwnedOne = new OwnedOne {B = "2"}, OwnedTwo = new OwnedTwo {C = "3"}};
                context.Primary.Add(testEntity);

                // ACT
                await context.SaveChangesAsyncWithHistory();

                // ASSERT
                var auditItem = context.Set<Audit>().First();

                auditItem.CreatedBy.ShouldBe("TestUser");
                auditItem.TableName.ShouldBe("Primary");
                auditItem.Action.ShouldBe("Added");
                auditItem.CreateDate.ShouldBe(options.CurrentDateTime());

                var changes = JsonConvert.DeserializeObject<Dictionary<string, AuditEntry.Change>>(auditItem.Data);
                changes["a"].NewValue.ShouldBe("1");
                changes["a"].OldValue.ShouldBe(null);
                changes["ownedOne_B"].NewValue.ShouldBe("2");
                changes["ownedOne_B"].OldValue.ShouldBe(null);
                changes["ownedTwo_C"].NewValue.ShouldBe("3");
                changes["ownedTwo_C"].OldValue.ShouldBe(null);

                var key = JObject.Parse(auditItem.RowId);
                Guid.Parse(key["id"].ToString()).ShouldNotBe(Guid.Empty);
            });
        }
    }
}