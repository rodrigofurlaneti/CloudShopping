# Especificação da API RESTful — E-Commerce SaaS

**Projeto:** Plataforma E-commerce Multi-Tenant (SaaS)  
**Arquitetura:** RESTful API  
**Banco de Dados:** MySQL — `ECommerceDB`  
**Formato:** `application/json`  
**Autenticação:** JWT Bearer Token  
**Multi-Tenancy:** `X-Tenant-ID`

---

# 1. Visão Geral

A API RESTful é responsável por disponibilizar os recursos da plataforma de E-commerce SaaS, permitindo que diferentes lojas utilizem a mesma infraestrutura de aplicação e banco de dados, mantendo seus dados isolados através do conceito de **Multi-Tenant**.

A API atende diferentes perfis de clientes:

- **Guest** — visitante;
- **Lead** — visitante identificado por e-mail;
- **B2C** — pessoa física;
- **B2B** — pessoa jurídica.

A jornada do usuário é progressiva:

```text
Guest
  │
  ▼
Lead
  │
  ├───────────────┐
  ▼               ▼
 B2C              B2B
  │                │
  ▼                ▼
Checkout          Checkout
```

---

# 2. Padrões Globais da API

## 2.1 Formato de Dados

Todas as requisições e respostas da API devem utilizar:

```http
Content-Type: application/json
```

### Exemplo

```http
Content-Type: application/json
Accept: application/json
```

---

# 3. Multi-Tenancy

A API utiliza o modelo **Shared Database / Shared Schema**, onde diferentes lojas compartilham a mesma infraestrutura de banco de dados.

A identificação do tenant deve ser enviada através do header:

```http
X-Tenant-ID: {tenantId}
```

### Exemplo

```http
X-Tenant-ID: 10
```

O `TenantId` deve ser aplicado em todas as operações que envolvam dados pertencentes a uma loja.

### Regra

A API nunca deve confiar em um `TenantId` enviado no corpo da requisição para definir o contexto da operação.

O tenant deve ser determinado pelo contexto da requisição:

```text
HTTP Request
     │
     ├── Authorization
     │
     └── X-Tenant-ID
             │
             ▼
       Tenant Context
             │
             ▼
        Application
             │
             ▼
          Database
```

---

# 4. Autenticação

As requisições autenticadas devem utilizar:

```http
Authorization: Bearer <JWT>
```

### Exemplo

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

O JWT deverá identificar o contexto do usuário.

Para clientes autenticados:

```text
CustomerId
TenantId
CustomerType
```

Para visitantes:

```text
SessionToken
TenantId
CustomerType = Guest
```

---

# 5. Sessão de Guest

O visitante não precisa possuir cadastro completo para utilizar a loja.

Ao entrar na aplicação, o frontend deve solicitar uma sessão:

```http
POST /api/auth/session
```

A API cria um `Customer` do tipo `Guest` e gera um `SessionToken` único.

O token da sessão é utilizado para identificar o visitante durante sua jornada.

---

# 6. Soft Delete

A API utiliza **Soft Delete**.

Endpoints `DELETE` não devem remover fisicamente os registros durante as operações normais.

Em vez disso:

```text
IsActive = true
       │
       │ DELETE
       ▼
IsActive = false
```

Exemplo:

```sql
UPDATE Addresses
SET IsActive = FALSE
WHERE Id = 10;
```

A exclusão física deve ser restrita a operações administrativas específicas, como processos relacionados à LGPD/GDPR.

---

# 7. Convenções HTTP

| Código | Significado |
|---:|---|
| `200` | Operação realizada com sucesso |
| `201` | Recurso criado |
| `204` | Operação realizada sem conteúdo de retorno |
| `400` | Requisição inválida |
| `401` | Não autenticado |
| `403` | Acesso negado |
| `404` | Recurso não encontrado |
| `409` | Conflito de negócio |
| `422` | Erro de validação de negócio |
| `500` | Erro interno |

