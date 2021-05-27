## Banco de Dados

Antes de partir para implementação dos endpoints da **API REST**, devemos criar os nossos modelos de dados, que são a representação dos dados no C# e que serão utilizados pelo **Entity Framework**. 

Baseando-se nas nossas informações, temos que:

- A API deverá gerar números aleatórios para o pedido de novo cartão;
- Cada cartão gerado deve estar associado a um email para identificar a pessoa que está utilizando;

### Modelos

Sendo assim, iniciamos criando nossa pasta de modelos (*Models*) e adicionamos as seguintes classes a ela:

```csharp
//Diretório: src/CreditCard.Api/Models/Person.cs

using System.ComponentModel.DataAnnotations;

namespace CreditCard.Api.Models
{
    public class Person
    {
        [Key] public int Id { get; init; }
        [Required] public string Email { get; set; }
    }
}
```

```csharp
//Diretório: src/CreditCard.Api/Models/Card.cs

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
```
Os campos `Id` tem como objetivo serem os identificadores únicos de cada Pessoa e Cartão. Os campos PersonId e Person na classe `Card` se referem ao relacionamento estabelecido entre eles, que será refletido no banco de dados devido ao mapeamento do Entity Framework.

### Contexto de Banco de Dados

Assim que os nossos modelos estiverem criados, devemos criar o nosso contexto de banco de dados, que utilizaremos para realizar queries. 

O Entity Framework nos fornece a classe `DbContext`, que representa uma combinação de padrões de projeto para manipulação de dados a fim de facilitar este trabalho, então toda a classe que herda dela pode utilizar o seus recursos. 

Para utilizá-lo com os nossos modelos devemos criar a pasta *Database* e dentro dela a classe abaixo:

```csharp
//Diretório: src/CreditCard.Api/Database/Context.cs

using CreditCard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditCard.Api.Database
{
    public class Context : DbContext
    {
        public Context (DbContextOptions<Context> options) : base(options) { }

        public DbSet<Person> Persons { get; private set; }
        public DbSet<Card> Cards { get; private set; }
    }
}
```

Após isso, devemos ir até a classe `Startup` e adicionar nosso contexto como um serviço. Sendo assim a instância do serviço vai ser gerenciada de forma automática, nos permitindo utilizar injeção de dependência e injetar o nosso contexto em qualquer classe passando-o através do construtor, como mostrado abaixo:

```csharp
//Diretório: src/CreditCard.Api/Startup.cs

using CreditCard.Api.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CreditCard.Api
{
    public class Startup
    {
        //...

        public void ConfigureServices (IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<Context>(options => options.UseInMemoryDatabase("Database"));
        }

       // ...
    }
}
```
> A classe `Startup` tem vários outros elementos que vem por padrão e que estão representados por esses "...", mas que estão sendo ignorados no exemplo a fim de mostrar somente as alterações realizadas. A classe completa pode ser vista clicando [aqui]().

## Serviços

Agora que temos os nossos modelos e o contexto de banco de dados criados, devemos dar início a implementação dos serviços. Os serviços tem como objetivo centralizar a lógica das funcionalidades da API, melhorando a divisão de responsabilidades de cada classe, facilitando o entendimento e a manutenção do código. Eles utilizarão o `Context`  e serão utilizados pelos `Controllers` que implementam os endpoints.

Para podermos cadastrar pessoas, devemos criar a classe `PersonService` na pasta de *Services*, que ficará responsável por isso, como mostrado abaixo:

```csharp

//Diretório: src/CreditCard.Api/Services/PersonService.cs

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

```


O método `CreatePersonIfNotExists` recebe o objeto `Person` e verifica utilizando o contexto se aquela pessoa já existe no banco de dados, caso exista ele a retorna, caso não, é criada e retornada.

Também criaremos a classe `CreditCardService` que ficará responsável pela geração de um novo cartão de crédito virtual:


```csharp

//Diretório: src/CreditCard.Api/Services/CreditCardService.cs

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

```

Esta classe é composta por 3 métodos com 3 responsabilidades distintas, sendo elas:

- `GenerateCardNumber`: gera o número aleatório em formato de `string` com 16 caracteres, que é o número médio de dígitos que um cartão contém.
- `GenerateCard`: cria um novo cartão para a pessoa trazida como parâmetro e na atribuição do número do cartão faz a chamada ao método `GenerateCardNumber` para obter o número aleatóriamente. Logo em seguida, utiliza o contexto para inserir o cartão no banco de dados e retorna o cartão.
- `GetCardsByEmail`: retorna todos os cartões cadastrados para um endereço de email específico passado como parâmetro.


Após isso, devemos ir até a classe `Startup` e adicionar nossas classes à coleção de serviços. Assim como o contexto, nossos serviços serão gerenciados de forma automática, nos permitindo utilizar injeção de dependência e injetá-los em qualquer classe passando-os através do construtor. Veja como ficará nossa classe `Startup`:

```csharp
//Diretório: src/CreditCard.Api/Startup.cs

using CreditCard.Api.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CreditCard.Api
{
    public class Startup
    {
        //...

        public void ConfigureServices (IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<Context>(options => options.UseInMemoryDatabase("Database"));
            services.AddTransient<PersonService>();
            services.AddTransient<CreditCardService>();
        }

       // ...
    }
}
```
> A classe `Startup` tem vários outros elementos que vem por padrão e que estão representados por esses "...", mas que estão sendo ignorados no exemplo a fim de mostrar somente as alterações realizadas. A classe completa pode ser vista clicando [aqui]().


## Endpoints

Por fim, tendo as nossas classes de serviço criadas, devemos implementar o nosso *controller*, que ficará reponsável por definir os endpoints, os parâmetros a serem recebidos, as rotas, entre outras responsabilidades. Para isso criaremos o `CreditCardController` na pasta *Controllers*.

Nosso classe deve ser da seguinte forma:

```csharp

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

```

O `CreditCardController` está definido na rota `/creditcards` e recebe por meio da injeção de dependência uma instância do `PersonService` e do `CreditCardService` que serão utilizados nos endpoints. Os decoradores em cima dos métodos indicam quais verbos deverão ser utilizados para acessar aquele endpoint.

Cada endpoint tem um método com responsabilidades distintas, sendo que:

- `Store`: recebe um objeto *JSON* (no formato da classe `Person`) no corpo da requisição contendo o email e retorna o cartão cadastrado com um código de status 201. Caso o email não seja informado, retorna um objeto com uma mensagem de erro e código de status 400;
- `List`: recebe o email como uma `string` através da *query string* da requisição e retorna a lista de cartões baseado no email informado, com um código de status 200. Caso o email não seja informado, retorna um objeto com uma mensagem de erro e código de status 400; 


Sendo assim, basta executarmos o servidor para termos acesso a API. Em ambiente de desenvolvimento podemos executar utilzando o comando `dotnet run --project src/CreditCard.Api`.

## Conclusão

Esses foram os passos para implementação da API baseada no desafio recebido. Caso queira testá-la pode acessar o exemplo que está hospedado no **Heroku** clicando [aqui](). 

Todo o código-fonte está neste repositório e pode ser acessado a qualquer momento.





