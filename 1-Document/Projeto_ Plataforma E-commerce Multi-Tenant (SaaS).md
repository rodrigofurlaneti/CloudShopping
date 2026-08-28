# Projeto: Plataforma E-commerce Multi-Tenant (SaaS)

**Base de Referência:** Modelo de Dados `ECommerceDB` (MySQL)

---

## 1. Visão Geral Arquitetural

O sistema opera em um modelo **SaaS Multi-Tenant (Multi-empresa)**.

Todos os clientes (lojistas) compartilham a mesma infraestrutura de banco de dados, mas seus dados são estritamente isolados através de chaves de inquilino (`TenantId`).

O ecossistema atende tanto:

- **B2C (Business-to-Consumer)** — vendas para pessoas físicas;
- **B2B (Business-to-Business)** — vendas corporativas.

### Modelo de Isolamento

```text
                    ┌─────────────────────────┐
                    │     ECommerceDB         │
                    │         MySQL           │
                    └────────────┬────────────┘
                                 │
                ┌────────────────┼────────────────┐
                │                │                │
                ▼                ▼                ▼
          ┌───────────┐    ┌───────────┐    ┌───────────┐
          │ Tenant 01 │    │ Tenant 02 │    │ Tenant 03 │
          │  Loja A   │    │  Loja B   │    │  Loja C   │
          └───────────┘    └───────────┘    └───────────┘
                │                │                │
                ▼                ▼                ▼
          TenantId = 1      TenantId = 2      TenantId = 3
```

O `TenantId` deve estar presente nas entidades que possuem dados específicos de cada loja, garantindo que consultas e operações sejam sempre realizadas dentro do contexto do respectivo tenant.

---

# 2. Domínios de Referência (Lookup Tables)

O sistema utiliza tabelas de domínio estáticas para garantir **integridade referencial**, impedindo a inserção de dados inconsistentes nas tabelas transacionais.

| Domínio | Tabela | Valores Permitidos | Regra de Uso |
|---|---|---|---|
| **Tipos de Cliente** | `CustomerTypes` | `1: Guest`, `2: Lead`, `3: B2C`, `4: B2B` | Define a jornada do usuário na plataforma. |
| **Tipos de Endereço** | `AddressTypes` | `1: Shipping`, `2: Billing` | Separa endereços de entrega e cobrança. |
| **Status do Pedido** | `OrderStatus` | `1: Pending`, `2: Paid`, `3: Shipped`, `4: Canceled` | Controla a máquina de estados do `Order`. |
| **Status do Pagamento** | `PaymentStatus` | `1: Processing`, `2: Approved`, `3: Declined`, `4: Refunded` | Controla o fluxo financeiro atrelado ao `Order`. |

### Objetivo das Lookup Tables

As tabelas de domínio possuem valores controlados e devem ser tratadas como dados de referência.

Por exemplo:

```text
Orders
   │
   └── OrderStatusId
           │
           ▼
     OrderStatus
           │
           ├── 1 - Pending
           ├── 2 - Paid
           ├── 3 - Shipped
           └── 4 - Canceled
```

Dessa forma, a aplicação não deve permitir valores arbitrários para campos que possuem domínio controlado.

---

# 3. Regras de Negócio Principais (RN)

## RN01 — Isolamento e Unicidade Multi-Tenant

### Garantia de Isolamento

Clientes, Produtos e Pedidos pertencem exclusivamente a uma empresa através do campo:

```text
TenantId
```

Nenhuma operação de consulta, criação, alteração ou exclusão deve permitir que dados de um tenant sejam acessados ou modificados por outro tenant.

### Unicidade de Identidade

Um mesmo endereço de e-mail pode ser cadastrado no banco de dados múltiplas vezes, **desde que pertença a Tenants diferentes**.

A unicidade deve ser garantida através da combinação:

```text
TenantId + Email
```

Exemplo:

| TenantId | Email | Permitido |
|---:|---|---|
| 1 | cliente@email.com | ✅ |
| 2 | cliente@email.com | ✅ |
| 3 | cliente@email.com | ✅ |
| 1 | cliente@email.com | ❌ |

Portanto, o e-mail deve ser único **somente dentro da mesma loja**.

### Unicidade de Catálogo

O código do produto (`SKU`) não pode se repetir dentro da mesma loja.

