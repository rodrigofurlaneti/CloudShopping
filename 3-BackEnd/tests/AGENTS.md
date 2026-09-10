AGENTS.md

CloudShopping — Regras para a Camada de Testes

Este documento define as regras obrigatórias que qualquer agente de IA deve seguir ao analisar, criar, alterar, corrigir ou refatorar código dentro de:

3-BackEnd/tests/

Estas instruções complementam o "3-BackEnd/src/AGENTS.md".

Quando houver conflito entre uma decisão conveniente para o teste e uma regra arquitetural ou de negócio definida no código de produção, a arquitetura e o comportamento correto do sistema têm prioridade.

«IMPORTANTE: antes de criar qualquer teste, analise a implementação real que será testada, seus contratos, regras de negócio, dependências, padrões existentes e testes semelhantes.

NÃO invente APIs, construtores, métodos, propriedades, eventos, contratos, regras de negócio ou fluxos apenas para fazer o teste compilar.

NÃO altere código de produção somente para facilitar um teste, salvo quando o teste revelar um problema real de testabilidade ou arquitetura e a alteração fizer parte do escopo solicitado.»

---

1. Objetivo da Camada de Testes

A camada de testes existe para proteger:

- regras de negócio;
- invariantes do domínio;
- casos de uso;
- validações;
- contratos;
- comportamento observável;
- isolamento multi-tenant;
- autorização;
- segurança;
- transições de estado;
- persistência quando aplicável;
- integrações quando aplicável;
- arquitetura;
- regressões;
- fluxos ponta a ponta.

O objetivo NÃO é apenas aumentar cobertura.

Um teste deve responder a pelo menos uma pergunta relevante:

Qual comportamento estou protegendo?
Qual regra pode quebrar?
Qual regressão este teste detectaria?
Qual contrato este teste garante?

---

2. Estrutura Atual de Testes

O projeto possui suítes com responsabilidades diferentes:

tests/
├── CloudShopping.Tests.Units/
│   ├── Domain/
│   └── Application/
│
├── CloudShopping.Tests.BehaviorDrivenDevelopment/
│   ├── Features/
│   └── Steps/
│
├── CloudShopping.Tests.Architecture/
│
├── CloudShopping.Tests.EndToEnd/
│   ├── Flows/
│   └── Infrastructure/
│
└── CloudShopping.Tests/

Cada suíte possui um propósito específico.

NÃO mova um teste para outra suíte apenas para obter cobertura ou evitar uma dependência corretamente proibida.

---

3. Regra Fundamental: Teste no Nível Correto

Antes de criar um teste, identifique o nível correto.

Regra de entidade / Value Object
        |
        v
Unit Test — Domain

Caso de uso / Handler / Validator
        |
        v
Unit Test — Application

Regra de negócio legível como cenário
        |
        v
BDD — Domain/Application

Dependência entre camadas / convenção estrutural
        |
        v
Architecture Test

Persistência / DI / Adapter / Gateway / Outbox / integração técnica
        |
        v
CloudShopping.Tests

Fluxo real pelo navegador
        |
        v
EndToEnd

Não use E2E para validar uma regra que pode ser protegida com teste unitário.

Não use BDD para aumentar cobertura de código puramente técnico.

Não transforme teste unitário em teste de integração.

---

4. Princípio da Pirâmide de Testes

Priorizar:

        E2E
       /   \
      /     \
 Integration
   /         \
  /           \
Unit / Domain / Application

A maior quantidade de testes deve estar próxima de "Domain" e "Application".

Testes E2E devem proteger fluxos críticos, não reproduzir todas as combinações que podem ser testadas abaixo da interface.

---

5. Antes de Escrever um Teste

O agente DEVE:

1. abrir a classe real;
2. analisar o construtor;
3. analisar interfaces utilizadas;
4. analisar regras e validações;
5. verificar "Result", exceptions e códigos de erro;
6. verificar efeitos colaterais;
7. verificar "CancellationToken";
8. verificar Tenant/Branch quando aplicável;
9. procurar testes existentes da mesma funcionalidade;
10. reutilizar o padrão já adotado.

É proibido escrever o teste baseado apenas no nome da classe.

---

6. Testes Unitários — Regra Geral

"CloudShopping.Tests.Units" deve testar exclusivamente comportamento de "Domain" e "Application".

Essa suíte:

- pode referenciar "Domain";
- pode referenciar "Application";
- NÃO deve depender de "Infrastructure";
- NÃO deve depender da API;
- NÃO deve conectar ao MySQL;
- NÃO deve chamar Asaas;
- NÃO deve chamar APIs externas;
- NÃO deve iniciar servidor;
- NÃO deve depender da internet.

Mocks substituem portas externas.

A implementação do Domain, Handler, Validator ou Use Case testado deve ser REAL.

---

7. Ferramentas dos Testes Unitários

Manter o padrão já utilizado pelo projeto:

xUnit
FluentAssertions
Moq
coverlet.collector

Não adicionar outro framework de teste ou mocking sem necessidade explícita.

Evitar misturar:

NUnit
MSTest
NSubstitute
Shouldly

se o projeto já padronizou xUnit + FluentAssertions + Moq.

---

8. Organização dos Testes Unitários

A estrutura deve refletir a camada de produção sempre que possível.

Exemplo:

CloudShopping.Tests.Units/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Services/
│   └── Rules/
│
└── Application/
    ├── Customers/
    ├── Products/
    ├── Carts/
    ├── Orders/
    ├── Payments/
    ├── Inventory/
    └── Auth/

Não criar uma pasta genérica "Tests2", "NewTests", "Misc" ou equivalente.

---

9. Nomenclatura dos Testes

Preferir nomes que expressem:

Método_Cenário_ResultadoEsperado

Exemplos:

AddItem_WhenQuantityIsValid_ShouldAddItem()
AddItem_WhenQuantityIsInvalid_ShouldReturnError()
Cancel_WhenOrderIsAlreadyCancelled_ShouldFail()
Handle_WhenTenantDoesNotOwnResource_ShouldReturnNotFound()

Também é aceitável o padrão já existente no projeto, desde que mantenha clareza.

Evitar:

Test1()
TestCart()
ShouldWork()
HappyPath()

---

10. Arrange, Act, Assert

Testes unitários devem manter separação clara:

// Arrange

// Act

// Assert

O teste deve ser simples de ler.

Se o Arrange crescer excessivamente, considere factory/builder/helper reutilizável.

Não esconda o comportamento principal dentro de helpers genéricos.

---

11. Um Comportamento Principal por Teste

Cada teste deve possuir um motivo principal para falhar.

É permitido fazer múltiplos asserts quando todos validam o mesmo comportamento.

Exemplo válido:

Adicionar item ao carrinho
- item existe;
- quantidade correta;
- total atualizado.

Evite um único teste validando cadastro, checkout, pagamento, estoque e notificação ao mesmo tempo quando esses comportamentos podem ser isolados.

---

12. Testes do Domain

Testes de Domain devem proteger comportamento real.

Priorizar:

- criação válida;
- criação inválida;
- invariantes;
- Value Objects;
- transições de estado;
- cálculos;
- regras de estoque;
- carrinho;
- pedidos;
- pagamentos;
- cupons;
- clientes;
- documentos;
- reservas;
- limites.

Não teste getters/setters apenas para aumentar cobertura.

---

13. Rich Domain Model

Quando uma entidade possui comportamento:

cart.AddItem(...)
order.Cancel(...)
payment.Confirm(...)
reservation.Release(...)

o teste deve chamar o comportamento público da entidade.

NÃO manipule estado interno por reflection apenas para simular um cenário que poderia ser produzido pela API pública.

Reflection só deve ser usada quando houver justificativa técnica forte e o padrão existente já exigir isso.

---

14. Value Objects

Todo Value Object com regra própria deve possuir testes para:

- valor válido;
- valor inválido;
- limites;
- igualdade;
- normalização, quando aplicável;
- comportamento imutável;
- representação quando relevante.

Exemplos do domínio:

Email
Password
TaxId
BusinessTaxId
Money
Quantity
SKU
PersonName
Phone
ZipCode
TrackingNumber
VisitorToken

Não replique a implementação do Value Object dentro do teste.

Teste entradas e saídas observáveis.

---

15. Casos Limite

Sempre analisar:

null
vazio
zero
negativo
máximo
mínimo
duplicado
expirado
já processado
estado incompatível
recurso inexistente
Tenant diferente
Branch diferente
concorrência
cancelamento

Aplicar apenas os cenários coerentes com a regra testada.

Não criar casos irrelevantes somente para aumentar número de testes.

---

16. Testes da Application

Testes de Application devem validar os casos de uso.

O foco é verificar:

- entrada;
- validação;
- coordenação;
- chamadas aos contratos;
- interação com Domain;
- persistência esperada;
- ausência de persistência em falha;
- resultado;
- código de erro;
- cancelamento;
- isolamento.

Handlers reais devem ser utilizados.

Mocks representam apenas dependências externas ao caso de uso.

---

17. Testes de Handlers

Para cada Handler relevante, considerar:

Happy Path
Validation Failure
Not Found
Conflict
Forbidden/Unauthorized quando aplicável
Business Rule Failure
Persistence Failure quando tratada
External Dependency Failure quando tratada
Cancellation
Tenant Isolation

Não é obrigatório criar todos quando não fazem parte do comportamento do Handler.

---

18. Validators

Validators devem possuir testes próprios quando contêm regras relevantes de entrada.

Cobrir:

- campo obrigatório;
- tamanho mínimo/máximo;
- formato;
- range;
- combinações;
- regras condicionais;
- mensagens/códigos quando fazem parte do contrato.

Não mover invariantes do Domain para Validator só para simplificar o teste.

---

19. Result Pattern

Quando o caso de uso retorna:

Result
Result<T>

o teste deve validar mais do que "IsSuccess".

Quando aplicável, verificar:

Success / Failure
Error Code
Error Type
Value
NotFound
Conflict
Validation
Unauthorized
Forbidden

Não aceitar teste que apenas verifica ausência de exception quando o contrato fornece um resultado estruturado.

---

20. Falhas Devem Provar Ausência de Efeito Colateral

Em cenários rejeitados, validar também que efeitos indevidos NÃO ocorreram.

Exemplos:

repository.Verify(x => x.AddAsync(...), Times.Never);
unitOfWork.Verify(x => x.SaveChangesAsync(...), Times.Never);
gateway.Verify(x => x.ChargeAsync(...), Times.Never);
publisher.Verify(x => x.PublishAsync(...), Times.Never);

Isso é especialmente importante em:

- validação;
- autorização;
- estoque insuficiente;
- pedido inválido;
- Tenant incorreto;
- pagamento duplicado;
- sessão expirada.

---

21. Mocks

Mockar contratos, não o sistema sob teste.

Correto:

IOrderRepository
IProductRepository
IPaymentGateway
IEmailService
IClock
IUnitOfWork
IMessagePublisher

Evitar mockar:

Order
Cart
Product
Value Objects
Handler sob teste
Validator sob teste

Entidades devem ser reais.

---

22. Verificação de Mocks

Use "Verify" quando a interação fizer parte do comportamento esperado.

Exemplos:

persistiu exatamente uma vez;
não persistiu;
publicou evento;
não chamou gateway;
consultou recurso correto;
propagou CancellationToken.

Não verifique toda chamada interna automaticamente.

Mocks excessivamente rígidos tornam testes frágeis.

---

23. CancellationToken

Métodos assíncronos que recebem "CancellationToken" devem ter cenários de propagação quando relevante.

