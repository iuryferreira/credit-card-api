using CreditCard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditCard.Api.Database
{
    public class Context : DbContext
    {
        public Context (DbContextOptions<Context> options) : base(options)
        {
        }

        public DbSet<Person> Persons { get; private set; }
        public DbSet<Card> Cards { get; private set; }
    }
}
