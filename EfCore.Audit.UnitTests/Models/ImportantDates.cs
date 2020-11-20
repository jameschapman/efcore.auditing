using System;

namespace EfCore.Audit.UnitTests.Models
{
    public class ImportantDate
    {
        public Guid Id { get; set; }

        public Person Person { get; set; }
        public Guid PersonId { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}