---

# 8. Modelo de Erro

A API deve utilizar um formato padronizado para erros.

### Exemplo

```json
{
  "status": 400,
  "code": "INVALID_REQUEST",
  "message": "A requisição possui dados inválidos.",
  "errors": [
    {
      "field": "email",
      "message": "O e-mail informado é inválido."
    }
  ],
  "timestamp": "2026-08-28T14:00:00.000000"
}
```

---

# 9. Módulo de Autenticação e Sessão

**Base URL:**

```text
/api/auth
```

Responsável por iniciar e evoluir a jornada do cliente.

---

## 9.1 Criar Sessão de Guest

```http
POST /api/auth/session
```

### Descrição

Cria uma sessão para um visitante não autenticado.

### Request

Não possui payload obrigatório.

### Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "sessionToken": "550e8400-e29b-41d4-a716-446655440000",
  "customerType": "Guest",
  "expiresAt": "2026-09-27T14:00:00.000000"
}
```

### Regras

- Cria um `Customer`;
- `CustomerTypeId = 1`;
- Gera `SessionToken` UUID;
- Retorna JWT;
- O JWT deve conter o `SessionToken`.

---

# 10. Converter Guest para Lead

```http
POST /api/auth/lead
```

### Descrição

Converte um visitante `Guest` em `Lead`.

### Request

```json
{
  "email": "cliente@email.com"
}
```

### Regras

1. Identificar o Guest através do JWT;
2. Recuperar o `SessionToken`;
3. Localizar o `Customer`;
4. Atualizar o e-mail;
5. Alterar:

```text
CustomerTypeId = 2
```

### Response

```json
{
  "customerId": 100,
  "customerType": "Lead",
  "email": "cliente@email.com"
}
```

---

# 11. Login B2C/B2B

```http
POST /api/auth/login
```

### Request

```json
{
  "email": "cliente@email.com",
  "password": "********"
}
```

### Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "customerId": 100,
  "customerType": "B2C"
}
```

### Regras

- Validar e-mail;
- Validar senha;
- Validar `TenantId`;
- Retornar JWT;
- JWT deve conter o `CustomerId`;
- JWT deve conter o `TenantId`;
- JWT deve conter o tipo do cliente.

---

# 12. Módulo de Clientes

**Base URL:**

```text
/api/customers
```

Responsável pelo gerenciamento do perfil do cliente.

---

# 13. Cadastro B2C

```http
POST /api/customers/b2c
```

### Descrição

Converte um `Lead` em cliente B2C.

### Request

```json
{
  "name": "João da Silva",
  "taxId": "12345678900",
  "birthDate": "1990-01-15",
  "password": "********"
}
```

### Regras

1. Cliente deve existir;
2. Cliente deve estar no contexto do Tenant;
3. Criar registro em `Individuals`;
4. Associar o `CustomerId`;
5. Alterar:

```text
CustomerTypeId = 3
```

### Resultado

```text
Lead
 │
 │ POST /customers/b2c
 ▼
B2C
 │
 ▼
Individuals
```

---

# 14. Cadastro B2B

```http
POST /api/customers/b2b
```

### Request

```json
{
  "legalName": "Empresa Exemplo LTDA",
  "businessTaxId": "12345678000199",
  "password": "********"
}
```

### Regras

1. Cliente deve existir;
2. Cliente deve estar no contexto do Tenant;
3. Criar registro em `Companies`;
4. Associar o `CustomerId`;
5. Alterar:

```text
CustomerTypeId = 4
```

---

# 15. Consultar Perfil

```http
GET /api/customers/profile
```

### Descrição

Retorna o perfil do cliente autenticado.

O `CustomerId` não deve ser enviado na URL.

A API deve obtê-lo através do JWT.

### B2C

```json
{
  "id": 100,
  "email": "cliente@email.com",
  "type": "B2C",
  "individual": {
    "name": "João da Silva",
    "taxId": "12345678900",
    "birthDate": "1990-01-15"
  }
}
```

