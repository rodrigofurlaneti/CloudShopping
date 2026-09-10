ALTER TABLE orders ADD COLUMN DiscountAmount decimal(12,2) NOT NULL DEFAULT 0, ADD COLUMN CouponCode varchar(40) NULL;
CREATE TABLE coupons (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, Code varchar(40) NOT NULL, Kind varchar(20) NOT NULL,
 Value decimal(12,2) NOT NULL, MinimumSubtotal decimal(12,2) NOT NULL, UsageLimit int NOT NULL, PerCustomerLimit int NOT NULL,
 UsedCount int NOT NULL DEFAULT 0, StartsAt datetime(6) NOT NULL, EndsAt datetime(6) NOT NULL, Enabled tinyint(1) NOT NULL DEFAULT 1,
 Version int NOT NULL DEFAULT 1, UNIQUE KEY ux_coupon_code (TenantId,Code),
 CHECK (Value>0 AND MinimumSubtotal>=0 AND UsageLimit>0 AND PerCustomerLimit>0 AND UsedCount>=0 AND UsedCount<=UsageLimit),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id)
);
CREATE TABLE couponredemptions (
 Id char(32) PRIMARY KEY, TenantId int NOT NULL, CouponId char(32) NOT NULL, OrderId int NOT NULL, CustomerId int NOT NULL,
 DiscountAmount decimal(12,2) NOT NULL, Released tinyint(1) NOT NULL DEFAULT 0,
 UNIQUE KEY ux_coupon_order (OrderId), INDEX ix_coupon_customer (CouponId,CustomerId,Released),
 FOREIGN KEY (TenantId) REFERENCES tenants(Id), FOREIGN KEY (CouponId) REFERENCES coupons(Id),
 FOREIGN KEY (OrderId) REFERENCES orders(Id), FOREIGN KEY (CustomerId) REFERENCES customers(Id)
);
