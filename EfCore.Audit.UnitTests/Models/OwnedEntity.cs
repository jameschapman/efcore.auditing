using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Audit.UnitTests.Models
{
    [Owned]
    public class OwnedOne
    {
        public string B { get; set; }
    }

    [Owned]
    public class OwnedTwo
    {
        public string C { get; set; }
    }

    public class Primary
    {
        [Key] public Guid Id { get; set; }

        public string A { get; set; }

        public OwnedOne OwnedOne { get; set; }
        public OwnedTwo OwnedTwo { get; set; }
    }
}