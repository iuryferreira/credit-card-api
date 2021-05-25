using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreditCard.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExampleController : ControllerBase
    {
        private readonly ILogger<ExampleController> _logger;

        public ExampleController (ILogger<ExampleController> logger)
        {
            _logger = logger;
        }
    }
}