O teste pode verificar que o token recebido pelo Handler foi encaminhado para:

- repository;
- UnitOfWork;
- gateway;
- serviço externo;
- publisher.

Não substituir sempre por "CancellationToken.None" se o comportamento de cancelamento fizer parte do contrato.

---

24. Async

Testes assíncronos devem utilizar:

public async Task ...

e:

await ...

É proibido utilizar:

.Result
.Wait()
.GetAwaiter().GetResult()

sem justificativa excepcional.

---

25. Tempo e Relógio

Testes não devem depender de:

DateTime.Now
DateTime.UtcNow
Task.Delay(...)
Thread.Sleep(...)

quando o projeto disponibiliza abstração de relógio.

Para expiração, sessão, reserva e validade:

- injete/fake/mock o clock;
- fixe o instante;
- avance o tempo de maneira controlada.

O teste deve produzir sempre o mesmo resultado.

---

26. Dados Aleatórios

Dados aleatórios precisam ser controlados.

Se utilizar Bogus ou equivalente:

- configure seed quando o valor afetar assert;
- não dependa de sorte;
- evite geração aleatória para casos limite;
- declare explicitamente valores importantes para a regra.

Um teste não pode falhar aleatoriamente.

---

27. Factories, Builders e Fixtures

Factories e Builders são permitidos para reduzir ruído.

Exemplo:

CustomerBuilder
ProductBuilder
CartBuilder
OrderBuilder
CommandFactory

Eles devem:

- produzir objetos válidos por padrão;
- permitir sobrescrever campos relevantes;
- não esconder regras;
- não fazer IO;
- não compartilhar estado mutável entre testes.

---

28. Isolamento entre Testes

Todo teste deve poder executar:

- sozinho;
- em conjunto;
- em qualquer ordem;
- repetidamente.

Não depender de estado deixado por outro teste.

É proibido exigir ordem como:

Teste A cria
Teste B atualiza
Teste C exclui

Cada teste deve preparar seu próprio contexto.

---

29. Paralelismo

Não desabilite paralelismo global apenas para corrigir teste mal isolado.

Se uma suíte específica precisa ser sequencial por uso de recurso compartilhado real, documente a necessidade.

E2E pode possuir execução sequencial quando isso protege o ambiente compartilhado.

---

30. Multi-Tenancy

CloudShopping é multi-tenant.

Todo comportamento que acessa dado pertencente a empresa deve considerar isolamento por Tenant.

Criar testes para garantir, quando aplicável:

Tenant A acessa dado do Tenant A -> permitido
Tenant A tenta acessar dado do Tenant B -> rejeitado
TenantId ausente/inválido -> comportamento definido

NUNCA remova filtro de Tenant do código para fazer teste passar.

---

31. Branch

Quando a regra também depende de filial, testar isolamento e escopo de "BranchId".

Tenant e Branch não devem ser tratados como sinônimos.

O teste deve refletir o nível real de isolamento exigido pelo caso de uso.

---

32. Guest / SessionToken / VisitorToken

Fluxos de visitante devem proteger:

- criação de sessão;
- token válido;
- token inválido;
- token expirado;
- carrinho do visitante;
- associação correta;
- conversão para cliente quando existir;
- impossibilidade de acessar sessão de outro visitante.

Não utilize GUID fixo compartilhado entre testes se isso puder gerar colisão.

---

33. Autenticação e Autorização

Testes nunca devem enfraquecer segurança.

Cobrir, quando aplicável:

credencial válida;
credencial inválida;
conta inativa;
sessão expirada;
permissão insuficiente;
Tenant incorreto;
revogação;
token inválido.

Não hardcodar senhas reais, JWTs reais, API Keys ou secrets.

---

34. Soft Delete

Quando uma entidade utiliza Soft Delete, testar o comportamento correto:

Delete -> IsActive = false

ou o padrão real existente.

Também considerar:

- item inativo não aparece em consulta ativa;
- tentativa de operar item inativo;
- reativação, quando suportada.

Não escrever teste esperando DELETE físico se a regra é Soft Delete.

---

35. Estoque

Regras de estoque devem considerar, quando aplicável:

- saldo disponível;
- quantidade inválida;
- entrada;
- saída;
- ajuste;
- reserva;
- liberação;
- reserva duplicada;
- estoque insuficiente;
- idempotência;
- concorrência;
- auditoria da movimentação.

O assert deve validar estado e efeitos persistidos relevantes.

---

36. Carrinho

Cobrir comportamentos relevantes como:

- criar carrinho;
- adicionar item;
- atualizar quantidade;
- remover item;
- impedir quantidade inválida;
- produto indisponível;
- cálculo de subtotal/total;
- Guest;
- Customer;
- Tenant;
- expiração;
- promoção/cupom quando aplicável.

O teste deve usar o comportamento real da entidade/Handler.

---

37. Pedido

Para Order, analisar estados e transições.

Testar, quando aplicável:

criação;
confirmação;
cancelamento;
pagamento;
expedição;
entrega;
transição inválida;
operação repetida;
estoque/reserva;
Tenant;
total;
itens.

Não forçar estado inválido por setter se a entidade protege a transição.

---

38. Pagamento e Idempotência

Pagamentos devem possuir testes específicos para idempotência quando a regra existir.

Exemplo conceitual:

mesma requisição/chave
        |
        +--> não gerar duas cobranças
        +--> não duplicar pagamento
        +--> retornar resultado consistente

Mocks de gateway devem confirmar quantidade de chamadas.

---

39. Integrações Externas

Integrações reais NÃO pertencem à suíte unitária/BDD.

Unit/BDD:

IPaymentGateway -> Mock
IExternalCatalog -> Mock
IEmailSender -> Mock

Integração técnica deve ser validada em suíte apropriada.

Nunca chamar produção.

Nunca usar credenciais reais no repositório.

