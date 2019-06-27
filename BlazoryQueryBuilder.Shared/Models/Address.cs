using System.ComponentModel.DataAnnotations;

namespace BlazoryQueryBuilder.Shared.Models
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }
        public string PersonId { get; set; }
        public Person Person { get; set; }
    }
}