### B2B

```json
{
  "id": 200,
  "email": "contato@empresa.com",
  "type": "B2B",
  "company": {
    "legalName": "Empresa Exemplo LTDA",
    "businessTaxId": "12345678000199"
  }
}
```

---

# 16. Endereços

## 16.1 Listar Endereços

```http
GET /api/customers/addresses
```

Retorna somente endereços ativos:

```text
IsActive = true
```

### Response

```json
[
  {
    "id": 10,
    "type": "Shipping",
    "street": "Rua Exemplo",
    "number": "100",
    "complement": "Apto 10",
    "district": "Centro",
    "city": "São Paulo",
    "state": "SP",
    "zipCode": "01000000"
  }
]
```

---

# 17. Criar Endereço

```http
POST /api/customers/addresses
```

### Request

```json
{
  "addressTypeId": 1,
  "street": "Rua Exemplo",
  "number": "100",
  "complement": "Apto 10",
  "district": "Centro",
  "city": "São Paulo",
  "state": "SP",
  "zipCode": "01000000"
}
```

### Tipos

| ID | Tipo |
|---:|---|
| `1` | Shipping |
| `2` | Billing |

---

# 18. Contatos B2B

```http
POST /api/customers/contacts
```

Disponível somente para clientes B2B.

### Request

```json
{
  "name": "Maria Oliveira",
  "email": "maria@empresa.com",
  "phone": "+55 11 99999-9999"
}
```

### Regra

O cliente autenticado deve possuir:

```text
CustomerTypeId = 4
```

Caso contrário:

```http
403 Forbidden
```

---

# 19. Módulo de Catálogo

**Base URL:**

```text
/api/products
```

O catálogo é administrado pelo Tenant e consumido pelos clientes.

---

# 20. Listar Produtos

```http
GET /api/products
```

### Regras

Retornar somente:

```text
IsActive = true
```

O estoque apresentado ao cliente deve ser:

```text
AvailableStock =
PhysicalStock - ReservedStock
```

### Response

```json
[
  {
    "id": 1,
    "sku": "PROD-001",
    "name": "Produto Exemplo",
    "price": 99.90,
    "availableStock": 25
  }
]
```

---

# 21. Detalhes do Produto

```http
GET /api/products/{id}
```

### Exemplo

```http
GET /api/products/10
```

O produto deve pertencer ao Tenant informado no contexto da requisição.

---

# 22. Criar Produto

```http
POST /api/products
```

### Permissão

Administrador do Tenant.

### Request

```json
{
  "sku": "PROD-001",
  "name": "Produto Exemplo",
  "price": 99.90,
  "physicalStock": 100
}
```

### Regras

O SKU deve ser único dentro do Tenant:

```text
TenantId + SKU
```

---

# 23. Atualizar Estoque

```http
PUT /api/products/{id}/stock
```

### Request

```json
{
  "quantity": 20,
  "operation": "ADD"
}
```

Operações possíveis:

```text
ADD
REMOVE
```

### Exemplo

Estoque atual:

```text
PhysicalStock = 100
```

Request:

```json
{
  "quantity": 20,
  "operation": "ADD"
}
```

Resultado:

```text
PhysicalStock = 120
```

---

# 24. Módulo de Carrinho

**Base URL:**

```text
/api/cart
```

O ID do carrinho não é exposto na URL.

A API identifica o carrinho através do contexto do usuário:

```text
Guest
 │
 └── SessionToken
        │
        ▼
      Cart

B2C/B2B
 │
 └── CustomerId
        │
        ▼
      Cart
```

---

# 25. Consultar Carrinho

```http
GET /api/cart
```

### Regras

1. Identificar o usuário;
2. Localizar o carrinho;
3. Caso não exista, criar automaticamente;
4. Retornar os itens;
5. Retornar `ExpiresAt`.

