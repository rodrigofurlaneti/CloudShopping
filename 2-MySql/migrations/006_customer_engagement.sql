CREATE TABLE wishlistitems (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, CustomerId int NOT NULL, ProductId int NOT NULL, CreatedAt datetime(6) NOT NULL,
 UNIQUE KEY ux_wishlist (TenantId,CustomerId,ProductId),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), FOREIGN KEY (CustomerId) REFERENCES customers(Id), FOREIGN KEY (ProductId) REFERENCES products(Id)
);
CREATE TABLE productreviews (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, CustomerId int NOT NULL, ProductId int NOT NULL, OrderId int NOT NULL,
 Rating int NOT NULL, Content varchar(2000) NOT NULL, State varchar(30) NOT NULL, ModerationReason varchar(1000) NOT NULL,
 CreatedAt datetime(6) NOT NULL, Version int NOT NULL DEFAULT 1,
 UNIQUE KEY ux_review_author (TenantId,CustomerId,ProductId), CHECK (Rating BETWEEN 1 AND 5),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), FOREIGN KEY (CustomerId) REFERENCES customers(Id),
 FOREIGN KEY (ProductId) REFERENCES products(Id), FOREIGN KEY (OrderId) REFERENCES orders(Id), INDEX ix_review_public (TenantId,ProductId,State,CreatedAt)
);
CREATE TABLE reviewdecisions (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, ReviewId char(32) NOT NULL,
 State varchar(30) NOT NULL, Reason varchar(1000) NOT NULL, Actor varchar(100) NOT NULL, CreatedAt datetime(6) NOT NULL,
 FOREIGN KEY (ReviewId) REFERENCES productreviews(Id), FOREIGN KEY (TenantId) REFERENCES tenants(Id)
);
CREATE TABLE supporttickets (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, CustomerId int NOT NULL, OrderId int NULL,
 Subject varchar(150) NOT NULL, Category varchar(30) NOT NULL, State varchar(30) NOT NULL,
 CreatedAt datetime(6) NOT NULL, Version int NOT NULL DEFAULT 1,
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), FOREIGN KEY (CustomerId) REFERENCES customers(Id),
 FOREIGN KEY (OrderId) REFERENCES orders(Id), INDEX ix_support_customer (TenantId,CustomerId,CreatedAt)
);
CREATE TABLE supportmessages (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, TicketId char(32) NOT NULL,
 Content varchar(4000) NOT NULL, Sender varchar(30) NOT NULL, OperationKey varchar(100) NOT NULL, CreatedAt datetime(6) NOT NULL,
 UNIQUE KEY ux_support_message (TicketId,OperationKey), FOREIGN KEY (TicketId) REFERENCES supporttickets(Id),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id)
);
