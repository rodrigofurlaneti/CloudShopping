CREATE DATABASE IF NOT EXISTS ECommerceDB;
USE ECommerceDB;

-------------------------------------------------------------------------------
-- 0. TENANTS (MULTI-EMPRESA)
-------------------------------------------------------------------------------
CREATE TABLE Tenants (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CompanyName VARCHAR(150) NOT NULL,
    Domain VARCHAR(100) UNIQUE, 
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
);

-------------------------------------------------------------------------------
-- 1. LOOKUP TABLES GLOBAIS (APENAS TIPOS DE SISTEMA)
-------------------------------------------------------------------------------
CREATE TABLE CustomerTypes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
);
INSERT INTO CustomerTypes (Id, Name) VALUES (1, 'Guest'), (2, 'Lead'), (3, 'B2C'), (4, 'B2B');

CREATE TABLE AddressTypes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
);
INSERT INTO AddressTypes (Id, Name) VALUES (1, 'Shipping'), (2, 'Billing');

CREATE TABLE PaymentStatus (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
);
INSERT INTO PaymentStatus (Id, Name) VALUES (1, 'Processing'), (2, 'Approved'), (3, 'Declined'), (4, 'Refunded');

-------------------------------------------------------------------------------
-- 1.1 SETORES DO KANBAN (PADRÃO DO SISTEMA OU CUSTOMIZADO POR TENANT)
-------------------------------------------------------------------------------
CREATE TABLE OrderSectors (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NULL, -- NULL significa que é um setor padrão global da plataforma para novos tenants
    Name VARCHAR(100) NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_tenant_sector_name (TenantId, Name)
);

-- Inserindo os Setores Padrões Globais do Sistema (TenantId = NULL)
INSERT INTO OrderSectors (Id, TenantId, Name) VALUES 
(1, NULL, 'Novos / Faturamento'),
(2, NULL, 'Armazém (Separação/Embalagem)'),
(3, NULL, 'Expedição'),
(4, NULL, 'Em Trânsito'),
(5, NULL, 'Concluídos'),
(6, NULL, 'Exceções / Pós-Venda');

-------------------------------------------------------------------------------
-- 1.2 STATUS DO PEDIDO (PADRÃO DO SISTEMA OU CUSTOMIZADO POR TENANT)
-------------------------------------------------------------------------------
CREATE TABLE OrderStatus (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NULL, -- NULL significa padrão global da plataforma
    OrderSectorId INT NOT NULL, 
    Name VARCHAR(50) NOT NULL,
    IsSystemDefault BOOLEAN DEFAULT FALSE, -- Identifica se veio do template do sistema
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE,
    FOREIGN KEY (OrderSectorId) REFERENCES OrderSectors(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_tenant_status_name (TenantId, Name)
);

-- Inserindo os Status Padrões Globais do Sistema (TenantId = NULL, IsSystemDefault = TRUE)
INSERT INTO OrderStatus (Id, TenantId, OrderSectorId, Name, IsSystemDefault) VALUES 
(1, NULL, 1, 'Pending', TRUE),
(2, NULL, 1, 'Paid', TRUE),
(3, NULL, 1, 'Invoiced', TRUE),
(4, NULL, 2, 'Processing', TRUE),
(5, NULL, 2, 'Separating', TRUE),
(6, NULL, 2, 'Packing', TRUE),
(7, NULL, 3, 'GenerateLabel', TRUE),
(8, NULL, 3, 'ReadyToShip', TRUE),
(9, NULL, 3, 'Shipped', TRUE),
(10, NULL, 4, 'TrackingNumber', TRUE),
(11, NULL, 4, 'Intransit', TRUE),
(12, NULL, 5, 'Delivered', TRUE),
(13, NULL, 6, 'DeliveryFailed', TRUE),
(14, NULL, 6, 'Returning', TRUE),
(15, NULL, 6, 'Refunded', TRUE),
(16, NULL, 6, 'Canceled', TRUE);

-------------------------------------------------------------------------------
-- 2. BASE TABLES (CLIENTES, PRODUTOS E INVENTÁRIO)
-------------------------------------------------------------------------------
CREATE TABLE Customers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    Email VARCHAR(100) NULL, 
    PasswordHash VARCHAR(255) NULL, 
    CustomerTypeId INT NOT NULL DEFAULT 1, 
    SessionToken CHAR(36) DEFAULT (UUID()), 
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE RESTRICT,
    FOREIGN KEY (CustomerTypeId) REFERENCES CustomerTypes(Id) ON DELETE RESTRICT,
    UNIQUE KEY uk_tenant_email (TenantId, Email)
);