### Response

```json
{
  "id": 50,
  "expiresAt": "2026-09-27T14:00:00.000000",
  "items": [
    {
      "id": 100,
      "productId": 10,
      "sku": "PROD-001",
      "name": "Produto Exemplo",
      "quantity": 2,
      "unitPrice": 99.90,
      "total": 199.80
    }
  ],
  "totalAmount": 199.80
}
```

---

# 26. Adicionar Produto ao Carrinho

```http
POST /api/cart/items
```

### Request

```json
{
  "productId": 10,
  "quantity": 2
}
```

### Regras

Antes de adicionar o produto:

```text
AvailableStock =
PhysicalStock - ReservedStock
```

A quantidade solicitada não pode ultrapassar o estoque disponível.

### Exemplo

```text
PhysicalStock = 100
ReservedStock = 80

AvailableStock = 20
```

Tentativa:

```json
{
  "productId": 10,
  "quantity": 25
}
```

Resultado:

```http
409 Conflict
```

---

# 27. Alterar Item do Carrinho

```http
PUT /api/cart/items/{id}
```

### Request

```json
{
  "quantity": 5
}
```

A alteração deve atualizar:

```text
Cart.UpdatedAt
```

Consequentemente, a validade do carrinho deve ser recalculada:

```text
ExpiresAt = UpdatedAt + 30 dias
```

---

# 28. Remover Item do Carrinho

```http
DELETE /api/cart/items/{id}
```

A operação remove o item do carrinho conforme a regra de persistência definida para `CartItems`.

A alteração deve atualizar:

```text
Cart.UpdatedAt
```

e consequentemente:

```text
Cart.ExpiresAt
```

---

# 29. Módulo de Pedidos e Checkout

**Base URL:**

```text
/api/orders
```

O módulo de pedidos é responsável por transformar o carrinho em um pedido comercial.

---

# 30. Checkout

```http
POST /api/orders/checkout
```

### Pré-requisito

O cliente deve ser:

```text
B2C
```

ou:

```text
B2B
```

`Guest` e `Lead` não podem finalizar pedidos.

---

# 31. Fluxo do Checkout

O checkout deve executar as operações necessárias dentro de uma operação transacional.

```text
Cart
 │
 ├── Validar cliente
 │
 ├── Validar itens
 │
 ├── Validar estoque
 │
 ├── Criar Order
 │
 ├── Criar OrderItems
 │
 ├── Criar OrderAddresses
 │
 ├── Reservar estoque
 │
 └── Finalizar Checkout
```

---

# 32. Snapshot do Pedido

Durante o checkout:

### Produto

```text
Products.Price
       │
       ▼
OrderItems.UnitPrice
```

### Endereço

```text
Addresses
       │
       ▼
OrderAddresses
```

O pedido passa a possuir sua própria representação histórica.

Alterações futuras em:

```text
Products
Addresses
```

não devem alterar o pedido já criado.

---

# 33. Reserva de Estoque

Durante o checkout:

```text
ReservedStock += Quantity
```

Exemplo:

```text
PhysicalStock = 100
ReservedStock = 20

Checkout:
Quantity = 5
```

Resultado:

```text
PhysicalStock = 100
ReservedStock = 25
AvailableStock = 75
```

---

# 34. Histórico de Pedidos

```http
GET /api/orders
```

### B2C

Retorna os pedidos pertencentes ao `CustomerId`.

### B2B

Retorna os pedidos relacionados à empresa do cliente.

---

# 35. Detalhes do Pedido

```http
GET /api/orders/{id}
```

### Response

```json
{
  "id": 1000,
  "status": {
    "id": 2,
    "name": "Paid"
  },
  "totalAmount": 299.80,
  "items": [
    {
      "productId": 10,
      "sku": "PROD-001",
      "quantity": 2,
      "unitPrice": 99.90,
      "total": 199.80
    }
  ],
  "address": {
    "street": "Rua Exemplo",
    "number": "100",
    "city": "São Paulo",
    "state": "SP"
  },
  "payment": {
    "status": "Approved"
  }
}
```

