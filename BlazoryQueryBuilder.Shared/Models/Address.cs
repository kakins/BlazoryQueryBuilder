using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazoryQueryBuilder.Shared.Models
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }
        public string PersonId { get; set; }

        public bool IsPrimary { get; set; }

        [ForeignKey(nameof(PersonId))]
        public virtual Person Person { get; set; }
    }
}