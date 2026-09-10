AGENTS.md

CloudShopping — Regras para Banco de Dados MySQL

Este documento define as regras obrigatórias que qualquer agente de IA deve seguir ao analisar, criar, alterar, corrigir, migrar ou otimizar o banco de dados MySQL do projeto CloudShopping.

Este arquivo deve permanecer na raiz da pasta responsável pelo banco:

MySql/
├── AGENTS.md
├── CreateDatabase.sql
├── SeedAdmin.sql
├── migrations/
└── scripts/

Estas instruções complementam as regras existentes no Backend e devem ser consideradas sempre que uma alteração envolver persistência, estrutura de dados, integridade, multi-tenancy, índices, migrations, seeds ou scripts SQL.

«IMPORTANTE: antes de criar ou alterar qualquer SQL, analise o schema existente, migrations já executadas, entidades do Backend, repositories, configurações de persistência e regras de negócio relacionadas.

NÃO invente tabelas, colunas, relacionamentos, constraints, índices, enums, status, procedures, triggers ou regras de negócio sem verificar primeiro o modelo existente.

NÃO altere uma migration histórica já aplicada apenas para fazer o banco atual funcionar.

Alterações evolutivas devem ser realizadas por novas migrations.»

---

1. Objetivo da Camada MySQL

A pasta "MySql" é responsável pela definição e evolução da persistência do CloudShopping.

Ela deve proteger:

- estrutura relacional;
- integridade dos dados;
- relacionamentos;
- multi-tenancy;
- isolamento entre empresas;
- isolamento entre filiais quando aplicável;
- constraints;
- índices;
- unicidade;
- histórico;
- auditoria;
- estoque;
- carrinho;
- pedidos;
- pagamentos;
- catálogo;
- cupons;
- operações;
- permissões;
- Outbox;
- integrações;
- rastreabilidade;
- performance;
- evolução segura do schema.

O banco NÃO deve ser tratado apenas como armazenamento passivo.

Ele deve garantir integridade estrutural sem assumir indevidamente regras que pertencem ao Domain.

---

2. Estrutura Atual

A estrutura da pasta segue o padrão:

MySql/
├── AGENTS.md
├── CreateDatabase.sql
├── SeedAdmin.sql
│
├── migrations/
│   ├── 000_...
│   ├── 001_...
│   ├── 002_...
│   ├── ...
│   └── 012_...
│
└── scripts/

As migrations existentes representam a evolução histórica do banco.

Elas abrangem áreas como:

Storefront
Reference Data
Cart Integrity
Asaas
Operations
Engagement
Outbox
Coupons
Imports
Catalog
Permissions
LogTracker

O agente deve respeitar essa evolução.

---

3. Fonte de Verdade

Antes de alterar o banco, considerar conjuntamente:

Domain
   ↓
Application
   ↓
Infrastructure / Persistence
   ↓
MySQL Schema

Nenhuma dessas camadas deve ser analisada isoladamente quando a alteração afeta contrato persistido.

Antes de criar uma coluna, verificar se existe propriedade correspondente.

Antes de remover uma coluna, verificar se Backend ainda depende dela.

Antes de criar relacionamento, verificar regra real do domínio.

---

4. Ordem Obrigatória de Análise

Para qualquer tarefa de banco:

1. Ler este AGENTS.md
        ↓
2. Ler CreateDatabase.sql
        ↓
3. Analisar migrations existentes
        ↓
4. Localizar tabelas afetadas
        ↓
5. Localizar entidades correspondentes
        ↓
6. Localizar configurações de persistência
        ↓
7. Localizar repositories/queries
        ↓
8. Identificar regra de negócio
        ↓
9. Identificar impacto multi-tenant
        ↓
10. Criar menor alteração possível

Nunca começar criando SQL apenas com base na descrição da tarefa.

---

5. MySQL

O banco utiliza MySQL.

SQL deve ser compatível com a versão utilizada pelo projeto.

Não introduzir sintaxe específica de:

SQL Server
PostgreSQL
Oracle
SQLite

em scripts MySQL.

---

6. Engine

Manter:

ENGINE=InnoDB

para tabelas relacionais do sistema, salvo justificativa técnica explícita.

InnoDB fornece:

- transactions;
- foreign keys;
- row-level locking;
- crash recovery;
- integridade relacional.

---

7. Charset

Manter padrão:

utf8mb4

para suportar Unicode completo.

Não criar tabelas com charset incompatível sem necessidade.

---

8. Collation

Preservar a collation definida pelo projeto.

Não misturar collations aleatoriamente entre tabelas e colunas.

Collations incompatíveis podem gerar:

- erros em joins;
- comparações inconsistentes;
- problemas de índice;
- comportamento inesperado em buscas.

---

9. Identificadores

Preservar o padrão de IDs utilizado pelo schema existente.

Antes de criar:

Id BIGINT

ou outro tipo, verificar tabelas semelhantes.

Não introduzir UUID, INT ou VARCHAR como chave primária em uma área que utiliza outro padrão sem justificativa arquitetural.

---

10. Primary Keys

Toda entidade persistida deve possuir chave primária apropriada.

Exemplo conceitual:

PRIMARY KEY (`Id`)

Não criar tabela operacional sem PK.

---

11. Foreign Keys

Relacionamentos reais devem possuir Foreign Keys quando compatível com a arquitetura.

Exemplo:

CONSTRAINT `FK_Order_Customer`
    FOREIGN KEY (`CustomerId`)
    REFERENCES `Customers` (`Id`)

Não depender apenas do Backend para garantir integridade referencial básica.

---

12. Foreign Key Não É Regra de Negócio Completa

Foreign Key garante existência estrutural.

Ela não substitui regras como:

pedido pertence ao Tenant;
produto está disponível;
cliente pode comprar;
pagamento pode ser confirmado;
estoque pode ser reservado.

Essas regras continuam no Domain/Application.

---

13. ON DELETE

Nunca adicionar:

ON DELETE CASCADE

automaticamente.

Antes de usar cascade, analisar:

- histórico;
- auditoria;
- pagamentos;
- pedidos;
- dependências;
- Soft Delete;
- rastreabilidade.

Em sistemas comerciais, exclusão em cascata pode destruir histórico crítico.

---

14. Soft Delete

O projeto utiliza "IsActive" em diferentes entidades.

Quando uma entidade segue Soft Delete:

DELETE lógico
        ↓
IsActive = false

Não substituir por:

DELETE FROM ...

sem confirmar que exclusão física é permitida.

---

15. Soft Delete e Queries

Quando "IsActive" representa registro ativo, queries operacionais devem respeitar esse comportamento.

Mas não adicionar:

WHERE IsActive = 1

automaticamente em qualquer consulta sem analisar finalidade.

Consultas administrativas, auditoria e histórico podem precisar visualizar inativos.

---

16. Multi-Tenancy

CloudShopping é multi-tenant.

"TenantId" é elemento estrutural crítico.

Toda nova tabela de dados pertencentes a uma empresa deve ser analisada para determinar se precisa de:

TenantId

Nunca omitir Tenant apenas porque relacionamento indireto permitiria descobrir a empresa.

---

17. Isolamento de Tenant

Dados de:

Tenant A

não podem colidir ou ser confundidos com:

Tenant B

Constraints, índices e queries devem considerar Tenant quando necessário.

---

18. Unicidade Multi-Tenant

Valores únicos por empresa normalmente devem utilizar constraint composta.

Exemplo:

UNIQUE KEY `UX_Product_Tenant_SKU`
(
    `TenantId`,
    `SKU`
)

e não necessariamente:

UNIQUE (`SKU`)

global.

Sempre verificar a regra real.

---

19. Registros Globais

Partes do modelo permitem registros globais através de:

TenantId = NULL

quando o registro pode ser compartilhado/global e customizado por Tenant.

Esse comportamento deve ser preservado onde já fizer parte do modelo.

Não alterar automaticamente:

TenantId NULL

para:

TenantId NOT NULL

sem analisar a semântica da tabela.

---

20. Global + Tenant Override

Quando existir modelo:

registro global
TenantId = NULL

registro customizado
TenantId = X

queries devem considerar precedência corretamente.

Não retornar simultaneamente versões conflitantes quando a regra determinar override.

---

21. Branch

Quando uma entidade pertence a uma filial:

Tenant
   ↓
Branch

preservar ambos os conceitos.

"BranchId" não substitui "TenantId" conceitualmente.

Antes de criar tabela relacionada à filial, verificar padrão existente.

---

22. Integridade Tenant + Branch

Uma Branch deve pertencer ao Tenant correto.