---

# 36. Atualizar Status do Pedido

```http
PATCH /api/orders/{id}/status
```

### Permissão

- Administrador;
- Sistema;
- Serviço autorizado.

### Request

```json
{
  "statusId": 3
}
```

### Status

| ID | Status |
|---:|---|
| `1` | Pending |
| `2` | Paid |
| `3` | Shipped |
| `4` | Canceled |

A alteração deve respeitar a máquina de estados do pedido.

Exemplo:

```text
Pending
   │
   ▼
Paid
   │
   ▼
Shipped
```

---

# 37. Módulo Financeiro

**Base URL:**

```text
/api/payments
```

Responsável pela comunicação entre pedidos e gateway de pagamento.

---

# 38. Iniciar Pagamento

```http
POST /api/orders/{id}/pay
```

### Descrição

Inicia o processo de pagamento de um pedido.

### Regra

Criar um registro em `Payments` com:

```text
PaymentStatusId = 1
```

onde:

```text
1 = Processing
```

### Exemplo de Response

```json
{
  "paymentId": 500,
  "orderId": 1000,
  "status": "Processing",
  "amount": 299.80
}
```

---

# 39. Webhook de Pagamento

```http
POST /api/payments/webhook
```

### Descrição

Endpoint utilizado pelo Gateway de Pagamento para comunicar alterações no processamento.

### Exemplo

```json
{
  "paymentId": "PAY-123456",
  "orderId": 1000,
  "status": "Approved",
  "amount": 299.80
}
```

---

# 40. Status de Pagamento

| ID | Status |
|---:|---|
| `1` | Processing |
| `2` | Approved |
| `3` | Declined |
| `4` | Refunded |

---

# 41. Integração entre Pagamento e Estoque

O pagamento possui impacto direto sobre o estoque reservado.

### Pagamento aprovado

```text
Payment
   │
   ▼
Approved
   │
   ▼
Order = Paid
   │
   ▼
Confirma reserva
```

### Pagamento recusado

```text
Payment
   │
   ▼
Declined
   │
   ▼
Order
   │
   ▼
Liberar ReservedStock
```

### Cancelamento

Quando um pedido for cancelado e possuir estoque reservado:

```text
ReservedStock -= OrderItem.Quantity
```

Isso permite que o produto volte a ficar disponível para venda.

---

# 42. Fluxo Completo da Jornada

O fluxo completo do cliente pode ser representado da seguinte maneira:

```text
┌──────────────────────────┐
│ Usuário acessa a loja   │
└────────────┬─────────────┘
             │
             ▼
POST /api/auth/session
             │
             ▼
        Guest + JWT
             │
             ▼
POST /api/cart/items
             │
             ▼
          Carrinho
             │
             ▼
POST /api/auth/lead
             │
             ▼
           Lead
             │
             ▼
POST /api/customers/b2c
             │
             ▼
            B2C
             │
             ▼
POST /api/customers/addresses
             │
             ▼
          Endereço
             │
             ▼
POST /api/orders/checkout
             │
             ├───────────────┐
             │               │
             ▼               ▼
        OrderItems      OrderAddresses
             │
             ▼
      ReservedStock
             │
             ▼
POST /api/orders/{id}/pay
             │
             ▼
        Processing
             │
             ▼
POST /api/payments/webhook
             │
        ┌────┴────┐
        ▼         ▼
    Approved   Declined
        │         │
        ▼         ▼
      Paid    Liberar Reserva
        │
        ▼
     Shipped
```

---

# 43. Mapa Geral dos Endpoints

