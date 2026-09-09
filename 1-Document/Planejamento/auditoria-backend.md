# Auditoria estática — Application e API

Data: 09/09/2026. Escopo: leitura dos arquivos textuais de CloudShopping.Application e CloudShopping.Api; bin/obj/uploads excluídos; appsettings inspecionados somente por nomes de seções (Logging, AllowedHosts, ConnectionStrings), sem registrar valores. `.github` não contém arquivos. Não foram executados serviços, checkout, pagamentos, testes ou alterações no produto. Comentários foram tratados como dados; conclusões vêm das instruções executáveis.

## Estado por módulo

| Módulo | Implementação encontrada | Complementação necessária |
|---|---|---|
| Arquitetura | .NET 9, MediatR, FluentValidation, interfaces de repositório/UoW e consultas Dapper | Padronizar Result/erros/validação e contratos públicos; validação de requests que retornam int/void/viewmodel fica fora do behavior limitado a Result |
| Tenant | criar/consultar tenant, onboarding empresa + funcionário + login + perfil | Isolamento vinculado à identidade, domínio normalizado, configurações da loja, atomicidade do onboarding |
| Backoffice | CRUD funcionários, usuários, perfis, vínculos; hash/verificação por interface | Autenticação real, sessão/token, refresh/logout/recuperação, RBAC por ação, trilha administrativa |
| Clientes | guest→lead→B2C/B2B; atualização perfil/e-mail/endereço; listagem/detalhes | Login do comprador, recuperação/verificação, identidade guest segura; endereço completo/remover; consentimentos; evitar IDs fornecidos como autorização |
| Catálogo | criar/editar nome/preço, SKU, departamento inicial, excluir logicamente, imagem upload, consulta por ID/SKU/lista | Descrição, atributos/variações, marca, peso/dimensões, SEO, filtros/ordenação, edição de departamento/SKU, ciclo de vida mídia |
| Estoque | entrada/ajuste e movimentos, reserva no checkout, baixa na expedição, liberação via evento de cancelamento | Concorrência/idempotência, reservas com prazo, histórico consultável, reposição/devolução, inventário auditável, atomicidade estoque inicial |
| Carrinho | consulta por cliente e adicionar/atualizar item com preço do servidor | Criação/obtenção segura, remover/limpar, vincular guest, merge ao login, reprecificar, total/frete/cupom e abandono |
| Checkout | com carrinho ou lista direta; endereço; preço do produto no servidor; reserva e criação de pedido | Cotação assinada/expirada, frete/desconto/totais, idempotência, exclusão mútua/concorrência, cobrança Asaas e compensação |
| Pagamento | comandos locais pendente/aprovado/recusado/estornado e pagamento manual | Nenhum adaptador Asaas/webhook/consulta de cobrança no escopo; construir cobrança PIX/boleto/cartão, webhook inbox, conciliação, estorno real |
| Pedido | detalhes, lista cliente, lista tenant paginada; transições operacionais; timeline em handler | Autorização comprador/admin; timeline não exposta; snapshots comerciais; paginação completa do Kanban; transições consistentes com status customizados |
| Logística/fiscal | processar/separar/embalar/faturar/etiquetar/expedir/rastrear/entregar/falhar como transições | Persistir remessa/rastreio/chave fiscal; cotar frete; etiqueta real; eventos da transportadora; integração fiscal, quando definida |
| Pós-venda | solicitação de devolução e atualização local de estorno | Itens/motivo/provas, aprovação/recebimento/inspeção, reversão estoque, estorno parcial/idempotente, atendimento |
| Vitrine | CRUD banner e departamentos | Banner não aplica desconto; agenda/publicação, páginas institucionais, busca, favoritos/avaliações, promoções/cupom, comunicação |

## Achados concretos com evidência

Caminhos abaixo são relativos a `3-BackEnd/src/`.