---

40. CloudShopping.Tests — Testes de Integração/Técnicos

O projeto "CloudShopping.Tests" contém testes de aspectos como:

- Access;
- ArchitectureRegression;
- AsaasGateway;
- Catalog;
- Commerce;
- Coupon;
- DependencyInjection;
- Engagement;
- Import;
- Operation;
- Outbox;
- Payment;
- RequestCancellation;
- SessionLifecycle;
- WorkerShutdown.

Ao alterar essa suíte, primeiro identifique se o teste é:

integração;
regressão;
composição/DI;
adapter;
gateway;
worker;
persistência;
outbox;

Não migre automaticamente esses testes para Units.

---

41. Persistência

Quando um teste realmente precisa testar persistência:

- utilizar infraestrutura controlada;
- isolar dados;
- não utilizar banco de produção;
- preparar estado explicitamente;
- validar escrita e leitura relevantes;
- limpar somente o que o próprio teste criou, quando aplicável;
- respeitar Tenant/Branch.

Não mockar EF Core para fingir que está testando persistência.

---

42. Repository

Repository unitário não deve testar regra de negócio.

Testes de repository devem validar aspectos técnicos como:

- filtro;
- query;
- mapeamento;
- persistência;
- Tenant;
- Soft Delete;
- tracking/no-tracking quando relevante;
- paginação;
- include;
- concorrência técnica.

Regras de negócio devem estar cobertas em Domain/Application.

---

43. Unit of Work

Quando um caso de uso exige commit:

- validar "SaveChangesAsync" no sucesso;
- validar ausência de commit em rejeição;
- validar cancelamento quando relevante;
- validar atomicidade em teste de integração quando necessário.

Não simular atomicidade complexa somente com mocks e chamar isso de teste transacional.

---

44. Outbox

Testes de Outbox devem proteger:

- gravação do evento;
- payload esperado;
- persistência na mesma unidade transacional quando essa for a arquitetura;
- processamento;
- idempotência;
- retry quando implementado;
- marcação de processado;
- falha sem perda silenciosa.

Não dependa de broker externo em teste unitário.

---

45. Workers

Workers devem ser testados sem loops infinitos.

Utilizar:

- CancellationToken controlado;
- serviços mockados/fakes;
- execução limitada;
- relógio controlado quando necessário.

Testar desligamento gracioso.

Não utilizar "Thread.Sleep" para “dar tempo” ao worker.

---

46. BDD — Objetivo

"CloudShopping.Tests.BehaviorDrivenDevelopment" descreve comportamentos de negócio executáveis.

Utiliza:

Reqnroll
Gherkin
xUnit
FluentAssertions
Moq

A suíte deve referenciar apenas:

Domain
Application

Não deve acessar banco, API, Infrastructure ou Asaas real.

---

47. Linguagem dos Features

Arquivos ".feature" devem permanecer em português, conforme padrão atual.

Exemplo:

Funcionalidade: Reserva de estoque

  Cenário: Reservar quantidade disponível
    Dado que existe estoque disponível para o produto
    Quando o cliente reservar uma quantidade válida
    Então a reserva deve ser criada
    E o saldo disponível deve ser reduzido

Priorizar linguagem de negócio.

Evitar detalhes técnicos como:

DbContext
HTTP 200
classe X
método Y
Moq
SQL

salvo quando o comportamento especificado for realmente técnico.

---

48. Given / When / Then

BDD deve manter semântica:

Given / Dado    -> contexto
When / Quando   -> ação
Then / Então    -> consequência

Não colocar asserts escondidos no "Given".

Não executar a ação principal no "Given".

Não usar "Then" apenas para preparar estado.

---

49. Step Definitions

Steps devem ser reutilizáveis por assunto, sem se tornarem genéricos demais.

Evitar:

Dado("que alguma coisa existe")
Quando("eu faço a operação")
Então("deve funcionar")

Preferir passos específicos ao domínio.

Cada cenário deve possuir fixture/contexto isolado.

---

50. BDD e Código Real

O cenário BDD deve executar:

- entidade real;
- Handler real;
- Validator real;
- pipeline real quando fizer parte do comportamento.

Mocks apenas nas portas externas.

BDD não deve ser uma simulação escrita em Steps que replica a regra sem executar produção.

---

51. Cenários BDD Obrigatórios Quando Aplicáveis

Para regras críticas considerar:

- sucesso;
- rejeição;
- limite;
- recurso ausente;
- duplicidade;
- concorrência;
- isolamento;
- cancelamento;
- expiração;
- idempotência.

Não criar "Pending", "Skip" ou assert vazio para representar cobertura faltante.

---

52. Arquivos Gerados pelo Reqnroll

Arquivos ".feature.cs" gerados automaticamente:

- não devem ser editados manualmente;
- não devem ser tratados como fonte;
- não devem ser usados para esconder alterações;
- não devem ser versionados quando o padrão do projeto os ignora.

Sempre alterar o ".feature" ou Step correspondente.

---

53. Tags BDD

Manter tags coerentes com o padrão do projeto:

@dominio
@aplicacao
@estoque
@sessoes
@carrinho
@pedido
@pagamento

Adicionar novas tags somente quando agregarem organização real.

---

54. Testes de Arquitetura

"CloudShopping.Tests.Architecture" existe para transformar decisões arquiteturais em regras executáveis.

Tecnologias atuais:

xUnit
FluentAssertions
NetArchTest.Rules

Esses testes podem referenciar:

Domain
Application
Infrastructure
API

porque precisam inspecionar as dependências compiladas.

---

55. Regras Arquiteturais que Devem Permanecer Protegidas

No mínimo:

Domain !-> Application
Domain !-> Infrastructure
Domain !-> API
Domain !-> ASP.NET
Domain !-> bibliotecas de banco