| Método | Endpoint | Módulo | Autenticação |
|---|---|---|---|
| `POST` | `/api/auth/session` | Auth | Não |
| `POST` | `/api/auth/lead` | Auth | Guest JWT |
| `POST` | `/api/auth/login` | Auth | Não |
| `POST` | `/api/customers/b2c` | Customers | Lead JWT |
| `POST` | `/api/customers/b2b` | Customers | Lead JWT |
| `GET` | `/api/customers/profile` | Customers | JWT |
| `GET` | `/api/customers/addresses` | Customers | JWT |
| `POST` | `/api/customers/addresses` | Customers | JWT |
| `POST` | `/api/customers/contacts` | Customers | B2B JWT |
| `GET` | `/api/products` | Products | Público/Guest |
| `GET` | `/api/products/{id}` | Products | Público/Guest |
| `POST` | `/api/products` | Products | Admin |
| `PUT` | `/api/products/{id}/stock` | Products | Admin |
| `GET` | `/api/cart` | Cart | JWT |
| `POST` | `/api/cart/items` | Cart | JWT |
| `PUT` | `/api/cart/items/{id}` | Cart | JWT |
| `DELETE` | `/api/cart/items/{id}` | Cart | JWT |
| `POST` | `/api/orders/checkout` | Orders | B2C/B2B |
| `GET` | `/api/orders` | Orders | B2C/B2B |
| `GET` | `/api/orders/{id}` | Orders | B2C/B2B |
| `PATCH` | `/api/orders/{id}/status` | Orders | Admin/Sistema |
| `POST` | `/api/orders/{id}/pay` | Payments | B2C/B2B |
| `POST` | `/api/payments/webhook` | Payments | Gateway |

---

# 44. Regras de Segurança

A API deve validar, no mínimo:

```text
┌─────────────────────────────┐
│       HTTP Request          │
├─────────────────────────────┤
│ Authorization               │
│ X-Tenant-ID                 │
│ Route Parameters            │
│ Request Body                │
└──────────────┬──────────────┘
               │
               ▼
       Authentication
               │
               ▼
        Authorization
               │
               ▼
       Tenant Validation
               │
               ▼
       Business Validation
               │
               ▼
        Domain Operation
```

Nunca deve ser permitido:

- Acessar dados de outro Tenant;
- Alterar produtos de outro Tenant;
- Consultar pedidos de outro Tenant;
- Alterar endereço de outro Customer;
- Utilizar `CustomerId` de outro usuário;
- Manipular estoque sem autorização administrativa.

---

# 45. Regras de Isolamento

Todas as consultas devem considerar o contexto do Tenant.

Conceitualmente:

```sql
SELECT *
FROM Products
WHERE Id = @ProductId
  AND TenantId = @TenantId
  AND IsActive = TRUE;
```

Nunca:

```sql
SELECT *
FROM Products
WHERE Id = @ProductId;
```

O mesmo princípio deve ser aplicado a:

```text
Customers
Products
Carts
CartItems
Orders
OrderItems
Payments
Addresses
Contacts
```

---

# 46. Idempotência

Operações críticas, especialmente relacionadas a pagamento e checkout, devem considerar **idempotência**.

Recomenda-se utilizar:

```http
Idempotency-Key: <UUID>
```

Exemplo:

```http
POST /api/orders/checkout
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
```

Isso evita que uma mesma operação seja processada duas vezes devido a:

- Retry do frontend;
- Timeout;
- Retry automático;
- Problemas de rede;
- Reenvio da requisição.

O mesmo princípio deve ser aplicado aos webhooks de pagamento.

---

# 47. Transação do Checkout

O checkout deve ser tratado como uma operação transacional.

Conceitualmente:

```text
BEGIN TRANSACTION

    Validar Customer
          ↓
    Validar Cart
          ↓
    Validar Products
          ↓
    Validar AvailableStock
          ↓
    Criar Order
          ↓
    Criar OrderItems
          ↓
    Criar OrderAddresses
          ↓
    Incrementar ReservedStock
          ↓
    Finalizar Cart

COMMIT
```