Não permitir relacionamento lógico:

Tenant A
   ↓
registro
   ↓
Branch do Tenant B

quando isso puder ser impedido pela modelagem ou Application.

---

23. DATETIME

O projeto utiliza precisão temporal como:

DATETIME(6)

Preservar esse padrão quando tabelas relacionadas utilizarem a mesma convenção.

Não misturar indiscriminadamente:

DATETIME
DATETIME(6)
TIMESTAMP
VARCHAR para data

---

24. Datas

Datas devem ser armazenadas como tipos temporais apropriados.

Nunca armazenar:

"10/09/2026"

em VARCHAR para representar data operacional.

---

25. UTC

Quando Backend trabalha com UTC, schema deve ser compatível com essa política.

Não introduzir conversões silenciosas para horário local dentro de migrations.

---

26. Valores Calculados

O schema atual utiliza valores calculados em áreas específicas.

Exemplos conceituais:

AvailableStock
Cart expiration

Antes de criar coluna calculada, avaliar:

- determinismo;
- possibilidade de índice;
- custo;
- compatibilidade MySQL;
- necessidade real.

---

27. Estoque

Estoque é uma área crítica.

O modelo deve diferenciar conceitos como:

Physical Stock
Reserved Stock
Available Stock

Conceitualmente:

AvailableStock
=
PhysicalStock - ReservedStock

quando essa for a regra existente.

---

28. Não Duplicar AvailableStock

Se "AvailableStock" já é calculado pelo banco ou derivado de outras colunas, não criar uma segunda fonte independente.

Evitar:

PhysicalStock
ReservedStock
AvailableStock manual

quando os três podem divergir.

---

29. Movimentação de Estoque

Movimentações devem ser rastreáveis.

Não atualizar saldo crítico sem considerar histórico/auditoria quando o modelo já utiliza movimentações.

Exemplos:

entrada;
saída;
reserva;
liberação;
ajuste;
cancelamento.

---

30. Auditoria de Estoque

Movimentações devem permitir responder:

O que mudou?
Quanto mudou?
Quando?
Por quê?
Qual entidade originou?
Qual Tenant?
Qual usuário/processo?

quando essas informações fizerem parte do modelo existente.

---

31. Concorrência de Estoque

Nunca implementar estoque assumindo que somente uma requisição altera o produto por vez.

Considerar:

- transactions;
- locks;
- optimistic concurrency;
- atomic updates;
- constraints.

Não resolver concorrência apenas no Front-End.

---

32. Carrinho

O banco possui regras relacionadas à integridade do carrinho.

Alterações devem considerar:

Customer;
Guest;
SessionToken;
Tenant;
Items;
Quantity;
Expiration.

---

33. Carrinho Guest

Carrinho de visitante deve possuir identificação estável conforme modelo existente.

Não criar segundo mecanismo concorrente se já existe:

SessionToken
VisitorToken

com finalidade definida.

---

34. Expiração do Carrinho

Se expiração é calculada ou persistida conforme regra atual, preservar uma única fonte de verdade.

Não adicionar outro campo de expiração redundante sem necessidade.

---

35. CartItem

Constraints devem impedir estados estruturalmente inválidos quando apropriado.

Exemplos possíveis:

item sem carrinho;
item sem produto;
duplicidade indevida;
quantidade incompatível com constraint estrutural.

Regra completa de quantidade continua no Domain.

---

36. Customer

Modelagem de cliente deve preservar distinções existentes entre:

Customer
Individual
Company
Guest
Lead
B2C
B2B

Não fundir conceitos apenas para reduzir número de tabelas.

---

37. Individual e Company

Quando o modelo utiliza especialização:

Customer
   ├── Individual
   └── Company

preservar a estrutura.

Não adicionar campos específicos de empresa diretamente em Customer sem analisar modelagem existente.

---

38. Documentos

CPF/CNPJ e outros documentos devem respeitar:

- tipo;
- tamanho;
- unicidade;
- Tenant quando aplicável;
- normalização definida.

Não alterar tipo ou tamanho sem verificar Value Objects e DTOs.

---

39. Addresses

Endereço deve permanecer relacionado ao agregado correto.

Antes de adicionar campo, verificar:

- Address entity;
- AddressType;
- CustomersLocation;
- contratos da API.

---

40. CustomersLocation

Localização geográfica pode possuir:

Latitude
Longitude
Road
Suburb
CityDistrict
City
State
Country

conforme modelo.

Não armazenar latitude/longitude como texto se schema utiliza tipo numérico apropriado.

---

41. Products

Produto deve preservar conceitos existentes como:

Tenant
SKU
Name
Price
Stock
Category
IsActive

conforme schema atual.

Não assumir que SKU é chave primária.

---

42. SKU

SKU é identificador de negócio/comercial.

Unicidade deve seguir regra do Tenant.

Não transformar SKU em relacionamento técnico quando "ProductId" já possui essa responsabilidade.

---

43. Catalog

O catálogo possui evolução própria nas migrations.

Antes de alterar catálogo, analisar migrations relacionadas.

Não recriar tabelas ou estruturas já introduzidas por migrations anteriores.

---

44. Reference Data

Dados de referência devem ser tratados separadamente de dados transacionais.

Exemplos possíveis:

status;
tipos;
categorias de sistema;
métodos;
classificações.

Não inserir dados de referência em migrations sem avaliar idempotência e ambiente.

---

45. Status

Não utilizar números mágicos:

1
2
3
4

sem verificar tabela/enum correspondente.

Backend e banco devem permanecer alinhados.

---

46. Histórico de Status

Quando existe histórico:

OrderStatusHistory
PaymentStatusHistory
...

não substituir histórico por simples atualização destrutiva se rastreabilidade for requisito.

---

47. Orders

Pedido é dado histórico crítico.

Evitar alterações destrutivas em tabelas de pedidos.

Não utilizar cascade que possa remover pedido ao excluir Customer/Product.

---

48. OrderItem

OrderItem normalmente representa snapshot comercial.

Antes de remover campos aparentemente duplicados, verificar se eles preservam informações históricas como:

nome do produto;
SKU;
preço;
quantidade;
valor no momento da compra.

Não assumir que join com Product atual reproduzirá o pedido histórico corretamente.

---

49. Payments

Pagamento é dado crítico.

Alterações devem considerar:

- idempotência;
- status;
- provider;
- external id;
- amount;
- timestamps;
- Tenant;
- Order;
- histórico.

Nunca apagar pagamentos automaticamente.

---

50. Asaas

O banco possui migrations específicas relacionadas ao Asaas.

Antes de alterar integração:

1. localizar migrations Asaas;
2. localizar entidades;
3. localizar gateway;
4. localizar webhook;
5. localizar idempotência;
6. verificar campos externos.

Não inventar campos do Asaas.

---

51. IDs Externos

Identificadores de fornecedores devem ser tratados como dados externos.

Exemplo:

AsaasPaymentId
ExternalOrderId
ProviderReference

Não utilizá-los como PK interna sem justificativa.

---

52. Webhooks

Persistência de webhook deve considerar:

- provider event id;
- idempotência;
- payload quando permitido;
- status de processamento;
- data;
- erro;
- retry.

Não processar o mesmo evento indefinidamente como novo.

---

53. Outbox

O projeto possui estrutura de Outbox.

Preservar o padrão:

Transaction
   ├── alteração de negócio
   └── Outbox Message

para garantir consistência quando essa arquitetura for utilizada.

---

54. Outbox e Atomicidade

Quando evento precisa acompanhar transação de negócio, registro do Outbox deve ocorrer na mesma unidade transacional quando essa for a arquitetura existente.

Não criar processo:

COMMIT negócio
↓
tentar gravar Outbox depois

se isso permitir perda do evento.

---

55. Outbox Processado

Estrutura deve permitir distinguir:

pendente;
processado;
falha;
tentativas;
data de processamento.

conforme implementação existente.

Não apagar mensagem imediatamente se histórico for necessário.

---

56. Idempotência

Operações críticas devem considerar idempotência no banco quando necessário.

Possíveis mecanismos:

UNIQUE KEY;
ExternalId;
IdempotencyKey;
EventId;
constraint composta.

Não depender somente de:

if (!exists)

quando concorrência puder gerar duplicidade.

---

57. Coupons

O projeto possui migrations de cupons.

Alterações devem considerar:

- Tenant;
- código;
- validade;
- utilização;
- limite;
- status;
- regras de aplicação;
- histórico quando existente.

---

58. Código de Cupom

Unicidade do código deve seguir escopo real.

Pode ser:

global

ou:

por Tenant

Não decidir sem verificar