Entretanto, lojas diferentes podem utilizar o mesmo SKU.

A unicidade deve ser garantida através da combinação:

```text
TenantId + SKU
```

Exemplo:

| TenantId | SKU | Permitido |
|---:|---|---|
| 1 | PROD-001 | ✅ |
| 2 | PROD-001 | ✅ |
| 3 | PROD-001 | ✅ |
| 1 | PROD-001 | ❌ |

---

# 4. RN02 — Jornada e Tipagem do Cliente

O sistema suporta a evolução progressiva do cliente sem perda de histórico.

A jornada pode ser representada da seguinte forma:

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
Individuals     Companies
```

## 4.1 Guest — Visitante

O `Guest` representa um usuário que ainda não realizou autenticação ou cadastro completo.

Características:

- Não possui cadastro completo;
- É identificado através de `SessionToken`;
- `SessionToken` utiliza UUID;
- Pode navegar pela loja;
- Pode adicionar produtos ao carrinho;
- Pode iniciar o processo de checkout.

Exemplo:

```text
SessionToken = 550e8400-e29b-41d4-a716-446655440000
```

---

## 4.2 Lead

O `Lead` representa um usuário que forneceu alguma informação de contato, normalmente um endereço de e-mail, mas ainda não possui informações fiscais completas.

Exemplos:

- Assinatura de newsletter;
- Início do checkout;
- Cadastro parcial;
- Solicitação de informações comerciais.

---

## 4.3 B2C — Pessoa Física

O cliente `B2C` representa uma pessoa física com cadastro completo.

É obrigatório possuir relacionamento com:

```text
Individuals
```

A entidade `Individuals` deve conter informações como:

- CPF (`TaxId`);
- Data de nascimento;
- Dados pessoais necessários ao cadastro.

Representação:

```text
Customer
   │
   │ CustomerType = B2C
   ▼
Individuals
   │
   ├── TaxId
   └── BirthDate
```

---

## 4.4 B2B — Pessoa Jurídica

O cliente `B2B` representa uma empresa.

É obrigatório possuir relacionamento com:

```text
Companies
```

A entidade `Companies` deve conter:

- CNPJ (`BusinessTaxId`);
- Razão Social;
- Dados corporativos.

Um cliente B2B também pode possuir múltiplos contatos através da tabela:

```text
Contacts
```

Representação:

```text
Customer
   │
   │ CustomerType = B2B
   ▼
Companies
   │
   ├── BusinessTaxId
   ├── LegalName
   │
   └── Contacts
          ├── Contact 01
          ├── Contact 02
          └── Contact N
```

---

# 5. RN03 — Motor de Concorrência de Estoque

Para evitar o problema de **overselling**, ou seja, vender uma quantidade maior do que a disponível fisicamente, o estoque é dividido em diferentes conceitos.

## 5.1 PhysicalStock

Representa a quantidade física existente no armazém.

```text
PhysicalStock = quantidade física disponível
```

---

## 5.2 ReservedStock

Representa a quantidade que já foi reservada para:

- Carrinhos ativos;
- Processos de checkout;
- Pagamentos em processamento;
- Outras operações que temporariamente bloqueiam o estoque.

```text
ReservedStock = quantidade temporariamente reservada
```

---

## 5.3 AvailableStock

Representa a quantidade efetivamente disponível para venda.

O valor deve ser calculado dinamicamente:

```text
AvailableStock = PhysicalStock - ReservedStock
```

### Exemplo

```text
PhysicalStock = 100
ReservedStock = 25

AvailableStock = 100 - 25

