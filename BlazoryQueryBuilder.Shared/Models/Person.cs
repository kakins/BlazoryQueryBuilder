using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazoryQueryBuilder.Shared.Models
{
    public class Person
    {
        [Key]
        public string PersonId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int NumberOfChildren { get; set; }
        public bool IsAlive { get; set; }
        public DateTime Created { get; set; }
        public virtual List<Address> Addresses { get; set; }
    }
}