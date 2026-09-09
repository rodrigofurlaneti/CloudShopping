-- Refuse inconsistent legacy data; review duplicates before applying to an existing store.
ALTER TABLE cartitems ADD UNIQUE KEY IX_cartitems_CartId_ProductId (CartId,ProductId);
ALTER TABLE cartitems ADD CONSTRAINT CK_cartitems_quantity CHECK (Quantity > 0);