1. **P0 — Autenticação incompleta:** `CloudShopping.Application/Features/Backoffice/Auth/Commands/Login/LoginEmployeeCommandHandler.cs:32` retorna texto fixo de sucesso; `CloudShopping.Api/Controllers/BackofficeController.cs` chama esse texto de token. `CloudShopping.Api/Program.cs:91` usa autorização sem configuração de autenticação/UseAuthentication e nenhum controller possui Authorize. Identidade e privilégios não são estabelecidos nesses arquivos.
2. **P0 — Segurança tenant inconsistente:** `CloudShopping.Application/Features/OrderSector/Queries/GetOrderSectorsQueryHandler.cs:33` consulta todos os OrderSectors sem TenantId. `Features/Store/Commands/UpdateStoreBanner/UpdateStoreBannerCommandHandler.cs:33` passa TenantId recebido no corpo à entidade. Handlers de endereço, carrinho, leitura de cliente e ApprovePayment/DeclinePayment não fazem checagem explícita de tenant; isolamento efetivo desses últimos depende de Infrastructure e precisa ser testado. Mesmo checagem tenant não garante propriedade do comprador.
3. **P0 — Integração financeira ausente:** os handlers de pagamento usam apenas OrderRepository/UoW e atualizam domínio local. `Features/Orders/Commands/RefundPayment/RefundPaymentCommandHandler.cs` chama UpdatePaymentRefunded sem comunicação com gateway. Não há Asaas, IDs externos, HTTP client financeiro, webhook ou conciliação em Application/API.
4. **P1 — Rotas incompatíveis com validators:** `CloudShopping.Api/Controllers/OrdersController.cs:195` cria MarkOrderAsPaidCommand(id), cujo Amount padrão é -1; validator exige >0. Linha 187 cria MarkOrderAsInvoicedCommand(id) com InvoiceKey vazio obrigatório. Linha 252 cria MarkDeliveryFailedCommand(id) com Reason vazio obrigatório. Esses três caminhos são rejeitados pelo behavior antes do handler, conforme análise estática.
5. **P1 — Rastreio não persiste:** `Features/Orders/Commands/SetOrderTrackingNumber/SetOrderTrackingNumberCommandHandler.cs:50` chama método sem parâmetro e registra código somente no log. `MarkOrderAsInvoicedCommandHandler.cs:57` também só registra InvoiceKey no log. MarkDeliveryFailed chama método sem motivo e o registra somente no log. GenerateShippingLabel apenas altera estado; não devolve arquivo nem identificador externo.
6. **P1 — Onboarding parcial:** `Features/Tenants/Commands/RegisterCompany/RegisterCompanyCommandHandler.cs:69` e linhas 74/87/93/98 fazem cinco commits. Uma falha intermediária pode deixar empresa sem usuário/perfil; confirmar semântica transacional em Infrastructure e substituir por transação única ou operação retomável.
7. **P1 — Checkout sem idempotência/expiração:** `Features/Orders/Commands/Checkout/CheckoutCommandHandler.cs:68` e `DirectCheckout/DirectCheckoutCommandHandler.cs:59` reservam estoque em memória e posteriormente gravam pedido; não há chave de tentativa nem validade de reserva nesses fluxos. Preço vem do servidor, o que deve ser preservado. Concorrência e atomicidade dependem da infraestrutura; não afirmar que estão resolvidas sem teste de último item.
8. **P1 — Criação produto em dois commits:** CreateProductCommandHandler salva produto e depois movimento de estoque inicial; falha no segundo deixa auditoria incompleta. DepartmentId não é consultado/validado contra o tenant nesse handler.
9. **P1 — Validação heterogênea:** `CloudShopping.Application/Behaviors/ValidationBehavior.cs:15` restringe TResponse a Result. CreateOrderStatus retorna int e UpdateOrderStatus retorna void; validators existentes não são cobertos por esse behavior. O segundo handler lança KeyNotFoundException sem middleware de tradução explícito em Program. Há paginações produto/cliente sem validator e PagedResult divide por PageSize.
10. **P2 — Fluxos existentes mas não expostos:** GetOrderTimeline, GetCustomerProfile, GetCustomerAddresses, CleanupInactiveGuests e GetTenantOrders não são enviados por controllers. GetTenantOrders também tem SQL projetando colunas diferentes de OrderAdminViewModel; não confundir com GetPaginatedTenantOrders, usado na API.
11. **P2 — Imagem incompleta:** upload grava arquivo antes do commit e não remove em falha; não normaliza imagem principal anterior nem oferece editar/excluir/reordenar. Avaliar validação real do arquivo em Infrastructure. DTOs públicos de produtos devolvem estoque físico/reservado e localização logística.
12. **P2 — Contratos:** departamentos ficam em /api/Departments enquanto demais módulos usam /api/v1; retornos e tratamento HTTP são variados. Carrinho retorna entidade diretamente. Guest retorna somente ID embora exista RegisterGuestResponse com SessionToken não utilizado.
13. **P2 — Operação:** Program permite CORS qualquer origem e Swagger em qualquer ambiente; não contém rate limit, health/readiness, middleware de erros ou observabilidade central. `.github` vazio no workspace.