Application !-> Infrastructure
Application !-> API
Application !-> EF Core
Application !-> Dapper
Application !-> MySQL
Application !-> contratos HTTP

Infrastructure !-> API

Controllers !-> Infrastructure

Além das regras estruturais já implementadas.

---

56. Regras de Modelo e CQRS

Testes de arquitetura podem proteger:

- entidades com criação controlada;
- setters encapsulados;
- coleções protegidas;
- contratos de repository em camada interna;
- implementação de repository na Infrastructure;
- Commands em namespace apropriado;
- Queries em namespace apropriado;
- Handler único;
- Validator para Commands quando exigido;
- Query Handler sem "IUnitOfWork" quando essa é a política;
- BDD referenciando apenas Domain/Application.

---

57. Teste de Arquitetura Não Deve Ser Burlado

É PROIBIDO:

- adicionar lista de exceções sem justificativa;
- ignorar namespace problemático;
- filtrar classe específica apenas para teste passar;
- afrouxar regra por causa de código legado;
- remover assert;
- usar "Skip".

Quando um teste de arquitetura revela uma violação real, a violação deve ser corrigida na arquitetura.

---

58. Arquitetura Vermelha Pode Ser Resultado Correto

Uma suíte de arquitetura falhando porque encontrou uma violação real NÃO significa que o teste está errado.

O agente deve distinguir:

falha do teste
vs.
teste detectando falha do sistema

Nunca “corrija” o teste apenas para deixar o pipeline verde.

---

59. End-to-End

"CloudShopping.Tests.EndToEnd" valida o sistema pelo navegador.

Stack atual:

.NET 9
xUnit
Selenium.WebDriver
Selenium.Support

Essa suíte não deve referenciar diretamente Domain/Application/Infrastructure para manipular estado interno do sistema.

O fluxo deve acontecer pela interface real.

---

60. Responsabilidade do E2E

E2E deve validar fluxos críticos como usuário real.

Exemplos:

login;
cadastro;
carrinho;
checkout;
administração;
CRUDs importantes;
fluxos de pedido;
operações críticas.

Não testar detalhes internos de classe via E2E.

---

61. Ambiente E2E

E2E deve utilizar ambiente dedicado e explicitamente configurado.

Nunca:

- assumir banco de desenvolvimento;
- usar produção;
- executar migration destrutiva automaticamente;
- limpar banco inteiro;
- trocar connection string silenciosamente.

Configuração ausente deve falhar claramente.

---

62. E2E Local

Durante a fase atual do projeto, preferir endpoints loopback/local conforme o padrão existente.

Exemplo:

http://localhost:5173

A configuração deve informar explicitamente:

Base URL
Environment
Admin username
Admin password
Product ID
Headless

Segredos devem permanecer fora do Git.

---

63. Credenciais E2E

Nunca commitar:

senha;
token;
JWT;
API Key;
cookie;
connection string sensível.

Arquivos locais de configuração devem permanecer ignorados pelo Git.

Arquivos ".example" devem conter somente placeholders seguros.

---

64. Dados E2E

Cada teste deve criar ou identificar seus próprios dados de maneira controlada.

Quando criar registros:

- usar nomes claramente identificáveis como E2E;
- usar identificadores únicos quando necessário;
- excluir/desativar somente dados criados pelo próprio teste, quando suportado;
- evitar limpeza global.

Se determinado dado permanecer para inspeção, documentar isso.

---

65. E2E e Ordem

E2E não deve depender da ordem lógica de outros testes.

Mesmo quando a execução da suíte for sequencial para reduzir interferência, cada fluxo deve ser independente.

Um teste não pode exigir que outro tenha executado antes.

---

66. E2E e Esperas

Utilizar esperas explícitas baseadas em condição.

Evitar:

Thread.Sleep(...)
Task.Delay(5000)

como sincronização de Selenium.

Esperar por:

elemento visível;
elemento clicável;
URL;
estado;
texto;
condição de DOM.

---

67. Screenshots de Falha

Screenshots podem ser geradas em falhas quando a configuração permitir.

Cuidados:

- não capturar senha;
- não expor token;
- não coletar cookies;
- não versionar screenshot;
- desabilitar em contexto sensível quando necessário.

Screenshot é evidência auxiliar, não substitui assert.

---

68. Integração com Asaas em E2E

Não disparar cobrança real acidentalmente.

Cenários de pagamento externo devem utilizar ambiente Sandbox explicitamente preparado quando o escopo exigir integração real.

Abrir uma página de pagamento não prova:

- cobrança;
- conciliação;
- webhook;
- estorno;
- idempotência.

Cada comportamento deve possuir evidência adequada.

---

69. Cobertura

Cobertura é uma métrica de apoio.

Na suíte unitária atual, a cobertura de "Domain" e "Application" deve ser medida separadamente.

Meta existente:

>= 90% de linhas em Domain
>= 90% de linhas em Application

Não somar BDD, integração ou E2E artificialmente para mascarar baixa cobertura unitária.

---

70. Quando a Tarefa Exigir 100%

Se a tarefa solicitar 100% de cobertura de uma classe específica:

- cobrir todos os caminhos relevantes;
- cobrir branches relevantes;
- cobrir erros;
- cobrir limites;
- verificar efeitos colaterais;
- não excluir classe do relatório;
- não usar pragma para esconder linha;
- não marcar como GeneratedCode;
- não criar teste sem assert útil.

100% de linhas não garante qualidade, mas a meta solicitada deve ser alcançada legitimamente.

---

71. SonarCloud

Nunca criar teste apenas para “enganar” o SonarCloud.

É proibido:

assert True;
executar linha sem verificar resultado;
reflection só para tocar código;
ignorar branch real;
excluir arquivo da cobertura;
alterar configuração para esconder código;

Se código não é testável, analisar primeiro o desenho.

---

72. Cobertura Pendente

Quando houver relatório como:

