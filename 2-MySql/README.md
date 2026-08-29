# Arquitetura do Banco de Dados

Abaixo está o diagrama Entidade-Relacionamento do nosso E-commerce:

```mermaid
erDiagram
    %% ==========================================
    %% RELACIONAMENTOS
    %% ==========================================

    %% Multi-Tenant
    Tenants ||--o{ Customers : "has (1:N)"
    Tenants ||--o{ Products : "has (1:N)"
    Tenants ||--o{ Orders : "has (1:N)"

    %% Lookups / Domínios
    OrderSectors ||--o{ OrderStatus : "groups (1:N)"
    OrderStatus ||--o{ Orders : "defines (1:N)"
    OrderStatus ||--o{ OrderStateHistory : "records (1:N)"
    CustomerTypes ||--o{ Customers : "categorizes (1:N)"
    AddressTypes ||--o{ Addresses : "categorizes (1:N)"
    AddressTypes ||--o{ OrderAddresses : "categorizes (1:N)"
    PaymentStatus ||--o{ Payments : "defines (1:N)"

    %% Clientes
    Customers ||--|| Individuals : "is (1:1)"
    Customers ||--|| Companies : "is (1:1)"
    Customers ||--o{ Addresses : "owns (1:N)"
    Customers ||--o{ Contacts : "has (1:N)"
    Customers ||--|| Carts : "owns (1:1)"
    Customers ||--o{ Orders : "places (1:N)"

    %% Produtos & Carrinho
    Products ||--o{ CartItems : "added to (1:N)"
    Products ||--o{ OrderItems : "ordered in (1:N)"
    Carts ||--o{ CartItems : "contains (1:N)"

    %% Pedidos
    Orders ||--o{ OrderStateHistory : "tracks timeline (1:N)"
    Orders ||--|| OrderAddresses : "ships to (1:1)"
    Orders ||--o{ OrderItems : "contains (1:N)"
    Orders ||--o{ Payments : "paid via (1:N)"

    %% ==========================================
    %% ESTRUTURA DAS TABELAS
    %% ==========================================

    Tenants {
        int Id PK
        varchar CompanyName
        varchar Domain
    }

    OrderSectors {
        int Id PK
        varchar Name
    }

    OrderStatus {
        int Id PK
        int OrderSectorId FK
        varchar Name
    }

    Customers {
        int Id PK
        int TenantId FK
        int CustomerTypeId FK
        varchar Email
    }

    Individuals {
        int CustomerId PK, FK
        char TaxId "CPF"
        varchar FullName
    }

    Companies {
        int CustomerId PK, FK
        char BusinessTaxId "CNPJ"
        varchar CompanyName
    }

    Addresses {
        int Id PK
        int CustomerId FK
        int AddressTypeId FK
        varchar ZipCode
    }

    Products {
        int Id PK
        int TenantId FK
        varchar SKU
        decimal Price
        int AvailableStock
    }

    Carts {
        int Id PK
        int CustomerId FK
        datetime ExpiresAt
    }

    CartItems {
        int Id PK
        int CartId FK
        int ProductId FK
        int Quantity
    }

    Orders {
        int Id PK
        int TenantId FK
        int CustomerId FK
        int OrderStatusId FK
        decimal TotalAmount
    }

    OrderStateHistory {
        int Id PK
        int OrderId FK
        int OrderStatusId FK
        datetime CreatedAt "Timestamp exato"
    }

    OrderAddresses {
        int OrderId PK, FK
        int AddressTypeId FK
        varchar ZipCode
    }

    OrderItems {
        int Id PK
        int OrderId FK
        int ProductId FK
        int Quantity
    }

    Payments {
        int Id PK
        int OrderId FK
        int PaymentStatusId FK
        decimal Amount
    }
```
