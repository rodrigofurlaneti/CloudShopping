CREATE TABLE asaasconnections (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, Mode varchar(20) NOT NULL,
 Environment varchar(20) NOT NULL, WalletId varchar(80) NOT NULL, AccountKey varchar(110) NOT NULL,
 ProtectedApiKey text NOT NULL, WebhookTokenHash char(64) NOT NULL,
 Enabled boolean NOT NULL, CreatedAt datetime(6) NOT NULL,
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), KEY IX_connection_tenant(TenantId,Enabled)
);
CREATE TABLE asaascustomers (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, CustomerId int NOT NULL,
 AccountKey varchar(110) NOT NULL, RemoteId varchar(100) NULL, RequestSent boolean NOT NULL,
 UNIQUE KEY UX_asaas_customer(TenantId,CustomerId,AccountKey),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), FOREIGN KEY (CustomerId) REFERENCES customers(Id)
);
CREATE TABLE paymentattempts (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, OrderId int NOT NULL, CustomerId int NOT NULL,
 ConnectionId char(32) NOT NULL, AccountKey varchar(110) NOT NULL, Method varchar(20) NOT NULL,
 Amount decimal(12,2) NOT NULL, SplitJson text NOT NULL, DueDate datetime(6) NOT NULL,
 RemoteCustomerId varchar(100) NULL, RemotePaymentId varchar(100) NULL, RemoteCheckoutId varchar(100) NULL,
 State varchar(30) NOT NULL, ProviderStatus varchar(60) NOT NULL, CreateSent boolean NOT NULL, CreationRejected boolean NOT NULL,
 CancelRequested boolean NOT NULL, CancelSent boolean NOT NULL, RefundRequested boolean NOT NULL, RefundSent boolean NOT NULL,
 RefundObserved boolean NOT NULL, RefundRequestUrl text NULL,
 PaymentUrl text NULL, PixPayload text NULL, PixImage mediumtext NULL, BankSlipUrl text NULL,
 LastError varchar(500) NULL, CreatedAt datetime(6) NOT NULL, NextCheckAt datetime(6) NOT NULL,
 Version int NOT NULL DEFAULT 1,
 UNIQUE KEY UX_payment_order(OrderId), UNIQUE KEY UX_remote_payment(AccountKey,RemotePaymentId),
 UNIQUE KEY UX_remote_checkout(AccountKey,RemoteCheckoutId), KEY IX_payment_work(NextCheckAt,State),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), FOREIGN KEY (OrderId) REFERENCES orders(Id),
 FOREIGN KEY (CustomerId) REFERENCES customers(Id), FOREIGN KEY (ConnectionId) REFERENCES asaasconnections(Id)
);
CREATE TABLE asaasinbox (
 Id bigint NOT NULL AUTO_INCREMENT PRIMARY KEY, AccountKey varchar(110) NOT NULL,
 EventId varchar(150) NOT NULL, EventType varchar(80) NOT NULL, ProtectedPayload mediumtext NOT NULL,
 ReceivedAt datetime(6) NOT NULL, ProcessedAt datetime(6) NULL, Attempts int NOT NULL DEFAULT 0,
 NextCheckAt datetime(6) NOT NULL, LastError varchar(500) NULL,
 UNIQUE KEY UX_asaas_event(AccountKey,EventId), KEY IX_inbox_work(ProcessedAt,NextCheckAt)
);
CREATE TABLE paymentoperations (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, OrderId int NOT NULL, AttemptId char(32) NOT NULL,
 Kind varchar(20) NOT NULL, RequestedBy varchar(100) NOT NULL, RequestedAt datetime(6) NOT NULL,
 UNIQUE KEY UX_operation(AttemptId,Kind), FOREIGN KEY (TenantId) REFERENCES tenants(Id),
 FOREIGN KEY (OrderId) REFERENCES orders(Id), FOREIGN KEY (AttemptId) REFERENCES paymentattempts(Id)
);
ALTER TABLE orders ADD COLUMN FinancialState varchar(30) NOT NULL DEFAULT 'Unpaid',
 ADD COLUMN FulfillmentBlocked boolean NOT NULL DEFAULT false;
ALTER TABLE payments ADD COLUMN ProviderKey varchar(220) NULL,
 ADD UNIQUE KEY UX_payment_provider(ProviderKey);
