CREATE TABLE commerceoutbox (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, OrderId int NOT NULL, OrderVersion int NOT NULL,
 Kind varchar(60) NOT NULL, CreatedAt datetime(6) NOT NULL, AvailableAt datetime(6) NOT NULL,
 ProcessedAt datetime(6) NULL, Attempts int NOT NULL DEFAULT 0, State varchar(20) NOT NULL DEFAULT 'Pending',
 LastError varchar(200) NULL,
 UNIQUE KEY ux_outbox_event (OrderId,OrderVersion,Kind), INDEX ix_outbox_claim (State,AvailableAt),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), FOREIGN KEY (OrderId) REFERENCES orders(Id)
);
CREATE TABLE customernotifications (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, CustomerId int NOT NULL, OrderId int NOT NULL,
 EventId char(32) NOT NULL, Kind varchar(60) NOT NULL, CreatedAt datetime(6) NOT NULL, ReadAt datetime(6) NULL,
 UNIQUE KEY ux_notification_event (EventId), INDEX ix_notification_customer (TenantId,CustomerId,CreatedAt),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), FOREIGN KEY (CustomerId) REFERENCES customers(Id),
 FOREIGN KEY (OrderId) REFERENCES orders(Id), FOREIGN KEY (EventId) REFERENCES commerceoutbox(Id)
);
