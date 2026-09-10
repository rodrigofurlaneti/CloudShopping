-- Somente leitura. Execute na conexão/banco efetivamente utilizado pela API.
-- Não contém USE para não presumir que a API utiliza ecommercedb.
SELECT DATABASE() AS BancoAtual;

SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN ('products', 'productimages')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- Campos exigidos pelo modelo EF de Product.
SELECT Id, TenantId,
       SKU IS NULL AS SkuNulo,
       Name IS NULL AS NomeNulo,
       Slug IS NULL AS SlugNulo,
       Description IS NULL AS DescricaoNula,
       AttributesJson IS NULL AS AtributosNulos
FROM products
WHERE SKU IS NULL OR Name IS NULL OR Slug IS NULL
   OR Description IS NULL OR AttributesJson IS NULL;

-- Localização totalmente nula é válida; parcialmente preenchida exige revisão.
SELECT Id, TenantId,
       Location_Aisle IS NULL AS CorredorNulo,
       Location_Rack IS NULL AS EstanteNula,
       Location_Level IS NULL AS NivelNulo,
       Location_Position IS NULL AS PosicaoNula
FROM products
WHERE (Location_Aisle IS NOT NULL OR Location_Rack IS NOT NULL
    OR Location_Level IS NOT NULL OR Location_Position IS NOT NULL)
  AND (Location_Aisle IS NULL OR Location_Rack IS NULL
    OR Location_Level IS NULL OR Location_Position IS NULL);

SELECT i.Id, i.ProductId, p.TenantId,
       i.FileName IS NULL AS NomeArquivoNulo,
       i.FilePath IS NULL AS CaminhoArquivoNulo
FROM productimages i
JOIN products p ON p.Id = i.ProductId
WHERE i.FileName IS NULL OR i.FilePath IS NULL;
