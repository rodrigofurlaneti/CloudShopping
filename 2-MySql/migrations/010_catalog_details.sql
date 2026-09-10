ALTER TABLE products
 ADD COLUMN Slug varchar(150) NULL,
 ADD COLUMN Description text NOT NULL,
 ADD COLUMN Brand varchar(100) NULL,
 ADD COLUMN WeightKg decimal(10,3) NOT NULL DEFAULT 0,
 ADD COLUMN WidthCm decimal(10,2) NOT NULL DEFAULT 0,
 ADD COLUMN HeightCm decimal(10,2) NOT NULL DEFAULT 0,
 ADD COLUMN LengthCm decimal(10,2) NOT NULL DEFAULT 0,
 ADD COLUMN FamilyCode varchar(50) NULL,
 ADD COLUMN VariantLabel varchar(100) NULL,
 ADD COLUMN AttributesJson text NOT NULL;
UPDATE products SET Slug=CONCAT('produto-',Id),Description='',AttributesJson='{}';
ALTER TABLE products MODIFY Slug varchar(150) NOT NULL,
 ADD UNIQUE KEY ux_product_slug (TenantId,Slug),
 ADD UNIQUE KEY ux_product_variant (TenantId,FamilyCode,VariantLabel),
 ADD CONSTRAINT ck_product_dimensions CHECK (WeightKg>=0 AND WidthCm>=0 AND HeightCm>=0 AND LengthCm>=0);
