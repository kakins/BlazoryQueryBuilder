using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazoryQueryBuilder.Shared.Models
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }
        public string PersonId { get; set; }
        public string City { get; set; }

        public bool IsPrimary { get; set; }

        [ForeignKey(nameof(PersonId))]
        public virtual Person Person { get; set; }

        public virtual List<Utility> Utilities { get; set; }
    }

    public class Utility
    {
        public int UtilityId { get; set; }
        public string Provider { get; set; }
        public string AccountNumber { get; set; }
        public string Type { get; set; }
        public int AddressId { get; set; }

        [ForeignKey(nameof(AddressId))]
        public virtual Address Address { get; set; }
    }
}