using System;
using System.Collections.Generic;

namespace EfCore.Audit.UnitTests.Models
{
    public class Person
    {
        public Person()
        {
            ImportantDates = new List<ImportantDate>();
        }

        public Guid Id { get; set; }

        public string? Name { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public IEnumerable<ImportantDate> ImportantDates { get; set; }
    }
}