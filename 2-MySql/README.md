# Arquitetura do Banco de Dados

Abaixo está o diagrama Entidade-Relacionamento do nosso E-commerce:

```mermaid
erDiagram
    %% RELACIONAMENTOS
    Tenants ||--o{ Customers : "tem"
    Tenants ||--o{ Products : "tem"
    Tenants ||--o{ Orders : "tem"

    OrderSectors ||--o{ OrderStatus : "agrupa"
    OrderStatus ||--o{ Orders : "define"
    OrderStatus ||--o{ OrderStateHistory : "registra"
    CustomerTypes ||--o{ Customers : "categoriza"
    AddressTypes ||--o{ Addresses : "categoriza"
    AddressTypes ||--o{ OrderAddresses : "categoriza"
    PaymentStatus ||--o{ Payments : "define"

    Customers ||--|| Individuals : "eh"
    Customers ||--|| Companies : "eh"
    Customers ||--o{ Addresses : "possui"
    Customers ||--o{ Contacts : "tem"
    Customers ||--|| Carts : "possui"
    Customers ||--o{ Orders : "realiza"

    Products ||--o{ CartItems : "adicionado em"
    Products ||--o{ OrderItems : "pedido em"
    Products ||--o{ StockMovements : "audita"
    Carts ||--o{ CartItems : "contem"

    Orders ||--o{ OrderStateHistory : "rastreia"
    Orders ||--|| OrderAddresses : "enviado para"
    Orders ||--o{ OrderItems : "contem"
    Orders ||--o{ Payments : "pago via"

    %% ESTRUTURA DAS TABELAS
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
        int CustomerId PK
        char TaxId
        varchar FullName
    }

    Companies {
        int CustomerId PK
        char BusinessTaxId
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
        int PhysicalStock
        int ReservedStock
        int AvailableStock
        varchar Location_Aisle
        varchar Location_Rack
        varchar Location_Level
        varchar Location_Position
    }

    StockMovements {
        int Id PK
        int ProductId FK
        int QuantityChanged
        int BalanceAfterMovement
        varchar Reason
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
        datetime CreatedAt
    }

    OrderAddresses {
        int OrderId PK
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
