# AGENTS.md

# E-Commerce SaaS — Regras de Arquitetura e Desenvolvimento

Este documento define as regras obrigatórias que qualquer agente de IA deve seguir ao analisar, modificar, criar ou refatorar código neste repositório.

Estas instruções têm prioridade sobre decisões autônomas do agente relacionadas à organização do código.

> IMPORTANTE:
> Antes de implementar qualquer alteração, analise a solução existente, seus projetos, dependências, padrões, abstrações, nomenclaturas e implementações já utilizadas.
>
> NÃO invente uma nova arquitetura quando o projeto já possuir um padrão estabelecido.

---

# 1. Arquitetura

O projeto utiliza:

- Domain-Driven Design (DDD)
- Clean Architecture
- CQRS
- SOLID
- Dependency Injection
- Repository Pattern
- Value Objects
- Rich Domain Model
- Testes unitários
- Testes de regras de negócio
- Testes de arquitetura

A arquitetura deve manter separação clara entre:

```text
API / Presentation
        |
        v
Application
        |
        v
Domain

Infrastructure
        |
        +------> Application
        |
        +------> Domain
```

O **Domain é o núcleo da aplicação**.

As dependências devem apontar para dentro.

---

# 2. Regra de Dependência

Esta regra é OBRIGATÓRIA.

## Permitido

```text
API -> Application
API -> Infrastructure (somente composição/DI quando necessário)

Infrastructure -> Application
Infrastructure -> Domain

Application -> Domain

Domain -> NENHUMA camada externa
```

## Proibido

```text
Domain -> Infrastructure
Domain -> Application
Domain -> API

Application -> Infrastructure
Application -> API

Infrastructure -> API
```

O agente NÃO deve resolver problemas de referência simplesmente adicionando dependências entre projetos.

Antes de adicionar qualquer `ProjectReference`, verifique se isso viola a Clean Architecture.

---

# 3. Domain

A camada `Domain` representa o negócio.

É nela que devem existir as regras de negócio que pertencem às entidades e conceitos do domínio.

Exemplos:

```text
Entities
ValueObjects
Enums
Domain Exceptions
Domain Services
Domain Events
Business Rules
Interfaces/Contracts pertencentes ao domínio
```

Exemplo conceitual:

```text
Domain/
├── Entities/
│   ├── Customer.cs
│   ├── Product.cs
│   ├── Cart.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Payment.cs
│
├── ValueObjects/
│   ├── Email.cs
│   ├── Password.cs
│   ├── TaxId.cs
│   ├── BusinessTaxId.cs
│   ├── Money.cs
│   ├── Quantity.cs
│   ├── SKU.cs
│   ├── PersonName.cs
│   ├── Phone.cs
│   ├── ZipCode.cs
│   └── TrackingNumber.cs
│
├── Enums/
├── Events/
├── Exceptions/
└── Interfaces/
```

---

# 4. Rich Domain Model

Evite modelo de domínio anêmico.

Não crie entidades que sejam apenas conjuntos de propriedades públicas.

ERRADO:

```csharp
public class Order
{
    public decimal Total { get; set; }
    public string Status { get; set; }
}
```

Quando existir uma regra de negócio pertencente à entidade, a própria entidade deve protegê-la.

Preferir:

```csharp
order.AddItem(...);
order.RemoveItem(...);
order.Cancel();
order.Confirm();
```

em vez de:

```csharp
order.Items.Add(...);
order.Status = "Cancelled";
```

A entidade deve proteger seus invariantes.

---

# 5. Regras de Negócio

Regras de negócio NÃO devem ficar espalhadas pela aplicação.

É proibido colocar regra de negócio diretamente em:

```text
Controllers
Repositories
DbContext
EF Configurations
Middlewares
Program.cs
Infrastructure Services
```

Se uma regra pertence ao comportamento de uma entidade ou Value Object, ela deve estar no `Domain`.

Se uma regra representa coordenação de um caso de uso, ela deve estar na `Application`.

Antes de implementar uma regra, pergunte:

```text
Isso é uma regra do negócio?

    SIM
     |
     +--> Pertence naturalmente a uma entidade/VO?
     |        |
     |        +--> SIM -> Domain
     |
     +--> É uma orquestração de caso de uso?
              |
              +--> SIM -> Application
```

---

# 6. Value Objects

Utilize Value Objects para conceitos que possuem regras próprias.

Exemplos já esperados no projeto:

```text
Email
Password
TaxId
BusinessTaxId
Money
Quantity
SKU
AddressType
CustomerType
OrderStatus
PaymentMethod
PaymentStatus
PersonName
Phone
ZipCode
TrackingNumber
VisitorToken
```

Evite Primitive Obsession.

Por exemplo, não espalhe:

```csharp
string email
string cpf
decimal money
string zipCode
```

quando já existir um Value Object correspondente.

Não recrie validações que já estejam implementadas dentro dos Value Objects.

---

# 7. Application

A camada `Application` representa os casos de uso do sistema.

Responsabilidades:

```text
Commands
Queries
Handlers
Use Cases
DTOs
Validators
Mappings
Application Services
Interfaces necessárias aos casos de uso
```

Exemplo:

```text
Application/
├── Customers/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── DTOs/
│   └── Validators/
│
├── Carts/
├── Orders/
├── Payments/
└── Products/
```

A Application deve ORQUESTRAR o fluxo.

Exemplo:

```text
Request
   |
   v
Command
   |
   v
Handler
   |
   +--> Domain
   |
   +--> Repository Contract
   |
   v
Response
```

A Application NÃO deve conhecer detalhes técnicos de persistência.

---

# 8. CQRS

Utilize separação entre Commands e Queries.

## Command

Representa alteração de estado.

Exemplos:

```text
CreateCustomerCommand
UpdateCustomerCommand
CreateOrderCommand
AddItemToCartCommand
RemoveItemFromCartCommand
CreatePaymentCommand
```

## Query

Representa leitura.

Exemplos:

```text
GetCustomerByIdQuery
GetCartQuery
GetOrderByIdQuery
GetProductsQuery
```

Não misture operações de escrita e leitura sem necessidade.

---

# 9. Handlers

Handlers representam a execução/orquestração dos casos de uso.

Um Handler pode:

```text
validar entrada
consultar contratos
carregar entidades
executar operações do Domain
persistir alterações
publicar eventos
retornar resultado
```

Um Handler NÃO deve se transformar em uma classe gigante contendo toda a lógica de negócio.

Exemplo desejado:

```csharp
var cart = await repository.GetByIdAsync(...);

cart.AddItem(product, quantity);

await repository.UpdateAsync(cart);
```

Preferir isso a colocar toda a regra de `AddItem` dentro do Handler.

---

# 10. Infrastructure

Infrastructure contém detalhes técnicos.

Exemplos:

```text
Persistence
Repositories
Entity Framework
DbContext
Configurations
External Services
HTTP Clients
Messaging
Caching
File Storage
Authentication implementation
Payment providers
Email providers
```

Estrutura conceitual:

```text
Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs
│   ├── Configurations/
│   └── Repositories/
│
├── Integrations/
├── Services/
└── DependencyInjection/
```

Infrastructure IMPLEMENTA contratos definidos pelas camadas internas.

---

# 11. Repository Pattern

Repositories concretos devem ficar na `Infrastructure`.

Exemplo:

```text
Domain/Application
        |
        v
IOrderRepository
        ^
        |
Infrastructure
        |
OrderRepository
```

Controllers NÃO podem acessar Repository concreto.

ERRADO:

```csharp
public OrdersController(OrderRepository repository)
```

CORRETO:

```text
Controller
    |
    v
Command / Query
    |
    v
Handler
    |
    v
Repository Contract
    |
    v
Repository Implementation
```

---

# 12. CRUD / Repository Contracts

Antes de criar um novo Repository ou contrato CRUD:

1. Procure abstrações existentes.
2. Analise os repositories atuais.
3. Analise interfaces existentes.
4. Reutilize o padrão do projeto.
5. Não crie um segundo padrão concorrente.

Se o projeto possuir contratos CRUD/base repository, utilize-os quando fizer sentido.

Não crie `GenericRepository<T>` automaticamente apenas para reduzir código.

Abstrações devem existir porque fazem sentido arquiteturalmente, não apenas para diminuir quantidade de linhas.

---

# 13. Validações

Existem diferentes níveis de validação.

## Domain Validation

Protege invariantes do negócio.

Exemplo:

```text
quantidade inválida
estado inválido de pedido
email inválido
valor monetário inválido
transição de status inválida
```

Deve ficar no Domain.

## Application Validation

Valida a entrada do caso de uso.

Exemplo:

```text
campo obrigatório
formato de request
limites de entrada
combinação de parâmetros
```