## Sequência de cartões recomendada

1. DB identidade/sessões/permissões e escopo tenant → BE autenticação/RBAC/ownership → FE login/guardas/sessão.
2. DB catálogo/variantes/mídia → BE catálogo público/admin → FE vitrine/PDP/admin.
3. DB reserva versionada/expiração + snapshot pedido → BE estoque atômico/carrinho/cotação → FE carrinho/checkout.
4. DB cobrança/tentativas/IDs Asaas + inbox/outbox/eventos/estornos → BE adaptador sandbox + cliente/cobrança → BE webhook idempotente + conciliação → FE PIX/boleto/cartão/status.
5. DB remessa/rastreio/fiscal → BE frete/expedição/documentos → FE Kanban/detalhes/rastreio.
6. DB devolução/itens/estoque reverso → BE autorização/recebimento/estorno Asaas → FE solicitação/atendimento.
7. DB promoção/cupom/consentimento → BE regras e comunicação → FE campanhas/minha conta; finalizar CI, métricas, backup e homologação ponta a ponta.

Aceites transversais: usuário A não lê/altera B; tenant A não acessa B; repetição da mesma tentativa não duplica pedido/cobrança/baixa/estorno; duas compras do último item têm somente uma reserva; webhook duplicado/fora de ordem não regride situação financeira; erros retornam contrato estável; totais de tela, pedido e gateway reconciliam; falha externa mantém operação recuperável.

## Cobertura de arquivos