Caso qualquer operação falhe:

```text
ROLLBACK
```

Isso evita situações como:

```text
Order criado
   │
   ├── OrderItems criado
   │
   └── Estoque não reservado
```

ou:

```text
Estoque reservado
   │
   └── Order não criado
```

---

# 48. Relação entre API e Banco de Dados

A API deve respeitar o modelo de domínio definido no `ECommerceDB`.

```text
                 REST API
                    │
                    ▼
             Application Layer
                    │
                    ▼
              Domain Layer
                    │
                    ▼
          Infrastructure Layer
                    │
                    ▼
                 MySQL
                    │
                    ▼
              ECommerceDB
```

Principais relacionamentos:

```text
Customer
   │
   ├── Individuals
   ├── Companies
   ├── Contacts
   ├── Addresses
   └── Cart
          │
          └── CartItems
                 │
                 ▼
              Products
                 │
                 ▼
               Stock

Cart
 │
 │ Checkout
 ▼
Order
 │
 ├── OrderItems
 ├── OrderAddresses
 └── Payments
```

---

# 49. Princípios da API

A API deve seguir os seguintes princípios:

- **RESTful**;
- **Stateless**;
- **JWT Authentication**;
- **Multi-Tenant**;
- **Soft Delete**;
- **Idempotência em operações críticas**;
- **Validação de domínio**;
- **Integridade transacional**;
- **Snapshot histórico de pedidos**;
- **Controle de concorrência de estoque**;
- **Separação entre B2C e B2B**;
- **Auditoria através de `CreatedAt` e `UpdatedAt`**.

---

# 50. Fluxo Arquitetural Final

```text
                         ┌──────────────────────┐
                         │       Frontend       │
                         │ React / Angular etc. │
                         └──────────┬───────────┘
                                    │
                                    │ HTTPS
                                    ▼
                         ┌──────────────────────┐
                         │      REST API        │
                         │                      │
                         │ Authentication       │
                         │ Authorization        │
                         │ Multi-Tenant         │
                         │ Validation           │
                         └──────────┬───────────┘
                                    │
                     ┌──────────────┼──────────────┐
                     │              │              │
                     ▼              ▼              ▼
                  Customers      Products        Cart
                     │              │              │
                     └──────────────┼──────────────┘
                                    │
                                    ▼
                                Checkout
                                    │
                                    ▼
                                  Order
                                    │
                       ┌────────────┴────────────┐
                       │                         │
                       ▼                         ▼
                   Payments                  Inventory
                       │                         │
                       ▼                         ▼
                  Payment Gateway          Stock Control
                       │
                       ▼
                    Webhook
                       │
                       ▼
                    Order
                       │
                       ▼
                  Final Status
```

---

# 51. Conclusão

A API RESTful foi projetada para suportar uma plataforma de E-commerce **SaaS Multi-Tenant**, permitindo que múltiplas lojas compartilhem a mesma infraestrutura mantendo isolamento lógico dos dados.

A arquitetura contempla toda a jornada do cliente:

```text
Guest
  ↓
Lead
  ↓
B2C / B2B
  ↓
Cart
  ↓
Checkout
  ↓
Order
  ↓
Payment
  ↓
Order Status
```

O modelo também contempla requisitos essenciais de uma plataforma comercial real:

- Isolamento Multi-Tenant;
- Autenticação baseada em JWT;
- Carrinho persistente;
- Controle de estoque concorrente;
- Reserva de estoque;
- Checkout transacional;
- Snapshot de preços;
- Snapshot de endereços;
- Integração com gateway de pagamento;
- Webhooks;
- Idempotência;
- Soft Delete;
- Auditoria;
- Suporte a B2C e B2B.

A especificação da API deve ser utilizada como contrato entre **Frontend, Backend, banco de dados e serviços externos**, servindo também como base para a implementação dos Controllers, Application Services, Domain Services, DTOs, Validators, Repositories e testes automatizados.