Pode utilizar Validators.

## Infrastructure Validation

Somente validações técnicas.

Exemplo:

```text
configuração externa
conectividade
serialização
resposta de fornecedor
```

Infrastructure NÃO deve decidir regra de negócio.

---

# 14. API / Presentation

Controllers devem ser FINOS.

Responsabilidades:

```text
receber HTTP Request
obter informações HTTP necessárias
criar/enviar Command ou Query
retornar HTTP Response
```

Um Controller NÃO deve:

```text
implementar regra de negócio
acessar DbContext diretamente
executar SQL
instanciar Repository
conter lógica de persistência
implementar cálculos do domínio
conter fluxo complexo de negócio
```

Exemplo desejado:

```csharp
[HttpPost]
public async Task<IActionResult> Create(
    CreateOrderRequest request,
    CancellationToken cancellationToken)
{
    var command = new CreateOrderCommand(...);

    var result = await mediator.Send(command, cancellationToken);

    return Ok(result);
}
```

---

# 15. DbContext

`AppDbContext` pertence à Infrastructure.

É PROIBIDO:

```text
Controller -> AppDbContext
Domain -> AppDbContext
Application -> AppDbContext concreto
```

O DbContext não deve conter regras de negócio.

Ele é responsável por persistência e mapeamento.

---

# 16. Multi-Tenancy

O sistema é Multi-Tenant.

O Tenant deve ser respeitado em todas as operações aplicáveis.

O cabeçalho esperado é:

```http
X-Tenant-ID
```

Nunca execute uma consulta de entidade multi-tenant ignorando o Tenant quando a regra exigir isolamento.

Uma operação de um Tenant não pode acessar dados pertencentes a outro Tenant.

Nunca remova filtros de Tenant apenas para "fazer funcionar".

---

# 17. Autenticação

Autenticação utiliza JWT.

Formato:

```http
Authorization: Bearer <Token_JWT>
```

Visitantes podem utilizar sessão baseada em:

```text
SessionToken / VisitorToken
```

Não enfraqueça autenticação ou autorização para resolver testes ou erros.

---

# 18. Guest / Session

Visitantes devem possuir identificador de sessão.

Exemplo:

```text
Guest
   |
   v
SessionToken (UUID)
   |
   v
Cart
```

O carrinho pode ser identificado por:

```text
CustomerId
```

ou:

```text
SessionToken
```

dependendo do tipo de usuário.

Não quebre o fluxo Guest ao implementar autenticação de clientes cadastrados.

---

# 19. Soft Delete

O projeto utiliza Soft Delete.

Operações de exclusão devem preferencialmente alterar:

```text
IsActive = false
```

quando essa for a regra da entidade.

Não transforme Soft Delete em DELETE físico sem uma exigência explícita.

---

# 20. Banco de Dados

O banco deve respeitar a modelagem do domínio.

Entidades principais incluem conceitos como:

```text
Tenants
Branches
Customers
Individuals
Companies
Addresses
Contacts
Products
Carts
CartItems
Orders
OrderItems
Payments
CustomersLocation
Users
```

Antes de alterar estrutura de banco:

1. analise a modelagem atual;
2. analise entidades;
3. analise relacionamentos;
4. analise migrations/scripts existentes;
5. verifique TenantId;
6. verifique Soft Delete;
7. verifique índices;
8. verifique constraints.

Não altere banco para contornar uma modelagem incorreta no código.

---

# 21. SOLID

Toda implementação deve respeitar SOLID.

Especialmente:

## SRP

Uma classe deve possuir responsabilidade clara.

## OCP

Evite modificar grandes estruturas condicionais quando uma extensão apropriada resolver o problema.

## LSP

Implementações devem respeitar seus contratos.

## ISP

Evite interfaces gigantes.

## DIP

Camadas internas dependem de abstrações.

Detalhes externos implementam essas abstrações.

---

# 22. Dependency Injection

Utilize Dependency Injection.

Evite:

```csharp
new OrderRepository(...)
new PaymentService(...)
new HttpClient(...)
```

espalhados pelo código.

Dependências devem ser registradas na composição da aplicação.

Não utilize Service Locator.

---

# 23. Código Existente Tem Prioridade

Antes de criar uma implementação nova, pesquise o repositório.

Procure:

```text
implementações semelhantes
interfaces existentes
base classes
Value Objects
Validators
Handlers
Repositories
Configurations
Extensions
Factories
Services
testes existentes
```

