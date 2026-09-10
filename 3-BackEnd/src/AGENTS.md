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

# 35. Tratamento de Erros e Exceptions

Erros devem ser tratados na camada apropriada.

O Domain pode lançar exceções relacionadas a violações de regras ou invariantes do domínio.

Exemplos:

```text
DomainException
BusinessRuleException
InvalidOrderStateException
InvalidQuantityException
```

Não utilize exceptions como fluxo normal da aplicação.

Não exponha diretamente detalhes internos de exceptions para clientes da API.

ERRADO:

```json
{
  "error": "SqlException: Invalid column..."
}
```

A API deve transformar erros internos em respostas HTTP apropriadas e seguras.

---

# 36. Result Pattern

Se o projeto já utilizar `Result`, `Result<T>` ou padrão equivalente, mantenha o padrão existente.

Não introduza um segundo mecanismo de tratamento de resultados sem necessidade.

Exemplo conceitual:

```csharp
Result<Order>
```

pode representar:

```text
Success
Validation Error
Business Error
Not Found
Conflict
```

Antes de criar uma nova implementação de `Result`, procure uma existente no projeto.

---

# 37. HTTP Status Codes

A API deve utilizar códigos HTTP semanticamente corretos.

Exemplos:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity
500 Internal Server Error
```

Não retornar `200 OK` para qualquer situação apenas para simplificar o Controller.

A definição final deve respeitar o padrão já existente no projeto.

---

# 38. Segurança

Segurança não pode ser removida para facilitar uma implementação.

É PROIBIDO:

```text
remover autenticação para fazer endpoint funcionar
ignorar autorização
desabilitar validação de JWT
ignorar TenantId
expor secrets
hardcodar credenciais
registrar senhas em logs
registrar tokens completos
commitar API Keys
```

Credenciais devem utilizar mecanismos de configuração apropriados.

Exemplos:

```text
Environment Variables
User Secrets
Secret Manager
CI/CD Secrets
```

Nunca colocar secrets diretamente no código-fonte.

---

# 39. Async/Await

Operações de I/O devem utilizar `async/await` quando aplicável.

Exemplos:

```text
Database
HTTP
Storage
Messaging
External APIs
```

Evite:

```csharp
.Result
.Wait()
.GetAwaiter().GetResult()
```

quando uma implementação assíncrona for possível.

Propague `CancellationToken` quando o padrão do projeto permitir.

Exemplo:

```csharp
public async Task<Result> Handle(
    CreateOrderCommand command,
    CancellationToken cancellationToken)
```

---

# 40. CancellationToken

Operações assíncronas devem propagar `CancellationToken` pelas camadas quando aplicável.

Fluxo esperado:

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
Repository / Integration
```

Não substituir arbitrariamente:

```csharp
CancellationToken.None
```

quando um token já estiver disponível.

---

# 41. Entity Framework / Persistência

Entity Framework é detalhe de Infrastructure.

Configurações de banco devem permanecer na Infrastructure.

Exemplo:

```text
Infrastructure/
└── Persistence/
    ├── AppDbContext.cs
    └── Configurations/
        ├── CustomerConfiguration.cs
        ├── OrderConfiguration.cs
        └── ProductConfiguration.cs
```

Entidades do Domain não devem depender diretamente de:

```text
DbContext
DbSet
EntityTypeBuilder
Entity Framework attributes específicos
SQL
```

sempre que a arquitetura existente permitir manter essa separação.

---

# 42. Consultas e Performance

Evite problemas conhecidos de acesso a dados.

Verifique especialmente:

```text
N+1 Queries
consultas dentro de loops
carregamento excessivo de dados
Include desnecessário
múltiplas consultas iguais
materialização precoce
falta de paginação
```

Para consultas somente leitura, considere o padrão existente para `AsNoTracking()`.

Não faça otimizações prematuras sem evidência ou necessidade.

---

# 43. Paginação

Endpoints que retornam grandes coleções devem utilizar paginação quando aplicável.

Evite retornar tabelas completas sem necessidade.

Exemplo conceitual:

```text
page
pageSize
totalItems
totalPages
items
```

Siga o contrato de paginação já existente no projeto.

---

# 44. Migrations e Alterações de Banco

Não criar migration automaticamente para qualquer alteração sem verificar primeiro como o projeto gerencia banco de dados.

Antes de alterar schema:

1. analisar entidades;
2. analisar configurações;
3. analisar migrations/scripts existentes;
4. verificar impacto;
5. verificar TenantId;
6. verificar índices;
7. verificar constraints;
8. verificar relacionamentos;
9. verificar dados existentes.

Nunca executar migration em banco remoto sem autorização explícita.

---

# 45. Nomenclatura

Siga os padrões existentes no repositório.

Para C# utilizar, quando compatível com o projeto:

```text
PascalCase -> classes, métodos, propriedades
camelCase  -> parâmetros e variáveis locais
IName      -> interfaces
Async      -> métodos assíncronos quando apropriado
```

Não renomeie componentes existentes sem necessidade.

Prefira nomes que expressem o domínio.

Exemplo:

```text
CreateOrderCommand
CreateOrderHandler
IOrderRepository
OrderRepository
```

em vez de nomes genéricos como:

```text
OrderManager
OrderHelper
OrderUtils
ProcessData
```

---

# 46. Código Morto

Não deixe código comentado, implementações antigas ou arquivos temporários após concluir uma alteração.

Evite:

```csharp
// código antigo
// talvez usar depois
// teste temporário
```

O Git já mantém o histórico.

Também não crie arquivos como:

```text
temp.cs
test2.cs
backup.cs
old.cs
```

sem necessidade.

---

# 47. TODOs

Não utilize `TODO` para esconder implementação incompleta.

Se a tarefa exige determinada funcionalidade, ela deve ser concluída.

Um TODO somente deve ser adicionado quando:

1. fizer sentido;
2. estiver explicitamente fora do escopo;
3. não comprometer a funcionalidade atual.

---

# 48. Comentários

Comentários devem explicar o **porquê**, não simplesmente repetir o código.

Evite:

```csharp
// Adiciona o item
cart.AddItem(item);
```

Comentários são úteis quando explicam:

```text
decisão arquitetural
regra de negócio não óbvia
restrição de fornecedor externo
workaround documentado
motivo de uma implementação específica
```

---

# 49. Não Alterar Contratos Sem Necessidade

Não altere contratos públicos sem analisar impacto.

Isso inclui:

```text
Endpoints
Requests
Responses
DTOs
Interfaces
Events
Database Schema
Message Contracts
```

Antes de alterar um contrato existente, procure todos os consumidores.

Mudanças incompatíveis devem ser evitadas quando não forem explicitamente solicitadas.

---

# 50. Compatibilidade

Ao modificar código existente, preserve compatibilidade sempre que possível.

Não atualize automaticamente:

```text
.NET
NuGet packages
Entity Framework
bibliotecas
Docker images
Node
dependências frontend
```

apenas porque existe uma versão mais nova.

Atualização de dependências deve fazer parte explicitamente da tarefa ou ser necessária para solucionar o problema.

---

# 51. Execução SOMENTE Local

Esta regra é OBRIGATÓRIA.

O agente possui autorização para executar comandos necessários para analisar e validar o projeto **somente no ambiente local de trabalho disponibilizado para a tarefa**.

Pode executar localmente, quando necessário:

```bash
dotnet restore
dotnet build
dotnet test
```

Também pode executar comandos equivalentes de testes, lint, análise estática e validação utilizados pelo próprio repositório.

Antes de executar um comando, verifique o projeto correto e prefira utilizar a solução existente:

```bash
dotnet restore <solution>
dotnet build <solution>
dotnet test <solution>
```

O objetivo é validar localmente a alteração realizada.

---

# 52. PROIBIDO Deploy

O agente NÃO possui autorização para realizar deploy.

É PROIBIDO executar automaticamente qualquer ação que altere ambientes externos.

Isso inclui:

```text
Production
Staging
PreProduction
QA remoto
Azure
AWS
GCP
Render
Docker Registry remoto
GHCR
Kubernetes remoto
servidores
VMs
bancos remotos
serviços externos
```

Mesmo que existam credenciais disponíveis no ambiente, isso NÃO significa autorização para utilizá-las.

---

# 53. GitHub Actions / CI/CD

O agente pode ANALISAR arquivos de CI/CD.

Exemplos:

```text
.github/workflows/
Dockerfile
docker-compose.yml
deployment scripts
```

Porém, NÃO deve disparar pipelines remotos.

É proibido, sem autorização explícita:

```text
workflow_dispatch
deploy
release
publish remoto
push de imagem
execução manual de pipeline
alteração de secrets
```

Alterações em arquivos de CI/CD somente devem ser realizadas quando fizerem parte da tarefa.

---

# 54. Git

O agente pode analisar:

```bash
git status
git diff
git log
```

e comandos locais equivalentes necessários para compreender as alterações.

Por padrão, NÃO deve executar:

```bash
git push
git push --force
git tag + push
gh pr merge
gh release
```

Não enviar código para repositório remoto sem autorização explícita.

Também não sobrescrever alterações existentes do desenvolvedor.

Antes de modificar arquivos, verifique mudanças locais existentes quando possível.

---

# 55. Serviços Externos

Não executar operações reais contra fornecedores externos durante testes.

Exemplos:

```text
Payment Gateway
Email
SMS
Storage
APIs externas
Webhooks
Message Brokers remotos
```

Utilizar:

```text
Mocks
Fakes
Stubs
Test Doubles
ambientes locais
```

quando apropriado.

Nunca criar cobrança, pagamento ou operação real apenas para validar um teste.

---

# 56. Banco de Dados Local

Testes e validações devem utilizar banco local, em memória, container local ou estratégia de testes existente no projeto.

É PROIBIDO executar:

```text
DELETE
UPDATE
INSERT
ALTER
DROP
TRUNCATE
migration
seed destrutivo
```

em banco remoto sem autorização explícita.

Uma connection string disponível não deve ser considerada autorização.

---

# 57. Docker

Docker pode ser utilizado localmente quando necessário.

Exemplo:

```bash
docker compose build
docker compose up
```

desde que isso opere somente no ambiente local.

Não executar:

```bash
docker push
```

ou publicação em registry remoto sem autorização explícita.

---

# 58. Antes de Implementar

Antes de escrever código, execute esta análise:

```text
[ ] Entendi o requisito?
[ ] Localizei a camada correta?
[ ] Procurei implementação semelhante?
[ ] Procurei interfaces existentes?
[ ] Procurei Value Objects existentes?
[ ] Procurei Validators existentes?
[ ] Procurei Repository existente?
[ ] Procurei testes existentes?
[ ] Verifiquei regras de Tenant?
[ ] Verifiquei autenticação/autorização?
[ ] A solução respeita DDD?
[ ] A solução respeita Clean Architecture?
[ ] A solução respeita CQRS?
```

Não começar criando arquivos aleatoriamente.

Primeiro compreenda o fluxo existente.

---

# 59. Durante a Implementação

Durante a alteração:

```text
[ ] Manter regras de negócio no Domain
[ ] Manter casos de uso na Application
[ ] Manter detalhes técnicos na Infrastructure
[ ] Manter Controllers finos
[ ] Respeitar Repository Contracts
[ ] Reutilizar Value Objects
[ ] Respeitar Multi-Tenancy
[ ] Não duplicar abstrações
[ ] Não adicionar dependências arquiteturais inválidas
[ ] Criar/atualizar testes
[ ] Fazer apenas alterações relacionadas à tarefa
```

---

# 60. Depois da Implementação

Após concluir o código:

```text
[ ] Revisar git diff
[ ] Verificar referências entre projetos
[ ] Compilar solução localmente
[ ] Executar testes localmente
[ ] Verificar novos warnings relevantes
[ ] Verificar testes falhando
[ ] Verificar se regra de negócio ficou na camada correta
[ ] Verificar se Controller continua fino
[ ] Verificar se Application não depende de Infrastructure
[ ] Verificar se Domain continua independente
[ ] Verificar TenantId
[ ] Verificar tratamento de erros
[ ] Verificar CancellationToken
[ ] Verificar código duplicado
[ ] Verificar código morto
```

---

# 61. Build

Antes de considerar uma alteração concluída, executar localmente, quando o ambiente permitir:

```bash
dotnet build
```

O objetivo é terminar com:

```text
Build succeeded.
```

Se o build falhar por causa da alteração realizada, corrija antes de concluir.

Se o build não puder ser executado por limitação do ambiente, informe explicitamente.

Não esconda erros de compilação.

---

# 62. Testes

Após o build, executar localmente:

```bash
dotnet test
```

Quando houver múltiplos projetos de teste, executar os projetos relevantes ou a solução completa conforme apropriado.

O objetivo é:

```text
Failed: 0
```

Não remova ou ignore testes existentes simplesmente para obter build verde.

Não altere asserts corretos para fazer uma implementação incorreta passar.

---

# 63. Testes Falhando

Se um teste existente falhar:

1. investigar a causa;
2. verificar se a alteração provocou a falha;
3. corrigir a implementação quando necessário;
4. não modificar o teste automaticamente.

