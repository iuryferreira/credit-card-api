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