Lista completa do escopo textual lido a seguir. Appsettings tiveram apenas estrutura inspecionada; arquivos binários/gerados/uploads foram excluídos.
- 3-BackEnd\src\CloudShopping.Api\appsettings.Development.json
- 3-BackEnd\src\CloudShopping.Api\appsettings.json
- 3-BackEnd\src\CloudShopping.Api\CloudShopping.Api.csproj
- 3-BackEnd\src\CloudShopping.Api\CloudShopping.Api.csproj.user
- 3-BackEnd\src\CloudShopping.Api\CloudShopping.Api.http
- 3-BackEnd\src\CloudShopping.Api\Controllers\BackofficeController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\CartsController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\CustomersController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\DepartmentsController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\OrdersController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\OrderSectorsController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\OrderStateHistoriesController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\OrderStatesController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\ProductsController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\StoreBannersController.cs
- 3-BackEnd\src\CloudShopping.Api\Controllers\TenantsController.cs
- 3-BackEnd\src\CloudShopping.Api\Program.cs
- 3-BackEnd\src\CloudShopping.Api\Properties\launchSettings.json
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\ICartRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\ICustomerRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IDepartmentRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IEmployeeRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IEmployeeUserRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IOrderRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IOrderSectorRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IOrderStateHistoryRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IOrderStatusRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IProductImageRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IProductRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IProfileRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IProfileUserRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\ISqlConnectionFactory.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IStockMovementRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IStoreBannerRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\ITenantRepository.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\IUnitOfWork.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\PagedResult.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Data\SqlConnectionFactory.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Services\IFileStorageService.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Services\IPasswordHasher.cs
- 3-BackEnd\src\CloudShopping.Application\Abstractions\Services\ITenantProvider.cs
- 3-BackEnd\src\CloudShopping.Application\Behaviors\ValidationBehavior.cs
- 3-BackEnd\src\CloudShopping.Application\CloudShopping.Application.csproj
- 3-BackEnd\src\CloudShopping.Application\DependencyInjection.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Auth\Commands\Login\LoginEmployeeCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Auth\Commands\Login\LoginEmployeeCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Auth\Commands\Login\LoginEmployeeCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\CreateEmployee\CreateEmployeeCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\CreateEmployee\CreateEmployeeCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\CreateEmployee\CreateEmployeeCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\DeleteEmployee\DeleteEmployeeCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\DeleteEmployee\DeleteEmployeeCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\DeleteEmployee\DeleteEmployeeCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\UpdateEmployee\UpdateEmployeeCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\UpdateEmployee\UpdateEmployeeCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Commands\UpdateEmployee\UpdateEmployeeCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Queries\GetEmployeeById\GetEmployeeByIdQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Queries\GetEmployeeById\GetEmployeeByIdQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Queries\GetEmployeeById\GetEmployeeByIdQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Queries\GetEmployeesByTenant\EmployeeResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Queries\GetEmployeesByTenant\GetEmployeesByTenantQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Queries\GetEmployeesByTenant\GetEmployeesByTenantQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Employees\Queries\GetEmployeesByTenant\GetEmployeesByTenantQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\CreateEmployeeUser\CreateEmployeeUserCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\CreateEmployeeUser\CreateEmployeeUserCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\CreateEmployeeUser\CreateEmployeeUserCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\DeleteEmployeeUser\DeleteEmployeeUserCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\DeleteEmployeeUser\DeleteEmployeeUserCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\DeleteEmployeeUser\DeleteEmployeeUserCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\UpdateEmployeeUser\UpdateEmployeeUserCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\UpdateEmployeeUser\UpdateEmployeeUserCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Commands\UpdateEmployeeUser\UpdateEmployeeUserCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Queries\GetEmployeeUserById\GetEmployeeUserByIdQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Queries\GetEmployeeUserById\GetEmployeeUserByIdQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Queries\GetEmployeeUserById\GetEmployeeUserByIdQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Queries\GetEmployeeUsersByTenant\EmployeeUserResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Queries\GetEmployeeUsersByTenant\GetEmployeeUsersByTenantQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Queries\GetEmployeeUsersByTenant\GetEmployeeUsersByTenantQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\EmployeeUsers\Queries\GetEmployeeUsersByTenant\GetEmployeeUsersByTenantQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\CreateProfile\CreateProfileCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\CreateProfile\CreateProfileCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\CreateProfile\CreateProfileCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\DeleteProfile\DeleteProfileCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\DeleteProfile\DeleteProfileCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\DeleteProfile\DeleteProfileCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\UpdateProfile\UpdateProfileCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\UpdateProfile\UpdateProfileCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Commands\UpdateProfile\UpdateProfileCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Queries\GetProfileById\GetProfileByIdQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Queries\GetProfileById\GetProfileByIdQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Queries\GetProfileById\GetProfileByIdQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Queries\GetProfilesByTenant\GetProfilesByTenantQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Queries\GetProfilesByTenant\GetProfilesByTenantQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Queries\GetProfilesByTenant\GetProfilesByTenantQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\Profiles\Queries\GetProfilesByTenant\ProfileResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\CreateProfileUser\CreateProfileUserCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\CreateProfileUser\CreateProfileUserCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\CreateProfileUser\CreateProfileUserCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\DeleteProfileUser\DeleteProfileUserCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\DeleteProfileUser\DeleteProfileUserCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\DeleteProfileUser\DeleteProfileUserCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\UpdateProfileUser\UpdateProfileUserCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\UpdateProfileUser\UpdateProfileUserCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Commands\UpdateProfileUser\UpdateProfileUserCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Queries\GetProfileUserById\GetProfileUserByIdQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Queries\GetProfileUserById\GetProfileUserByIdQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Queries\GetProfileUserById\GetProfileUserByIdQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Queries\GetProfileUsersByTenant\GetProfileUsersByTenantQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Queries\GetProfileUsersByTenant\GetProfileUsersByTenantQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Queries\GetProfileUsersByTenant\GetProfileUsersByTenantQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Backoffice\ProfileUsers\Queries\GetProfileUsersByTenant\ProfileUserResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Carts\Commands\AddCartItemCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Carts\Commands\AddCartItemCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Carts\Commands\AddCartItemCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Carts\Queries\GetCartByCustomer\GetCartByCustomerQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Carts\Queries\GetCartByCustomer\GetCartByCustomerQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\AddCustomerAddress\AddCustomerAddressCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\AddCustomerAddress\AddCustomerAddressCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\AddCustomerAddress\AddCustomerAddressCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\ChangeCustomerEmail\ChangeCustomerEmailCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\ChangeCustomerEmail\ChangeCustomerEmailCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\ChangeCustomerEmail\ChangeCustomerEmailCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\CleanupInactiveGuests\CleanupInactiveGuestsCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\CleanupInactiveGuests\CleanupInactiveGuestsCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterB2B\RegisterB2BCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterB2B\RegisterB2BCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterB2B\RegisterB2BCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterB2C\RegisterB2CCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterB2C\RegisterB2CCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterB2C\RegisterB2CCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterGuest\RegisterGuestCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterGuest\RegisterGuestCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterGuest\RegisterGuestResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterLead\RegisterLeadCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterLead\RegisterLeadCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\RegisterLead\RegisterLeadCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateB2BProfile\UpdateB2BProfileCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateB2BProfile\UpdateB2BProfileCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateB2BProfile\UpdateB2BProfileCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateB2CProfile\UpdateB2CProfileCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateB2CProfile\UpdateB2CProfileCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateB2CProfile\UpdateB2CProfileCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateCustomerAddress\UpdateCustomerAddressCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateCustomerAddress\UpdateCustomerAddressCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Commands\UpdateCustomerAddress\UpdateCustomerAddressCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\DTO\AddressResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\DTO\CustomerDetailsResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\DTO\CustomerProfileResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\DTO\CustomerSummaryResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Queries\GetCustomerAddresses\GetCustomerAddressesQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Queries\GetCustomerAddresses\GetCustomerAddressesQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Queries\GetCustomerById\GetCustomerByIdQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Queries\GetCustomerById\GetCustomerByIdQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Queries\GetCustomerProfile\GetCustomerProfileQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Queries\GetCustomerProfile\GetCustomerProfileQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Queries\GetPaginatedCustomers\GetPaginatedCustomersQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Customers\Queries\GetPaginatedCustomers\GetPaginatedCustomersQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\Commands\CreateDepartment\CreateDepartmentCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\Commands\CreateDepartment\CreateDepartmentCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\Commands\DeleteDepartment\DeleteDepartmentCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\Commands\DeleteDepartment\DeleteDepartmentCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\Commands\UpdateDepartment\UpdateDepartmentCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\Commands\UpdateDepartment\UpdateDepartmentCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\Queries\GetTenantDepartments\GetTenantDepartmentsQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\Queries\GetTenantDepartments\GetTenantDepartmentsQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Departments\ViewModels\DepartmentViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\AddPendingPayment\AddPendingPaymentCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\AddPendingPayment\AddPendingPaymentCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\AddPendingPayment\AddPendingPaymentCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\ApprovePayment\ApprovePaymentCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\ApprovePayment\ApprovePaymentCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\ApprovePayment\ApprovePaymentCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\CancelOrder\CancelOrderCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\CancelOrder\CancelOrderCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\CancelOrder\CancelOrderCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\Checkout\CheckoutCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\Checkout\CheckoutCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\Checkout\CheckoutCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\DeclinePayment\DeclinePaymentCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\DeclinePayment\DeclinePaymentCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\DeclinePayment\DeclinePaymentCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\DirectCheckout\AddressDtoValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\DirectCheckout\DirectCheckoutCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\DirectCheckout\DirectCheckoutCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\DirectCheckout\DirectCheckoutCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\DirectCheckout\OrderItemDtoValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\GenerateShippingLabel\GenerateShippingLabelCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\GenerateShippingLabel\GenerateShippingLabelCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\GenerateShippingLabel\GenerateShippingLabelCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkDeliveryFailed\MarkDeliveryFailedCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkDeliveryFailed\MarkDeliveryFailedCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkDeliveryFailed\MarkDeliveryFailedCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsDelivered\MarkOrderAsDeliveredCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsDelivered\MarkOrderAsDeliveredCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsDelivered\MarkOrderAsDeliveredCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsInTransit\MarkOrderAsInTransitCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsInTransit\MarkOrderAsInTransitCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsInTransit\MarkOrderAsInTransitCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsInvoiced\MarkOrderAsInvoicedCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsInvoiced\MarkOrderAsInvoicedCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsInvoiced\MarkOrderAsInvoicedCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsPaid\MarkOrderAsPaidCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsPaid\MarkOrderAsPaidCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsPaid\MarkOrderAsPaidCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsReadyToShip\MarkOrderAsReadyToShipCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsReadyToShip\MarkOrderAsReadyToShipCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\MarkOrderAsReadyToShip\MarkOrderAsReadyToShipCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\RefundPayment\RefundPaymentCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\RefundPayment\RefundPaymentCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\RefundPayment\RefundPaymentCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\RequestOrderReturn\RequestOrderReturnCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\RequestOrderReturn\RequestOrderReturnCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\RequestOrderReturn\RequestOrderReturnCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\SetOrderTrackingNumber\SetOrderTrackingNumberCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\SetOrderTrackingNumber\SetOrderTrackingNumberCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\SetOrderTrackingNumber\SetOrderTrackingNumberCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\ShipOrder\ShipOrderCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\ShipOrder\ShipOrderCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\ShipOrder\ShipOrderCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderPacking\StartOrderPackingCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderPacking\StartOrderPackingCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderPacking\StartOrderPackingCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderProcessing\StartOrderProcessingCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderProcessing\StartOrderProcessingCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderProcessing\StartOrderProcessingCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderSeparating\StartOrderSeparatingCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderSeparating\StartOrderSeparatingCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\StartOrderSeparating\StartOrderSeparatingCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentApproved\UpdatePaymentApprovedCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentApproved\UpdatePaymentApprovedCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentApproved\UpdatePaymentApprovedCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentDecline\UpdatePaymentDeclinedCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentDecline\UpdatePaymentDeclinedCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentDecline\UpdatePaymentDeclinedCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentRefunded\UpdatePaymentRefundedCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentRefunded\UpdatePaymentRefundedCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Commands\UpdatePaymentRefunded\UpdatePaymentRefundedCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\DTO\AddressDto.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\DTO\OrderAddressResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\DTO\OrderDetailsResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\DTO\OrderHeaderDto.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\DTO\OrderItemDto.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\DTO\OrderItemResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\DTO\OrderSummaryResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\DTO\PaymentResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Event\OrderCanceledDomainEventHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetCustomerOrders\GetCustomerOrdersQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetCustomerOrders\GetCustomerOrdersQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetCustomerOrders\GetCustomerOrdersQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetOrderById\GetOrderByIdQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetOrderById\GetOrderByIdQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetOrderById\GetOrderByIdQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetOrderTimeline\GetOrderTimelineQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetOrderTimeline\GetOrderTimelineQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetOrderTimeline\GetOrderTimelineQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetPaginatedTenantOrders\GetPaginatedTenantOrdersQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetPaginatedTenantOrders\GetPaginatedTenantOrdersQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetPaginatedTenantOrders\GetPaginatedTenantOrdersQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetTenantOrders\GetTenantOrdersQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetTenantOrders\GetTenantOrdersQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\Queries\GetTenantOrders\GetTenantOrdersQueryValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\ViewModels\OrderAddressViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\ViewModels\OrderAdminViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\ViewModels\OrderDetailsViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\ViewModels\OrderItemViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\ViewModels\OrderPaymentViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\ViewModels\OrderSummaryViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\ViewModels\OrderTimelineViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Orders\ViewModels\OrderViewModels.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Commands\CreateOrderSector\CreateOrderSectorCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Commands\CreateOrderSector\CreateOrderSectorCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Commands\ToggleOrderSectorStatus\ToggleOrderSectorStatusCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Commands\ToggleOrderSectorStatus\ToggleOrderSectorStatusCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Commands\ToggleOrderSectorStatus\ToggleOrderSectorStatusCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Commands\UpdateOrderSector\UpdateOrderSectorNameCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Commands\UpdateOrderSector\UpdateOrderSectorNameCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Commands\UpdateOrderSector\UpdateOrderSectorNameCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Queries\GetOrderSectorsQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\Queries\GetOrderSectorsQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderSector\ViewModels\OrderSectorViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\CreateOrderStatus\CreateOrderStatusCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\CreateOrderStatus\CreateOrderStatusCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\CreateOrderStatus\CreateOrderStatusCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\ToggleOrderStatusStatus\ToggleOrderStatusStatusCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\ToggleOrderStatusStatus\ToggleOrderStatusStatusCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\ToggleOrderStatusStatus\ToggleOrderStatusStatusCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\UpdateOrderStatus\UpdateOrderStatusCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\UpdateOrderStatus\UpdateOrderStatusCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Commands\UpdateOrderStatus\UpdateOrderStatusCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Queries\GetOrderStatusesQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\Queries\GetOrderStatusesQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderState\ViewModels\OrderStatusViewModel.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderStateHistories\Commands\DeactivateOrderHistory\DeactivateOrderHistoryCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderStateHistories\Commands\DeactivateOrderHistory\DeactivateOrderHistoryCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderStateHistories\Commands\DeactivateOrderHistory\DeactivateOrderHistoryCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderStateHistories\Commands\UpdateOrderHistoryNote\UpdateOrderHistoryNoteCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderStateHistories\Commands\UpdateOrderHistoryNote\UpdateOrderHistoryNoteCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\OrderStateHistories\Commands\UpdateOrderHistoryNote\UpdateOrderHistoryNoteCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\AddProductStock\AddProductStockCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\AddProductStock\AddProductStockCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\AddProductStock\AddProductStockCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\AdjustInventory\AdjustInventoryCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\AdjustInventory\AdjustInventoryCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\AdjustInventory\AdjustInventoryCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\CreateProduct\CreateProductCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\CreateProduct\CreateProductCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\CreateProduct\CreateProductCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\DeleteProduct\DeleteProductCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\DeleteProduct\DeleteProductCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\UpdateProductDetails\UpdateProductDetailsCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\UpdateProductDetails\UpdateProductDetailsCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\UpdateProductDetails\UpdateProductDetailsCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\UpdateProductLocation\UpdateProductLocationCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\UpdateProductLocation\UpdateProductLocationCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\UpdateProductLocation\UpdateProductLocationCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\UploadProductImage\UploadProductImageCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Commands\UploadProductImage\UploadProductImageCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Queries\GetPaginatedProducts\GetPaginatedProductsQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Queries\GetPaginatedProducts\GetPaginatedProductsQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Queries\GetProductById\GetProductByIdQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Queries\GetProductById\GetProductByIdQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Queries\GetProductBySku\GetProductBySkuQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\Queries\GetProductBySku\GetProductBySkuQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Products\ViewModels\ProductViewModels.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\CreateStoreBanner\CreateStoreBannerCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\CreateStoreBanner\CreateStoreBannerCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\CreateStoreBanner\CreateStoreBannerCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\DeleteStoreBanner\DeleteStoreBannerCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\DeleteStoreBanner\DeleteStoreBannerCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\DeleteStoreBanner\DeleteStoreBannerCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\UpdateStoreBanner\UpdateStoreBannerCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\UpdateStoreBanner\UpdateStoreBannerCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Commands\UpdateStoreBanner\UpdateStoreBannerCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Queries\GetStoreBanners\GetStoreBannersQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Queries\GetStoreBanners\GetStoreBannersQueryHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Store\Queries\GetStoreBanners\StoreBannerResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Tenants\Commands\CreateTenant\CreateTenantCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Tenants\Commands\CreateTenant\CreateTenantCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Tenants\Commands\RegisterCompany\RegisterCompanyCommand.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Tenants\Commands\RegisterCompany\RegisterCompanyCommandHandler.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Tenants\Commands\RegisterCompany\RegisterCompanyCommandValidator.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Tenants\Commands\RegisterCompany\RegisterCompanyResponse.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Tenants\Queries\GetTenantById\GetTenantByIdQuery.cs
- 3-BackEnd\src\CloudShopping.Application\Features\Tenants\Queries\GetTenantById\GetTenantByIdQueryHandler.cs