AvailableStock = 75
```

Portanto, a vitrine deve apresentar:

```text
Disponível: 75 unidades
```

O `AvailableStock` não deve ser tratado como uma informação independente que pode ficar desatualizada. Seu valor deve ser derivado dos valores de estoque físico e reservado.

---

# 6. RN04 — Gestão do Carrinho de Compras

A relação entre Carrinho e Cliente é de:

```text
1 Cliente : 1 Carrinho
```

Ou seja, um cliente possui no máximo um carrinho ativo.

## Persistência Dinâmica

O carrinho possui uma data de expiração:

```text
ExpiresAt
```

Essa data é calculada automaticamente com base na última alteração:

```text
ExpiresAt = UpdatedAt + 30 dias
```

### Renovação do Carrinho

Sempre que ocorrer uma alteração no carrinho, o contador de expiração deve ser reiniciado.

Operações que renovam o carrinho:

- Adicionar produto;
- Remover produto;
- Alterar quantidade;
- Outras operações que modifiquem o conteúdo do carrinho.

Exemplo:

```text
UpdatedAt = 01/08/2026 10:00
ExpiresAt = 31/08/2026 10:00
```

Após adicionar um produto:

```text
UpdatedAt = 05/08/2026 15:30
ExpiresAt = 04/09/2026 15:30
```

---

# 7. RN05 — Imutabilidade Histórica de Pedidos (Snapshot)

O `Order` representa um **contrato comercial finalizado**.

Depois que a compra é concluída, informações utilizadas durante a venda não devem depender dos dados atuais do cadastro do cliente ou do catálogo.

O pedido deve preservar um **snapshot histórico** das informações relevantes.

---

## 7.1 Congelamento de Endereço

No momento da finalização da compra, o endereço selecionado pelo cliente é copiado da tabela:

```text
Addresses
```

para:

```text
OrderAddresses
```

O relacionamento histórico pode ser representado da seguinte forma:

```text
Customer
   │
   ▼
Addresses
   │
   │ Checkout
   ▼
OrderAddresses
   │
   ▼
Order
```

### Exemplo

Antes da compra:

```text
Endereço do Cliente
Rua A, 100
São Paulo - SP
```

O pedido recebe uma cópia:

```text
OrderAddress
Rua A, 100
São Paulo - SP
```

Posteriormente, caso o cliente altere seu endereço:

```text
Cliente
Rua B, 500
```

O pedido antigo continua armazenando:

```text
Order
Rua A, 100
São Paulo - SP
```

Isso garante a preservação do histórico da operação comercial.

---

## 7.2 Congelamento de Preço

O preço do produto no momento da compra deve ser copiado para:

```text
OrderItems.UnitPrice
```

O pedido não deve consultar o preço atual de:

```text
Products.Price
```

para reconstruir o valor histórico da venda.

### Exemplo

No momento da compra:

```text
Products.Price = R$ 100,00
```

O pedido registra:

```text
OrderItems.UnitPrice = R$ 100,00
```

Posteriormente, o produto pode ter seu preço alterado:

```text
Products.Price = R$ 150,00
```

O pedido antigo permanece:

```text
OrderItems.UnitPrice = R$ 100,00
```

Dessa forma, alterações futuras no catálogo não afetam o histórico financeiro dos pedidos.

---

# 8. Requisitos Não Funcionais e Padrões (RNF)

## RNF01 — Auditoria Contínua

Todas as tabelas devem possuir as colunas:

```text
CreatedAt
UpdatedAt
```

Ambas devem utilizar:

```sql
DATETIME(6)
```

permitindo precisão de microssegundos.

### CreatedAt

Registra a data e hora de criação do registro.

### UpdatedAt

Registra a última alteração do registro.

O banco de dados deve atualizar automaticamente o `UpdatedAt` quando ocorrer uma alteração, sem depender exclusivamente da aplicação.

Exemplo:

```sql
CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)
           ON UPDATE CURRENT_TIMESTAMP(6)
```

---

# 9. RNF02 — Deleção Lógica (Soft Delete)

Os registros não devem ser excluídos fisicamente durante as operações normais da aplicação.

Todas as tabelas devem possuir:

```text
IsActive
```

A coluna deve ser do tipo booleano e iniciar como:

```text
TRUE
```

### Registro ativo

```text
IsActive = TRUE
```

### Registro desativado

```text
IsActive = FALSE
```

Exemplo:

```sql
UPDATE Products
SET IsActive = FALSE
WHERE Id = 10;
```

O registro permanece no banco de dados, porém deixa de ser considerado ativo pela aplicação.

---

# 10. RNF03 — Precisão Financeira

Todas as colunas que representam valores monetários devem utilizar:

```sql
DECIMAL(12,2)
```

Exemplos:

- `Price`;
- `TotalAmount`;
- `UnitPrice`;
- `Amount`.

### Justificativa

Não devem ser utilizados tipos de ponto flutuante (`FLOAT` ou `DOUBLE`) para valores financeiros devido aos possíveis erros de representação e arredondamento.

Exemplo:

```sql
Price DECIMAL(12,2)
```

Isso permite trabalhar com valores como:

```text
R$ 10,00
R$ 99,99
R$ 1.250,50
```

mantendo precisão adequada para operações financeiras.

---

# 11. RNF04 — Integridade de Cascatas

A estratégia de exclusão deve diferenciar:

1. **Exclusão lógica**;
2. **Exclusão física controlada**;
3. **Tabelas de domínio**.

---

## 11.1 Exclusão Lógica

A exclusão lógica deve ser propagada pela aplicação quando necessário.

Exemplo:

```text
Customer
   │
   ├── Addresses
   ├── Orders
   └── Cart
