using System.IO;
using System.Threading.Tasks;
using EfCore.Audit.UnitTests.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace EfCore.Audit.UnitTests
{
    public class JsonFormattingTests
    {
        [Fact]
        public Task DefaultJsonSerializerSettings_ShouldIgnoreNullValues()
        {
            // ARRANGE
            var person = new Person {Name = "John"};

            // ACT
            var json = JsonConvert.SerializeObject(person, new AuditOptions().JsonSerializerSettings);
            var jObject = JObject.Load(new JsonTextReader(new StringReader(json)));

            // Assert
            jObject.ContainsKey("DateOfBirth").ShouldBeFalse();

            return Task.CompletedTask;
        }
    }
}