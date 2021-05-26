using System.ComponentModel.DataAnnotations;

namespace CreditCard.Api.Models
{
    public class Person
    {
        [Key] public int Id { get; init; }
        [Required] public string Email { get; set; }
    }
}