cobertura-pendente.csv
COBERTURA.md
inventario-casos-de-uso.csv

utilize-o como inventário, não como verdade absoluta.

Antes de implementar:

1. confirmar se a classe ainda existe;
2. confirmar se ainda está sem cobertura;
3. localizar comportamento;
4. escolher suíte correta;
5. criar teste útil.

---

73. Casos de Uso sem Cobertura

Para cada caso de uso pendente:

Request
   |
Validator
   |
Handler
   |
Domain
   |
Ports
   |
Result

identificar quais caminhos ainda não são protegidos.

Não duplicar cenário já coberto por outro teste unitário equivalente apenas porque está em arquivo diferente.

---

74. Testes de Regressão

Toda correção de bug relevante deve receber teste que:

1. falharia antes da correção;
2. passa após a correção;
3. representa o problema real;
4. evita retorno da regressão.

O nome do teste deve expressar o cenário, não o número do ticket apenas.

---

75. Exceptions

Quando Domain lança exception como parte de uma violação de invariante, testar:

- tipo correto;
- condição que provoca a exception;
- mensagem somente quando faz parte do contrato.

Quando Application usa Result Pattern, não exigir exception se o padrão real converte falhas em "Result".

---

76. Mensagens de Erro

Não acoplar teste a texto completo de mensagem quando somente código/tipo faz parte do contrato.

Preferir:

Error.Code
Error.Type

quando disponíveis.

Testar texto exato apenas quando ele é requisito funcional.

---

77. HTTP

HTTP status deve ser testado na camada apropriada.

Não usar teste de Domain para verificar "200", "404" ou "409".

Quando teste de API/integração existir, validar:

status;
payload;
headers relevantes;
autorização;
Tenant;
contrato.

---

78. Serialização

Testes de serialização são apropriados quando:

- contrato externo depende do formato;
- JSON possui naming específico;
- enum/formato de data é relevante;
- integração pode quebrar silenciosamente.

Não testar serialização padrão do framework sem motivo.

---

79. Banco e Constraints

Quando uma regra é garantida também por constraint de banco, pode ser necessário teste de integração.

Exemplos:

índice único;
foreign key;
not null;
chave composta;
concorrência;
unique por Tenant.

Não tentar provar constraint real usando apenas Moq.

---

80. Concorrência

Quando o domínio possui risco de concorrência, testar no nível adequado.

Unitário:

- comportamento determinístico;
- conflitos lógicos;
- idempotência.

Integração:

- optimistic concurrency;
- constraint;
- transaction;
- race relevante.

Não criar teste concorrente instável baseado em timing.

---

81. Idempotência

Operações idempotentes devem provar:

1ª execução -> efeito esperado
2ª execução equivalente -> nenhum efeito duplicado

Validar:

- estado;
- persistência;
- chamadas externas;
- eventos;
- resultado.

---

82. Eventos de Domínio

Quando entidades geram Domain Events:

- validar evento esperado;
- payload relevante;
- ausência de duplicidade;
- ausência de evento em falha.

Não testar implementação interna do dispatcher em um teste de entidade.

---

83. Auditoria

Operações auditáveis devem testar, no nível adequado:

- criação da informação de auditoria;
- ator;
- Tenant;
- operação;
- valores relevantes;
- não criação quando a operação falha.

Não use valores sensíveis em asserts/logs.

---

84. Logs

Não escrever teste que depende de log se o log não faz parte do comportamento requerido.

Quando logging for requisito técnico:

- use logger fake/mock;
- valide nível/evento relevante;
- não valide texto frágil desnecessariamente.

Nunca colocar segredo no log de teste.

---

85. Performance

Teste unitário não deve possuir limite de milissegundos extremamente rígido em máquina compartilhada.

Performance deve ser testada com estratégia específica quando requerida.

Evitar:

elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(5));

sem justificativa.

---

86. Flaky Tests

Teste instável é defeito.

Ao encontrar flaky test, investigar:

- tempo;
- concorrência;
- ordem;
- dados compartilhados;
- rede;
- random;
- timezone;
- cultura;
- ambiente;
- browser.

Não resolver flaky test adicionando retry indiscriminadamente.

---

87. Cultura e Localização

Quando parsing/formatação depende de cultura:

- definir cultura explicitamente;
- não depender da máquina do desenvolvedor;
- especialmente datas e decimais.

Nos ".feature" em português, manter o padrão atual de valores decimais adotado pelo projeto.

---

88. Timezone

Não assumir timezone local do computador.

Quando uma regra depende de data/hora:

- preferir UTC internamente conforme arquitetura;
- fixar instante;
- testar conversão apenas quando ela fizer parte da regra.

---

89. Testes Não Podem Alterar Regra de Produção

É proibido mudar:

if de negócio;
validação;
Tenant;
autorização;
Soft Delete;
transição;
constraint;
Result;

somente porque o teste esperado pelo agente não passa.

Primeiro verificar se a expectativa do teste está errada.

---

90. Código de Produção com Problema

Se durante a criação de teste for detectado bug real:

1. documentar o comportamento encontrado;
2. criar teste de regressão quando possível;
3. corrigir produção somente se fizer parte da tarefa;
4. não mascarar o bug no teste.

---

91. Não Criar APIs Exclusivas para Teste

Evitar adicionar em produção:

public void SetStatusForTest(...)
public void ForceTenant(...)
public void SetId(...)

apenas para montar cenário.

Preferir factories, builders e APIs legítimas do domínio.

---

92. InternalsVisibleTo

Não adicionar "InternalsVisibleTo" automaticamente.

Use apenas se o projeto já adotar o padrão ou houver razão arquitetural válida.

Não exponha internals só para aumentar cobertura.

---

93. Reflection

Reflection não deve ser ferramenta padrão de teste.

Pode ser usada para testes de arquitetura ou metaprogramação quando necessário.