Um teste somente deve ser alterado quando o comportamento esperado realmente tiver mudado conforme o requisito.

---

# 64. Warnings

Não introduza novos warnings relevantes.

Não utilize:

```csharp
#pragma warning disable
```

ou supressões equivalentes apenas para esconder problemas.

Supressões devem possuir justificativa técnica real.

---

# 65. SonarCloud / Qualidade

Quando a tarefa estiver relacionada ao SonarCloud, trate a causa do problema.

Não faça alterações artificiais apenas para satisfazer a métrica.

Verificar quando aplicável:

```text
Bugs
Vulnerabilities
Security Hotspots
Code Smells
Duplications
Coverage
Complexity
Maintainability
```

Cobertura deve representar testes úteis.

---

# 66. Regra Contra Overengineering

Não transforme uma tarefa simples em uma reconstrução arquitetural.

Evite criar desnecessariamente:

```text
Factories
Strategies
Builders
Services
Managers
Helpers
Wrappers
Abstractions
Interfaces
Base classes
```

Uma abstração deve resolver um problema real.

DDD e Clean Architecture não significam criar uma classe para cada linha de código.

---

# 67. Regra Contra Atalhos

Ao mesmo tempo, simplicidade NÃO significa violar arquitetura.

Nunca justificar:

```text
"É mais simples colocar no Controller."
"É só uma consulta, então vou usar DbContext direto."
"Vou referenciar Infrastructure para resolver rápido."
```

A solução deve ser simples **dentro das regras arquiteturais**.

---

# 68. Prioridade das Decisões

Quando houver mais de uma solução possível, utilizar esta ordem:

```text
1. Correção funcional
2. Regra de negócio
3. Arquitetura existente
4. Segurança
5. Consistência com o projeto
6. Testabilidade
7. Manutenibilidade
8. Performance
9. Menor complexidade
```

Não sacrificar arquitetura e segurança para economizar poucas linhas de código.

---

# 69. Em Caso de Dúvida

Se houver dúvida arquitetural:

1. NÃO invente;
2. procure exemplos existentes;
3. procure testes;
4. procure documentação;
5. analise dependências;
6. escolha a solução mais consistente com o repositório.

Se ainda existir uma decisão importante e ambígua que possa alterar comportamento do negócio, apresente a dúvida antes de realizar uma mudança destrutiva ou arquiteturalmente significativa.

---

# 70. Critério de Conclusão — Definition of Done

Uma tarefa somente pode ser considerada concluída quando:

```text
[ ] Requisito implementado
[ ] Regra de negócio na camada correta
[ ] Clean Architecture preservada
[ ] DDD preservado
[ ] CQRS preservado
[ ] SOLID respeitado
[ ] Multi-Tenancy preservado
[ ] Segurança preservada
[ ] Testes criados/atualizados quando necessários
[ ] Build local executado com sucesso
[ ] Testes locais executados com sucesso
[ ] Nenhum erro conhecido escondido
[ ] Nenhuma dependência arquitetural indevida adicionada
[ ] Nenhum deploy realizado
[ ] Nenhum ambiente remoto alterado
```

---

# 71. Regra Final para o Agente

Antes de alterar qualquer código deste repositório:

> **ENTENDA PRIMEIRO. IMPLEMENTE DEPOIS.**

Nunca assuma que uma nova implementação deve substituir a arquitetura existente.

Sempre:

```text
ANALISAR
   ↓
ENTENDER
   ↓
LOCALIZAR O PADRÃO EXISTENTE
   ↓
DEFINIR A CAMADA CORRETA
   ↓
IMPLEMENTAR A MENOR ALTERAÇÃO
   ↓
CRIAR/ATUALIZAR TESTES
   ↓
BUILD LOCAL
   ↓
TESTES LOCAIS
   ↓
REVISAR
```

A regra fundamental deste projeto é:

> **Domain contém o negócio.  
> Application contém os casos de uso.  
> Infrastructure contém detalhes técnicos e implementações.  
> API contém somente a interface HTTP e composição necessária.**

E, acima de tudo:

> **NUNCA coloque Infrastructure dentro de Controller.  
> NUNCA coloque regra de negócio no Controller.  
> NUNCA faça Application depender de Infrastructure.  
> NUNCA faça Domain depender das outras camadas.  
> NUNCA faça deploy ou altere ambiente remoto sem autorização explícita.**

Quando a implementação parecer exigir a violação de uma dessas regras, **pare e reavalie o design** em vez de contornar a arquitetura.