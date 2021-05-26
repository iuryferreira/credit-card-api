using System.ComponentModel.DataAnnotations;

namespace CreditCard.Api.Models
{
    public class Card
    {
        [Key] public int Id { get; set; }
        [Required] public string Number { get; init; }
        [Required] public int PersonId { get; set; }
        public Person Person { get; init; }
    }
}
