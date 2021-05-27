using System.Collections.Generic;
using System.Threading.Tasks;
using CreditCard.Api.Models;
using CreditCard.Api.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CreditCard.Api.Controllers
{
    [ApiController]
    [Route("/creditcards")]
    [Produces("application/json")]
    public class CreditCardController : ControllerBase
    {
        private readonly PersonService _personService;
        private readonly CreditCardService _creditCardService;


        public CreditCardController (PersonService personService, CreditCardService creditCardService)
        {
            _personService = personService;
            _creditCardService = creditCardService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Card>>> List ([FromQuery] string email)
        {

            return string.IsNullOrEmpty(email) ?
            BadRequest(new { error = "O email precisa ser passado como parâmetro." }) :
            Ok(await _creditCardService.GetCardsByEmail(email));
        }

        [HttpPost]
        public async Task<ActionResult<Card>> Store ([FromBody] Person data)
        {
            if (string.IsNullOrEmpty(data.Email))
            {
                return BadRequest(new { error = "insira um email." });
            }

            var person = await _personService.CreatePersonIfNotExists(data);
            var card = await _creditCardService.GenerateCard(person);
            return Created($"{Request.GetDisplayUrl()}/{card.Id}", card);
        }
    }
}
