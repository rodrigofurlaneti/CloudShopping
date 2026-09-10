# Inventário de contratos de entidades do domínio

Levantamento estático em 10/09/2026. Não certifica cobertura de CRUD, regras de negócio ou autorização. A ausência de repositório direto pode indicar uma entidade filha persistida pelo agregado; exige revisão individual. Entidades ainda declaradas na infraestrutura estão fora desta tabela e continuam na pendência arquitetural.

| Entidade | Identificador | Contrato IRepository direto | Outros contratos que mencionam a entidade |
|---|---|---|---|
| AccessChange | string | Não identificado | Verificar acesso pelo agregado |
| Employee | int | IEmployeeRepository | Verificar acesso pelo agregado |
| EmployeeUser | int | IEmployeeUserRepository | IAccountSecurityRepository, ISessionAccounts |
| Profile | int | IProfileRepository | IAccessRepository |
| ProfilePermission | int | Não identificado | Verificar acesso pelo agregado |
| ProfileUser | int | IProfileUserRepository | Verificar acesso pelo agregado |
| CartItem | int | Não identificado | Verificar acesso pelo agregado |
| Address | int | Não identificado | IStorefrontRepository |
| Company | int | Não identificado | Verificar acesso pelo agregado |
| Contact | int | Não identificado | Verificar acesso pelo agregado |
| Individual | int | Não identificado | Verificar acesso pelo agregado |
| CommerceOutbox | string | Não identificado | INotificationRepository |
| CustomerNotification | string | Não identificado | INotificationRepository |
| OrderAddress | int | Não identificado | Verificar acesso pelo agregado |
| OrderItem | int | Não identificado | Verificar acesso pelo agregado |
| OrderSector | int | IOrderSectorRepository | IOrderWorkflowReadRepository |
| OrderStateHistory | int | IOrderStateHistoryRepository | Verificar acesso pelo agregado |
| OrderStatus | int | IOrderStatusRepository | Verificar acesso pelo agregado |
| Payment | int | Não identificado | Verificar acesso pelo agregado |
| ProductImage | int | IProductImageRepository | Verificar acesso pelo agregado |
| StockMovement | int | IStockMovementRepository | Verificar acesso pelo agregado |
| Coupon | string | ICouponRepository | Verificar acesso pelo agregado |
| CouponRedemption | string | Não identificado | ICouponRedemptionRepository |
| AuthSession | string | IAccountSecurityRepository | Verificar acesso pelo agregado |
| StoreBanner | int | IStoreBannerRepository | Verificar acesso pelo agregado |
