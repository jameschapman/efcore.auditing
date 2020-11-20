using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EfCore.Audit
{
    public sealed class AuditOptions
    {
        public Func<string> Client { get; set; } = () => string.Empty;
        public Func<string> User { get; set; } = () => string.Empty;
        public Func<DateTime> CurrentDateTime { get; set; } = () => DateTime.UtcNow;
        public Func<string> TransactionId { get; set; } = () => Guid.NewGuid().ToString();

        // Test mode ignores auditing and calls the standard SaveChanges method
        public Func<bool> TestMode { get; set; } = () => false;

        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}