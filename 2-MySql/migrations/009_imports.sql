CREATE TABLE catalogimports (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, ContentHash char(64) NOT NULL, State varchar(30) NOT NULL,
 Actor varchar(100) NOT NULL, CreatedAt datetime(6) NOT NULL,
 UNIQUE KEY ux_import_hash (TenantId,ContentHash), INDEX ix_import_queue (State,CreatedAt),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id)
);
CREATE TABLE catalogimportrows (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, ImportId char(32) NOT NULL, LineNumber int NOT NULL,
 Sku varchar(30) NOT NULL, Name varchar(150) NOT NULL, DepartmentId int NOT NULL,
 Price decimal(12,2) NOT NULL, PhysicalStock int NOT NULL, ExpectedProductId int NULL, ExpectedVersion int NULL,
 State varchar(30) NOT NULL, Error varchar(300) NOT NULL,
 UNIQUE KEY ux_import_line (ImportId,LineNumber), FOREIGN KEY (ImportId) REFERENCES catalogimports(Id), FOREIGN KEY (TenantId) REFERENCES tenants(Id)
);
