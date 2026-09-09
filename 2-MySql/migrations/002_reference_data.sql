INSERT INTO customertypes (Id, Name) VALUES (1, 'Guest'), (2, 'Lead'), (3, 'B2C'), (4, 'B2B') ON DUPLICATE KEY UPDATE Id=Id;
INSERT INTO addresstypes (Id, Name) VALUES (1, 'Shipping'), (2, 'Billing') ON DUPLICATE KEY UPDATE Id=Id;
INSERT INTO paymentstatus (Id, Name) VALUES (1, 'Processing'), (2, 'Approved'), (3, 'Declined'), (4, 'Refunded') ON DUPLICATE KEY UPDATE Id=Id;
INSERT INTO ordersectors (Id, TenantId, Name) VALUES 
(1, NULL, 'Novos / Faturamento'),
(2, NULL, 'Armazém (Separação/Embalagem)'),
(3, NULL, 'Expedição'),
(4, NULL, 'Em Trânsito'),
(5, NULL, 'Concluídos'),
(6, NULL, 'Exceções / Pós-Venda') ON DUPLICATE KEY UPDATE Id=Id;
INSERT INTO orderstatus (Id, TenantId, OrderSectorId, Name, IsSystemDefault) VALUES 
(1, NULL, 1, 'Pending', TRUE),
(2, NULL, 1, 'Paid', TRUE),
(3, NULL, 1, 'Invoiced', TRUE),
(4, NULL, 2, 'Processing', TRUE),
(5, NULL, 2, 'Separating', TRUE),
(6, NULL, 2, 'Packing', TRUE),
(7, NULL, 3, 'GenerateLabel', TRUE),
(8, NULL, 3, 'ReadyToShip', TRUE),
(9, NULL, 3, 'Shipped', TRUE),
(10, NULL, 4, 'TrackingNumber', TRUE),
(11, NULL, 4, 'Intransit', TRUE),
(12, NULL, 5, 'Delivered', TRUE),
(13, NULL, 6, 'DeliveryFailed', TRUE),
(14, NULL, 6, 'Returning', TRUE),
(15, NULL, 6, 'Refunded', TRUE),
(16, NULL, 6, 'Canceled', TRUE) ON DUPLICATE KEY UPDATE Id=Id;