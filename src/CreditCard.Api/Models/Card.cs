using System.ComponentModel.DataAnnotations;

namespace CreditCard.Api
{

    public class Card
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Number { get; set; }
        [Required]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
    }
}
