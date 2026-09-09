-- Baseline schema only. Select target database explicitly. No DROP or personal data.
SET FOREIGN_KEY_CHECKS=0;
CREATE TABLE IF NOT EXISTS `addresses` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CustomerId` int NOT NULL,
  `AddressTypeId` int NOT NULL,
  `Street` varchar(150) NOT NULL,
  `Number` varchar(10) NOT NULL,
  `Neighborhood` varchar(50) DEFAULT NULL,
  `City` varchar(50) NOT NULL,
  `State` char(2) NOT NULL,
  `ZipCode` char(8) NOT NULL,
  `IsDefault` tinyint(1) DEFAULT '0',
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `CustomerId` (`CustomerId`),
  KEY `AddressTypeId` (`AddressTypeId`),
  CONSTRAINT `addresses_ibfk_1` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `addresses_ibfk_2` FOREIGN KEY (`AddressTypeId`) REFERENCES `addresstypes` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `addresstypes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `cartitems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CartId` int NOT NULL,
  `ProductId` int NOT NULL,
  `Quantity` int NOT NULL,
  `UnitPrice` decimal(12,2) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `CartId` (`CartId`),
  KEY `ProductId` (`ProductId`),
  CONSTRAINT `cartitems_ibfk_1` FOREIGN KEY (`CartId`) REFERENCES `carts` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `cartitems_ibfk_2` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `carts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CustomerId` int NOT NULL,
  `ExpiresAt` datetime(6) GENERATED ALWAYS AS ((`UpdatedAt` + interval 30 day)) VIRTUAL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `CustomerId` (`CustomerId`),
  CONSTRAINT `carts_ibfk_1` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `companies` (
  `CustomerId` int NOT NULL,
  `BusinessTaxId` char(14) NOT NULL,
  `CompanyName` varchar(150) NOT NULL,
  `StateTaxId` varchar(15) DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`CustomerId`),
  CONSTRAINT `companies_ibfk_1` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `contacts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CustomerId` int NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Phone` varchar(15) DEFAULT NULL,
  `Position` varchar(50) DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `CustomerId` (`CustomerId`),
  CONSTRAINT `contacts_ibfk_1` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `customers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int NOT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `PasswordHash` varchar(255) DEFAULT NULL,
  `CustomerTypeId` int NOT NULL DEFAULT '1',
  `SessionToken` char(36) DEFAULT (uuid()),
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_email` (`TenantId`,`Email`),
  KEY `CustomerTypeId` (`CustomerTypeId`),
  CONSTRAINT `customers_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `customers_ibfk_2` FOREIGN KEY (`CustomerTypeId`) REFERENCES `customertypes` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `customertypes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `departments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int DEFAULT NULL,
  `Name` varchar(100) NOT NULL,
  `Slug` varchar(100) NOT NULL,
  `IsSystemDefault` tinyint(1) DEFAULT '0',
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_department_name` (`TenantId`,`Name`),
  UNIQUE KEY `uk_tenant_department_slug` (`TenantId`,`Slug`),
  CONSTRAINT `departments_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `employees` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int NOT NULL,
  `Name` varchar(150) NOT NULL,
  `Cpf` char(11) NOT NULL,
  `Email` varchar(150) DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `HiredAt` datetime(6) NOT NULL,
  `DismissedAt` datetime(6) DEFAULT NULL,
  `Salary` decimal(12,2) DEFAULT NULL,
  `CommissionPercent` decimal(5,2) DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_cpf` (`TenantId`,`Cpf`),
  CONSTRAINT `employees_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `CK_Employee_CommissionPercent` CHECK (((`CommissionPercent` is null) or ((`CommissionPercent` >= 0) and (`CommissionPercent` <= 100))))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `employeeusers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int NOT NULL,
  `EmployeeId` int NOT NULL,
  `Username` varchar(100) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_username` (`TenantId`,`Username`),
  KEY `EmployeeId` (`EmployeeId`),
  CONSTRAINT `employeeusers_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `employeeusers_ibfk_2` FOREIGN KEY (`EmployeeId`) REFERENCES `employees` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `individuals` (
  `CustomerId` int NOT NULL,
  `TaxId` char(11) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `BirthDate` date DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`CustomerId`),
  CONSTRAINT `individuals_ibfk_1` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `orderaddresses` (
  `OrderId` int NOT NULL,
  `AddressTypeId` int NOT NULL DEFAULT '1',
  `Street` varchar(150) NOT NULL,
  `Number` varchar(10) NOT NULL,
  `Neighborhood` varchar(50) DEFAULT NULL,
  `City` varchar(50) NOT NULL,
  `State` char(2) NOT NULL,
  `ZipCode` char(8) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`OrderId`),
  KEY `AddressTypeId` (`AddressTypeId`),
  CONSTRAINT `orderaddresses_ibfk_1` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `orderaddresses_ibfk_2` FOREIGN KEY (`AddressTypeId`) REFERENCES `addresstypes` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `orderitems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderId` int NOT NULL,
  `ProductId` int NOT NULL,
  `Quantity` int NOT NULL,
  `UnitPrice` decimal(12,2) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `OrderId` (`OrderId`),
  KEY `ProductId` (`ProductId`),
  CONSTRAINT `orderitems_ibfk_1` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `orderitems_ibfk_2` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `orders` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int NOT NULL,
  `CustomerId` int NOT NULL,
  `OrderDate` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `TotalAmount` decimal(12,2) NOT NULL,
  `OrderStatusId` int NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `TenantId` (`TenantId`),
  KEY `CustomerId` (`CustomerId`),
  KEY `OrderStatusId` (`OrderStatusId`),
  CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `orders_ibfk_2` FOREIGN KEY (`CustomerId`) REFERENCES `customers` (`Id`),
  CONSTRAINT `orders_ibfk_3` FOREIGN KEY (`OrderStatusId`) REFERENCES `orderstatus` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `ordersectors` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int DEFAULT NULL,
  `Name` varchar(100) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_sector_name` (`TenantId`,`Name`),
  CONSTRAINT `ordersectors_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `orderstatehistory` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderId` int NOT NULL,
  `OrderStatusId` int NOT NULL,
  `Notes` varchar(255) DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `OrderStatusId` (`OrderStatusId`),
  KEY `idx_orderstatehistory_orderid` (`OrderId`),
  CONSTRAINT `orderstatehistory_ibfk_1` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `orderstatehistory_ibfk_2` FOREIGN KEY (`OrderStatusId`) REFERENCES `orderstatus` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `orderstatus` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int DEFAULT NULL,
  `OrderSectorId` int NOT NULL,
  `Name` varchar(50) NOT NULL,
  `IsSystemDefault` tinyint(1) DEFAULT '0',
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_status_name` (`TenantId`,`Name`),
  KEY `OrderSectorId` (`OrderSectorId`),
  CONSTRAINT `orderstatus_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `orderstatus_ibfk_2` FOREIGN KEY (`OrderSectorId`) REFERENCES `ordersectors` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `payments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderId` int NOT NULL,
  `PaymentMethod` varchar(50) NOT NULL,
  `Amount` decimal(12,2) NOT NULL,
  `PaymentStatusId` int NOT NULL DEFAULT '1',
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `OrderId` (`OrderId`),
  KEY `PaymentStatusId` (`PaymentStatusId`),
  CONSTRAINT `payments_ibfk_1` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `payments_ibfk_2` FOREIGN KEY (`PaymentStatusId`) REFERENCES `paymentstatus` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `paymentstatus` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `productimages` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `FileName` varchar(255) NOT NULL,
  `FilePath` varchar(500) NOT NULL,
  `IsPrimary` tinyint(1) DEFAULT '0',
  `DisplayOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `idx_productimages_productid` (`ProductId`),
  CONSTRAINT `productimages_ibfk_1` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `products` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int NOT NULL,
  `DepartmentId` int NOT NULL,
  `SKU` varchar(30) NOT NULL,
  `Name` varchar(150) NOT NULL,
  `Price` decimal(12,2) NOT NULL,
  `PhysicalStock` int NOT NULL DEFAULT '0',
  `ReservedStock` int NOT NULL DEFAULT '0',
  `AvailableStock` int GENERATED ALWAYS AS ((`PhysicalStock` - `ReservedStock`)) VIRTUAL,
  `Location_Aisle` varchar(10) DEFAULT NULL,
  `Location_Rack` varchar(10) DEFAULT NULL,
  `Location_Level` varchar(10) DEFAULT NULL,
  `Location_Position` varchar(10) DEFAULT NULL,
  `Version` int NOT NULL DEFAULT '1',
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_sku` (`TenantId`,`SKU`),
  KEY `DepartmentId` (`DepartmentId`),
  CONSTRAINT `products_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `products_ibfk_2` FOREIGN KEY (`DepartmentId`) REFERENCES `departments` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `profiles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int NOT NULL,
  `Name` varchar(100) NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_profile_name` (`TenantId`,`Name`),
  CONSTRAINT `profiles_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `profileusers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int NOT NULL,
  `ProfileId` int NOT NULL,
  `EmployeeUserId` int NOT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_profile_user` (`ProfileId`,`EmployeeUserId`),
  KEY `TenantId` (`TenantId`),
  KEY `EmployeeUserId` (`EmployeeUserId`),
  CONSTRAINT `profileusers_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `profileusers_ibfk_2` FOREIGN KEY (`ProfileId`) REFERENCES `profiles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `profileusers_ibfk_3` FOREIGN KEY (`EmployeeUserId`) REFERENCES `employeeusers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `stockmovements` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `MovementType` varchar(30) NOT NULL,
  `QuantityChanged` int NOT NULL,
  `BalanceAfterMovement` int NOT NULL,
  `Reason` varchar(150) NOT NULL,
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `ProductId` (`ProductId`),
  CONSTRAINT `stockmovements_ibfk_1` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `storebanners` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TenantId` int DEFAULT NULL,
  `Title` varchar(150) NOT NULL,
  `Subtitle` varchar(250) DEFAULT NULL,
  `DiscountPercentage` varchar(10) DEFAULT NULL,
  `ButtonText` varchar(50) NOT NULL DEFAULT 'Ver ofertas',
  `ButtonLink` varchar(250) NOT NULL DEFAULT '#',
  `BackgroundColor` varchar(30) NOT NULL DEFAULT '#f95d00',
  `DisplayOrder` int NOT NULL DEFAULT '0',
  `IsSystemDefault` tinyint(1) DEFAULT '0',
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_tenant_banner_order` (`TenantId`,`DisplayOrder`),
  CONSTRAINT `storebanners_ibfk_1` FOREIGN KEY (`TenantId`) REFERENCES `tenants` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `tenants` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CompanyName` varchar(150) NOT NULL,
  `Domain` varchar(100) DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT '1',
  `CreatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Domain` (`Domain`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

SET FOREIGN_KEY_CHECKS=1;