CREATE TABLE Individuals (
    CustomerId INT PRIMARY KEY,
    TaxId CHAR(11) NOT NULL, 
    FullName VARCHAR(100) NOT NULL, 
    BirthDate DATE NULL,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

CREATE TABLE Companies (
    CustomerId INT PRIMARY KEY,
    BusinessTaxId CHAR(14) NOT NULL, 
    CompanyName VARCHAR(150) NOT NULL, 
    StateTaxId VARCHAR(15) NULL,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

CREATE TABLE Addresses (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    CustomerId INT NOT NULL,
    AddressTypeId INT NOT NULL, 
    Street VARCHAR(150) NOT NULL, 
    Number VARCHAR(10) NOT NULL,
    Neighborhood VARCHAR(50), 
    City VARCHAR(50) NOT NULL, 
    State CHAR(2) NOT NULL, 
    ZipCode CHAR(8) NOT NULL,
    IsDefault BOOLEAN DEFAULT FALSE, 
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
    FOREIGN KEY (AddressTypeId) REFERENCES AddressTypes(Id) ON DELETE RESTRICT
);

CREATE TABLE Contacts (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    CustomerId INT NOT NULL,
    Name VARCHAR(100) NOT NULL, 
    Email VARCHAR(100), 
    Phone VARCHAR(15), 
    Position VARCHAR(50), 
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

CREATE TABLE Products (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    TenantId INT NOT NULL,
    SKU VARCHAR(30) NOT NULL, 
    Name VARCHAR(150) NOT NULL,
    Price DECIMAL(12,2) NOT NULL, 
    PhysicalStock INT NOT NULL DEFAULT 0, 
    ReservedStock INT NOT NULL DEFAULT 0,
    AvailableStock INT GENERATED ALWAYS AS (PhysicalStock - ReservedStock) VIRTUAL, 
    
    -- Endereçamento Logístico (Value Object StockLocation)
    Location_Aisle VARCHAR(10) NULL,
    Location_Rack VARCHAR(10) NULL,
    Location_Level VARCHAR(10) NULL,
    Location_Position VARCHAR(10) NULL,
    
    -- Versão para Controle de Concorrência Otimista
    Version INT NOT NULL DEFAULT 1,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE RESTRICT,
    UNIQUE KEY uk_tenant_sku (TenantId, SKU) 
);

-- Tabela para Auditoria de Movimentação/Inventário de Estoque
CREATE TABLE StockMovements (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    MovementType VARCHAR(30) NOT NULL,
    QuantityChanged INT NOT NULL,
    BalanceAfterMovement INT NOT NULL,
    Reason VARCHAR(150) NOT NULL,
    
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- Gerenciamento de Imagens do Produto
CREATE TABLE ProductImages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    IsPrimary BOOLEAN DEFAULT FALSE,
    DisplayOrder INT NOT NULL DEFAULT 0,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE INDEX idx_productimages_productid ON ProductImages(ProductId);

-------------------------------------------------------------------------------
-- 3. TRANSACTIONAL TABLES (CARTS & ORDERS)
-------------------------------------------------------------------------------
CREATE TABLE Carts (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    CustomerId INT NOT NULL UNIQUE,
    ExpiresAt DATETIME(6) GENERATED ALWAYS AS (UpdatedAt + INTERVAL 30 DAY) VIRTUAL,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);

CREATE TABLE CartItems (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    CartId INT NOT NULL,
    ProductId INT NOT NULL, 
    Quantity INT NOT NULL, 
    UnitPrice DECIMAL(12,2) NOT NULL,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE Orders (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    TenantId INT NOT NULL,
    CustomerId INT NOT NULL,
    OrderDate DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6), 
    TotalAmount DECIMAL(12,2) NOT NULL,
    OrderStatusId INT NOT NULL, 
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE RESTRICT,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (OrderStatusId) REFERENCES OrderStatus(Id) ON DELETE RESTRICT
);

CREATE TABLE OrderStateHistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    OrderStatusId INT NOT NULL,
    Notes VARCHAR(255) NULL, 
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (OrderStatusId) REFERENCES OrderStatus(Id) ON DELETE RESTRICT
);

CREATE INDEX idx_orderstatehistory_orderid ON OrderStateHistory(OrderId);

CREATE TABLE OrderAddresses (
    OrderId INT PRIMARY KEY,
    AddressTypeId INT NOT NULL DEFAULT 1, 
    Street VARCHAR(150) NOT NULL, 
    Number VARCHAR(10) NOT NULL,
    Neighborhood VARCHAR(50), 
    City VARCHAR(50) NOT NULL, 
    State CHAR(2) NOT NULL, 
    ZipCode CHAR(8) NOT NULL,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (AddressTypeId) REFERENCES AddressTypes(Id) ON DELETE RESTRICT
);

CREATE TABLE OrderItems (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    OrderId INT NOT NULL,
    ProductId INT NOT NULL, 
    Quantity INT NOT NULL, 
    UnitPrice DECIMAL(12,2) NOT NULL,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY, 
    OrderId INT NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL, 
    Amount DECIMAL(12,2) NOT NULL, 
    PaymentStatusId INT NOT NULL DEFAULT 1, 
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (PaymentStatusId) REFERENCES PaymentStatus(Id) ON DELETE RESTRICT
);