-- Apply once through the migration runner. Existing orders retain original totals.
ALTER TABLE carts ADD COLUMN Version int NOT NULL DEFAULT 1;
ALTER TABLE orders
 ADD COLUMN ShippingAmount decimal(12,2) NOT NULL DEFAULT 0,
 ADD COLUMN ShippingMethod varchar(100) NULL,
 ADD COLUMN CheckoutKey varchar(64) NULL,
 ADD COLUMN CheckoutHash varchar(64) NULL,
 ADD COLUMN ReservationExpiresAt datetime(6) NULL,
 ADD COLUMN ReservationState varchar(20) NOT NULL DEFAULT 'None',
 ADD COLUMN Version int NOT NULL DEFAULT 1,
 ADD UNIQUE KEY UX_checkout (TenantId,CustomerId,CheckoutKey),
 ADD KEY IX_reservation_expiry (ReservationState,ReservationExpiresAt);
ALTER TABLE orderitems ADD COLUMN ProductName varchar(150) NULL, ADD COLUMN Sku varchar(30) NULL;
CREATE TABLE authsessions (
 Id char(32) NOT NULL PRIMARY KEY, TenantId int NOT NULL, SubjectId int NOT NULL,
 Kind varchar(20) NOT NULL, CredentialStamp varchar(64) NOT NULL,
 ExpiresAt datetime(6) NOT NULL, RevokedAt datetime(6) NULL,
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), KEY IX_session_expiry(ExpiresAt)
);
CREATE TABLE shippingoptions (
 Id int NOT NULL AUTO_INCREMENT PRIMARY KEY, TenantId int NOT NULL,
 Name varchar(100) NOT NULL, Amount decimal(12,2) NOT NULL,
 PostalCodePrefix varchar(8) NOT NULL DEFAULT '', EstimatedDays int NOT NULL DEFAULT 0,
 IsActive boolean NOT NULL DEFAULT true,
 FOREIGN KEY (TenantId) REFERENCES tenants(Id),
 CHECK (Amount >= 0), CHECK (EstimatedDays >= 0)
);

