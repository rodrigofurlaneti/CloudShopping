ALTER TABLE orders ADD COLUMN FulfillmentState varchar(30) NOT NULL DEFAULT 'Unstarted';
CREATE TABLE operationevents (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, OrderId int NOT NULL,
 OperationKey varchar(100) NOT NULL, RequestHash char(64) NOT NULL,
 Kind varchar(40) NOT NULL, PreviousState varchar(30) NOT NULL, NewState varchar(30) NOT NULL,
 Actor varchar(100) NOT NULL, Notes varchar(1000) NOT NULL, CreatedAt datetime(6) NOT NULL,
 UNIQUE KEY ux_operation (TenantId,OrderId,OperationKey),
 FOREIGN KEY (OrderId) REFERENCES orders(Id), FOREIGN KEY (TenantId) REFERENCES tenants(Id)
);
CREATE TABLE shipments (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, OrderId int NOT NULL,
 Carrier varchar(100) NOT NULL, Service varchar(100) NOT NULL, TrackingCode varchar(120) NOT NULL,
 Volumes int NOT NULL, State varchar(30) NOT NULL, CreatedAt datetime(6) NOT NULL,
 FOREIGN KEY (OrderId) REFERENCES orders(Id), FOREIGN KEY (TenantId) REFERENCES tenants(Id),
 CHECK (Volumes>0), INDEX ix_shipment_order (TenantId,OrderId)
);
CREATE TABLE shipmentitems (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, ShipmentId char(32) NOT NULL,
 OrderItemId int NOT NULL, Quantity int NOT NULL, CHECK (Quantity>0),
 UNIQUE KEY ux_shipment_item (ShipmentId,OrderItemId),
 FOREIGN KEY (ShipmentId) REFERENCES shipments(Id), FOREIGN KEY (OrderItemId) REFERENCES orderitems(Id),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id)
);
CREATE TABLE returncases (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, OrderId int NOT NULL, CustomerId int NOT NULL,
 Reason varchar(1000) NOT NULL, State varchar(30) NOT NULL, Decision varchar(1000) NOT NULL,
 CreatedAt datetime(6) NOT NULL,
 FOREIGN KEY (OrderId) REFERENCES orders(Id), FOREIGN KEY (CustomerId) REFERENCES customers(Id),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), INDEX ix_return_order (TenantId,OrderId)
);
CREATE TABLE returnitems (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, ReturnCaseId char(32) NOT NULL,
 OrderItemId int NOT NULL, Quantity int NOT NULL, RestockedQuantity int NOT NULL DEFAULT 0,
 UNIQUE KEY ux_return_item (ReturnCaseId,OrderItemId), CHECK (Quantity>0),
 CHECK (RestockedQuantity>=0 AND RestockedQuantity<=Quantity),
 FOREIGN KEY (ReturnCaseId) REFERENCES returncases(Id), FOREIGN KEY (OrderItemId) REFERENCES orderitems(Id),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id)
);
