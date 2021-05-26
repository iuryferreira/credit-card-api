using System.Threading.Tasks;
using CreditCard.Api.Database;
using CreditCard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditCard.Api.Services
{
    public class PersonService
    {
        private readonly Context _context;

        public PersonService (Context context)
        {
            _context = context;
        }

        public async Task<Person> CreatePersonIfNotExists (Person person)
        {
            var exists = await _context.Persons.FirstOrDefaultAsync(p => p.Email == person.Email);
            if (exists != null)
            {
                return person;
            }

            await _context.Persons.AddAsync(person);
            await _context.SaveChangesAsync();
            return person;
        }
    }
}
