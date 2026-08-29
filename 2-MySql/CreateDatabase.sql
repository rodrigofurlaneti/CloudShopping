-- Criação do Banco de Dados
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
-- 1. LOOKUP TABLES (TABELAS DE DOMÍNIO / REFERÊNCIA)
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

-- 1.1 SETORES DO KANBAN (Criar antes de OrderStatus)
CREATE TABLE OrderSectors (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)
);

INSERT INTO OrderSectors (Id, Name) VALUES 
(1, 'Novos / Faturamento'),
(2, 'Armazém (Separação/Embalagem)'),
(3, 'Expedição'),
(4, 'Em Trânsito'),
(5, 'Concluídos'),
(6, 'Exceções / Pós-Venda');

-- 1.2 STATUS DO PEDIDO (Agora vinculada ao Setor)
CREATE TABLE OrderStatus (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    OrderSectorId INT NOT NULL, 
    Name VARCHAR(50) NOT NULL UNIQUE,
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (OrderSectorId) REFERENCES OrderSectors(Id) ON DELETE RESTRICT
);

INSERT INTO OrderStatus (Id, OrderSectorId, Name) VALUES 
(1, 1, 'Pending'),
(2, 1, 'Paid'),
(3, 1, 'Invoiced'),
(4, 2, 'Processing'),
(5, 2, 'Separating'),
(6, 2, 'Packing'),
(7, 3, 'GenerateLabel'),
(8, 3, 'ReadyToShip'),
(9, 3, 'Shipped'),
(10, 4, 'TrackingNumber'),
(11, 4, 'Intransit'),
(12, 5, 'Delivered'),
(13, 6, 'DeliveryFailed'),
(14, 6, 'Returning'),
(15, 6, 'Refunded'),
(16, 6, 'Canceled');

-------------------------------------------------------------------------------
-- 2. BASE TABLES (CLIENTES & PRODUTOS)
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
    
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE RESTRICT,
    UNIQUE KEY uk_tenant_sku (TenantId, SKU) 
);

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
    OrderStatusId INT NOT NULL DEFAULT 1, 
    
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
