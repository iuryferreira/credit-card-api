using Microsoft.EntityFrameworkCore;

namespace CreditCard.Api.Database
{
    public class Context : DbContext
    {
        public Context (DbContextOptions<Context> options) : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Card> Cards { get; set; }

    }
}