Se existir um padrão estabelecido, siga esse padrão.

NÃO duplique código ou abstrações existentes.

---

# 24. Mudanças Mínimas

Ao receber uma tarefa:

> Faça a menor alteração possível capaz de resolver corretamente o problema.

Não faça refatorações gigantes sem necessidade.

Não altere:

```text
nomes
pastas
namespaces
interfaces
contratos
arquitetura
bibliotecas
frameworks
```

quando isso não fizer parte da tarefa.

---

# 25. Não Inventar Requisitos

Não crie funcionalidades que não foram solicitadas.

Se houver ambiguidade relevante:

1. analise código existente;
2. analise documentação;
3. analise testes;
4. procure implementação equivalente;
5. somente então tome uma decisão.

Quando uma decisão puder modificar regra de negócio significativamente, não assuma silenciosamente.

---

# 26. Testes Unitários

Toda nova regra de negócio deve possuir testes.

Os testes devem cobrir:

```text
Happy Path
validações
erros esperados
edge cases
regras de negócio
transições de estado
```

Priorizar testes determinísticos.

Evitar testes dependentes de:

```text
internet
serviços externos reais
horário real
dados aleatórios não controlados
ambiente externo
```

---

# 27. Testes do Domain

Testes do Domain devem testar comportamento do negócio.

Exemplo:

```text
Adicionar produto válido ao carrinho
Não permitir quantidade inválida
Não permitir pagamento inválido
Não permitir transição inválida de OrderStatus
Calcular valores corretamente
```

Não teste apenas getters e setters para aumentar artificialmente cobertura.

---

# 28. Testes dos Handlers

Handlers devem possuir testes de casos de uso.

Mockar dependências externas quando apropriado.

Exemplo:

```text
Repository
External Services
Message Bus
Payment Gateway
Clock
```

O teste deve validar o comportamento do caso de uso, não detalhes internos irrelevantes.

---

# 29. BDD / Specifications

Testes BDD devem priorizar comportamentos e regras do negócio.

Formato conceitual:

```gherkin
Given
When
Then
```

Exemplo:

```text
Given um carrinho ativo
And um produto disponível
When o cliente adicionar o produto
Then o item deve existir no carrinho
And o total deve ser atualizado
```

BDD pode testar outros componentes quando existir comportamento relevante, mas não deve ser usado apenas para aumentar cobertura de código técnico.

---

# 30. Cobertura de Testes

Cobertura é uma métrica auxiliar.

Quando a tarefa solicitar 100% de cobertura em determinada classe, busque cobrir todos os caminhos relevantes.

Porém:

> 100% de cobertura não substitui testes de qualidade.

Não crie testes sem assert relevante apenas para aumentar o SonarCloud.

---

# 31. Testes de Arquitetura

O projeto deve possuir testes capazes de detectar violações arquiteturais.

Exemplos de regras:

```text
Domain não referencia Infrastructure
Domain não referencia Application
Domain não referencia API

Application não referencia Infrastructure
Application não referencia API

Infrastructure não referencia API
```

Também é desejável verificar:

```text
Controllers não acessam DbContext
Controllers não acessam Repository concreto
Domain não depende de Entity Framework
Domain não depende de ASP.NET
```

Esses testes devem falhar caso uma alteração futura viole a arquitetura.

---

# 32. Proibição de Correções Arquiteturalmente Incorretas

É PROIBIDO corrigir erro de compilação fazendo algo como:

```text
Application precisa de classe da Infrastructure
        |
        v
Adicionar referência Application -> Infrastructure
```

Isso é uma violação.

A solução correta normalmente será mover ou abstrair o contrato.

Exemplo:

```text
Application
    |
    v
IPaymentGateway
    ^
    |
Infrastructure
    |
AsaasPaymentGateway
```

---

# 33. Integrações Externas

Integrações externas são detalhes de Infrastructure.

Exemplos:

```text
Payment Gateway
Email
Storage
HTTP APIs
Message Brokers
External Authentication
```

O Domain não deve conhecer SDKs ou DTOs de fornecedores externos.

Utilize Adapter/Anti-Corruption Layer quando necessário.

---

# 34. Logs

Logs devem fornecer informações úteis para diagnóstico.

Não registrar:

```text
password
JWT completo
tokens
segredos
dados sensíveis desnecessários
```

Não utilizar logs como substituto para tratamento correto de erros.

---

# 35