```

Ao desativar um cliente:

```text
Customer.IsActive = FALSE
```

a aplicação pode realizar as regras necessárias para impedir que seus registros relacionados continuem sendo utilizados.

---

## 11.2 Exclusão Física

A exclusão física deve ser uma operação excepcional, destinada principalmente a cenários de:

- GDPR;
- LGPD;
- Solicitação formal de eliminação de dados;
- Operações administrativas controladas.

Quando necessária, as tabelas filhas podem utilizar:

```sql
ON DELETE CASCADE
```

### Exemplo

```text
Customer
   │
   ├── Addresses
   ├── Contacts
   └── Cart
```

Ao excluir fisicamente um `Customer`, os registros dependentes podem ser removidos automaticamente.

---

## 11.3 Tabelas de Domínio

As tabelas de domínio devem utilizar:

```sql
ON DELETE RESTRICT
```

Isso impede a remoção de um registro de domínio que ainda esteja sendo utilizado.

### Exemplo

Não deve ser possível excluir:

```text
OrderStatus
ID = 1
Name = Pending
```

caso existam pedidos relacionados:

```text
Orders.OrderStatusId = 1
```

Isso garante a integridade referencial do sistema.

---

# 12. Resumo das Regras

| Código | Regra | Objetivo |
|---|---|---|
| **RN01** | Isolamento Multi-Tenant | Garantir separação dos dados entre lojas. |
| **RN01** | Unicidade por Tenant | Permitir e-mails e SKUs iguais em lojas diferentes. |
| **RN02** | Jornada do Cliente | Permitir evolução de Guest → Lead → B2C/B2B. |
| **RN03** | Controle de Estoque | Evitar overselling através de reservas. |
| **RN04** | Expiração do Carrinho | Manter carrinhos ativos por até 30 dias após alteração. |
| **RN05** | Snapshot de Endereço | Preservar o endereço utilizado na compra. |
| **RN05** | Snapshot de Preço | Preservar o preço praticado no momento da venda. |
| **RNF01** | Auditoria | Registrar criação e atualização dos dados. |
| **RNF02** | Soft Delete | Evitar exclusões físicas nas operações normais. |
| **RNF03** | Precisão Financeira | Garantir precisão dos valores monetários. |
| **RNF04** | Integridade de Cascatas | Controlar exclusões e relacionamentos entre entidades. |

---

# 13. Princípios Arquiteturais do Modelo

O modelo de dados deve seguir os seguintes princípios:

```text
                    ┌──────────────────────────┐
                    │       Multi-Tenant       │
                    │        TenantId          │
                    └────────────┬─────────────┘
                                 │
              ┌──────────────────┼──────────────────┐
              │                  │                  │
              ▼                  ▼                  ▼
        ┌───────────┐      ┌───────────┐      ┌───────────┐
        │ Integridade│      │ Histórico │      │ Auditoria │
        │ Referencial│      │ Snapshot  │      │ CreatedAt │
        │            │      │           │      │ UpdatedAt │
        └───────────┘      └───────────┘      └───────────┘
              │                  │                  │
              └──────────────────┼──────────────────┘
                                 │
                                 ▼
                     ┌─────────────────────┐
                     │ ECommerceDB (MySQL) │
                     └─────────────────────┘
```

O modelo prioriza:

- **Isolamento de dados por Tenant**;
- **Integridade referencial**;
- **Consistência transacional**;
- **Preservação do histórico comercial**;
- **Precisão financeira**;
- **Auditoria automática**;
- **Soft Delete**;
- **Controle de concorrência de estoque**;
- **Evolução da jornada do cliente**;
- **Suporte a operações B2C e B2B**.