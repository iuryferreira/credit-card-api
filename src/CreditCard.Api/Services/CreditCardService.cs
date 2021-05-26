using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CreditCard.Api.Database;
using CreditCard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditCard.Api.Services
{
    public class CreditCardService
    {
        private readonly Context _context;

        public CreditCardService (Context context)
        {
            _context = context;
        }

        public async Task<Card> GenerateCard (Person person)
        {
            var card = new Card { Number = GenerateCardNumber(), Person = person };
            await _context.Cards.AddAsync(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<IEnumerable<Card>> GetCardsByEmail (string email)
        {
            return await _context.Cards
                .Include(c => c.Person)
                .Where(c => c.Person.Email == email)
                .ToListAsync();
        }

        private static string GenerateCardNumber ()
        {
            const string numbersAllowed = "0123456789";
            var random = new Random();
            var cardNumber = "";
            while (cardNumber.Length < 16)
            {
                cardNumber += numbersAllowed[random.Next(numbersAllowed.Length)];
            }

            return cardNumber;
        }
    }
}