Não usar reflection para contornar encapsulamento de entidade rica.

---

94. Duplicação

Antes de criar helper, fixture ou factory:

- procurar equivalente;
- reutilizar;
- evitar segundo padrão concorrente.

Mas não criar uma abstração enorme somente para eliminar três linhas de Arrange.

---

95. Clareza Acima de DRY Extremo

Em testes, pequena duplicação pode ser melhor do que abstração obscura.

O leitor deve entender:

contexto;
ação;
resultado esperado.

sem navegar por cinco helpers.

---

96. Dados Importantes Devem Ficar Visíveis

Valores que influenciam a regra devem permanecer explícitos no teste.

Exemplo:

var availableStock = 5;
var requestedQuantity = 6;

é melhor que esconder ambos em uma factory genérica quando o objetivo é testar estoque insuficiente.

---

97. Asserts

Utilizar FluentAssertions conforme padrão.

Asserts devem ser específicos.

Preferir:

result.IsSuccess.Should().BeFalse();
result.Error.Code.Should().Be("stock.insufficient");

em vez de:

result.Should().NotBeNull();

quando existe comportamento mais relevante a provar.

---

98. Asserts em Coleções

Verificar conteúdo semanticamente.

Preferir:

ContainSingle
ContainEquivalentOf
BeEquivalentTo
OnlyContain
NotContain

conforme intenção.

Não depender da ordem quando a ordem não faz parte do contrato.

---

99. Testes de Queries

Queries devem validar:

- retorno esperado;
- ausência;
- filtro;
- Tenant;
- paginação;
- ordenação quando contratual;
- mapping;
- cancelamento.

Query Handler não deve modificar estado.

Quando política arquitetural proíbe "IUnitOfWork" em Query Handler, o teste de arquitetura deve continuar protegendo isso.

---

100. Testes de Commands

Commands devem validar:

- Validator;
- Handler;
- mudança de estado;
- persistência;
- eventos;
- resultado;
- ausência de efeitos na falha.

Commands devem continuar separados de Queries.

---

101. Dependency Injection

Testes de DI devem provar:

- contratos importantes resolvem;
- lifetime correto quando crítico;
- implementação esperada registrada;
- ausência de dependência circular relevante.

Não transformar teste de DI em Service Locator no código de produção.

---

102. Controllers

Controllers devem permanecer finos.

Testes não devem incentivar:

Controller -> DbContext
Controller -> Repository concreto
Controller -> regra de negócio

Se uma regra só pode ser testada instanciando infraestrutura dentro do Controller, isso pode indicar violação arquitetural.

---

103. APIs Externas

Para adapters HTTP, testar em integração controlada:

- request;
- headers;
- payload;
- status;
- timeout;
- cancelamento;
- mapping;
- erro;
- retry quando implementado;
- idempotência.

Mocks unitários não substituem teste do adapter quando o formato externo é importante.

---

104. Contratos de Repository

Se a Application depende de contrato de repository, o teste unitário deve mockar o contrato.

Não referenciar a implementação da Infrastructure.

A implementação concreta é testada separadamente.

---

105. Segurança de Testes

Nunca usar em código versionado:

- credenciais reais;
- dados pessoais reais;
- cartão real;
- tokens reais;
- cookies reais;
- connection strings de produção;
- segredo de webhook.

Dados de teste devem ser artificiais.

---

106. Dados Pessoais

Evitar nomes, emails, documentos e telefones reais.

Utilizar:

example.test
CPF/CNPJ sintéticos apropriados ao teste
nomes fictícios
IDs de teste

Sem expor dados de clientes reais.

---

107. Testes Desabilitados

É proibido utilizar:

Skip
Pending
Ignore
return antecipado
assert vazio

para esconder teste quebrado ou cobertura faltante.

Se um requisito ainda não pode ser testado por ausência de interface/funcionalidade, documentar explicitamente o motivo na documentação da suíte.

---

108. Teste que Não Compila

Nunca “resolver” compilação:

- adicionando referência arquitetural proibida;
- duplicando DTO;
- criando fake de classe que não existe;
- mudando assinatura de produção sem necessidade;
- movendo regra para Infrastructure.

Analise a arquitetura correta primeiro.

---

109. ProjectReference

Antes de adicionar qualquer "ProjectReference" em projeto de teste, validar o propósito da suíte.

Regras principais:

Units -> Domain + Application
BDD -> Domain + Application

Architecture -> pode inspecionar Domain + Application + Infrastructure + API

E2E -> não deve depender das camadas internas para executar o fluxo

Não quebrar esses limites para facilitar import.

---

110. Pacotes

Antes de adicionar pacote NuGet:

1. verificar se já existe ferramenta equivalente;
2. verificar padrão das outras suítes;
3. confirmar necessidade;
4. evitar aumentar dependências sem benefício real.

---

111. Execução Local

Após alterar testes, executar primeiro a suíte afetada.

Exemplos:

dotnet test 3-BackEnd/tests/CloudShopping.Tests.Units
dotnet test 3-BackEnd/tests/CloudShopping.Tests.BehaviorDrivenDevelopment
dotnet test 3-BackEnd/tests/CloudShopping.Tests.Architecture
dotnet test 3-BackEnd/tests/CloudShopping.Tests

E2E somente quando o ambiente estiver corretamente preparado:

dotnet test 3-BackEnd/tests/CloudShopping.Tests.EndToEnd

Use o nome real do projeto/pasta existente no checkout atual.

---

112. Cobertura Local

Para Units, preservar a configuração existente de cobertura.

Exemplo existente:

./3-BackEnd/tests/CloudShopping.Tests.Units/Test-Coverage.ps1 -NoRestore

Não alterar a meta ou filtro apenas para obter sucesso.

---

113. Ordem de Validação

Quando a alteração for ampla:

1. Build
2. Units
3. BDD
4. Architecture
5. Integration/Technical
6. Coverage
7. E2E, quando ambiente estiver preparado

Uma falha deve ser analisada, não ignorada.

---

114. Build Verde Não Significa Teste Correto

Compilar é requisito mínimo.

Também verificar:

- asserts significativos;
- comportamento correto;
- cobertura legítima;
- arquitetura;
- isolamento;
- determinismo.

---

115. Todos os Testes Verdes Também Não Significam Arquitetura Correta

Se a suíte de arquitetura possui violações conhecidas, não esconder essas falhas.

A meta é:

código correto
+
testes corretos
+
arquitetura correta

e não apenas:

pipeline verde

---

116. Mudanças Mínimas

Ao criar testes:

- não renomear produção sem necessidade;
- não reorganizar namespaces sem escopo;
- não refatorar módulo inteiro;
- não atualizar pacotes aleatoriamente;
- não alterar arquitetura;
- não mudar regra de negócio.

Faça a menor alteração capaz de proteger corretamente o comportamento solicitado.

---

117. Teste Deve Falhar Pelo Motivo Certo

Antes de considerar um teste válido, pergunte:

Se a regra quebrar, este teste falha?
Se uma implementação diferente mas correta for criada, este teste continua válido?

Se o teste falhar por detalhes internos irrelevantes, ele está excessivamente acoplado.

---

118. Não Testar Implementação, Testar Comportamento

Evitar asserts sobre:

- número de variáveis internas;
- método privado chamado;
- estrutura interna de algoritmo;
- ordem de operações sem relevância;
- nome de classe interna.

Priorizar entrada, saída, estado e efeitos observáveis.

---

119. Regra de Negócio Tem Prioridade Sobre Cobertura

Se for necessário escolher entre:

teste artificial que aumenta cobertura

e:

teste de regra relevante que protege comportamento

escolha o segundo.

Depois aumente cobertura com cenários reais.

---

120. Definition of Done para Testes

Uma tarefa de testes só pode ser considerada concluída quando:

- testes compilam;
- testes relevantes passam;
- nenhum teste foi ignorado para mascarar problema;
- asserts são significativos;
- não há dependência externa indevida;
- arquitetura da suíte foi respeitada;
- Tenant/Branch foram considerados quando aplicáveis;
- falhas não geram efeitos colaterais indevidos;
- cancelamento foi considerado quando aplicável;
- teste é determinístico;
- não contém secret;
- não contém dado pessoal real;
- cobertura solicitada foi medida;
- nenhuma configuração foi alterada para esconder código;
- documentação da suíte foi atualizada quando necessário.

---

121. Checklist para Unit Test

Antes de finalizar:

[ ] Estou testando Domain ou Application?
[ ] A classe testada é real?
[ ] Mockei somente portas externas?
[ ] Cobri sucesso?
[ ] Cobri falha relevante?
[ ] Validei Error Code/Result?
[ ] Validei ausência de persistência na rejeição?
[ ] Considerei Tenant?
[ ] Considerei CancellationToken?
[ ] O teste é determinístico?
[ ] Não depende de banco/rede?

---

122. Checklist para BDD

[ ] O cenário descreve uma regra de negócio?
[ ] Está escrito em português?
[ ] Given prepara contexto?
[ ] When executa uma ação?
[ ] Then valida consequência?
[ ] Executa entidade/Handler real?
[ ] Mocka apenas portas externas?
[ ] Não acessa Infrastructure?
[ ] Não existe Pending/Skip?
[ ] O cenário agrega proteção real?

---

123. Checklist para Architecture Test

[ ] A regra representa uma decisão arquitetural real?
[ ] O teste detecta violações automaticamente?
[ ] Não adicionei exceções só para ficar verde?
[ ] Não filtrei classes problemáticas?
[ ] A mensagem de falha ajuda a localizar a violação?
[ ] A regra está alinhada ao src/AGENTS.md?

---

124. Checklist para Integration/Technical Test

[ ] Este comportamento realmente precisa de infraestrutura?
[ ] O ambiente é controlado?
[ ] Não usa produção?
[ ] Dados estão isolados?
[ ] Tenant está protegido?
[ ] Persistência real está sendo validada quando necessário?
[ ] Adapter/Gateway real está sob teste?
[ ] Secrets estão fora do repositório?

---

125. Checklist para E2E

[ ] Fluxo é crítico o suficiente para E2E?
[ ] Ambiente dedicado está configurado?
[ ] Não aponta para produção?
[ ] Credenciais não estão versionadas?
[ ] Teste não depende de ordem?
[ ] Esperas são explícitas?
[ ] Dados criados são identificáveis?
[ ] Limpeza afeta somente dados próprios?
[ ] Fluxo usa interface real?
[ ] Evidências não expõem informações sensíveis?

---

126. Fluxo Obrigatório do Agente

Sempre seguir:

1. Ler esta instrução
        |
        v
2. Ler src/AGENTS.md
        |
        v
3. Identificar classe/regra alvo
        |
        v
4. Ler implementação real
        |
        v
5. Procurar teste semelhante
        |
        v
6. Escolher suíte correta
        |
        v
7. Definir cenários
        |
        v
8. Implementar menor alteração
        |
        v
9. Executar suíte afetada
        |
        v
10. Executar cobertura quando aplicável
        |
        v
11. Executar arquitetura quando a mudança puder afetá-la
        |
        v
12. Relatar resultado real

---

127. Regra Final

O agente deve otimizar para:

correção
+
proteção de regra de negócio
+
determinismo
+
legibilidade
+
isolamento
+
arquitetura
+
segurança
+
manutenibilidade

e NÃO para:

quantidade de testes
+
percentual artificial
+
pipeline verde a qualquer custo

Sempre prefira um teste que prove um comportamento real do sistema.

«Testes são especificações executáveis daquilo que o sistema deve continuar fazendo.»