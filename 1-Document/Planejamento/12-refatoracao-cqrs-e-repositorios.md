# Refatoração para o padrão original — situação em 10/09/2026

## Padrão adotado

- API: contratos HTTP, autenticação, status e envio pelo MediatR.
- Application: Features/Módulo/Commands/Operação e Queries/Operação, com request, handler e validator em arquivos separados; comandos com Result<T>.
- Domain: entidades com Entity<TId>, construtor privado, factories e métodos de alteração. Regras e exceções de negócio independentes de infraestrutura.
- Application/Abstractions/Data: contratos de persistência; Infrastructure/Repositories: EF/Dapper e implementação dos contratos; IUnitOfWork para persistir alterações.
- Consultas não criam carrinho nem alteram dados. Dados de outros tenants não podem ser consultados ou alterados.
- CRUD de cadastros deve ser completado por módulo. Eventos e auditoria não recebem atualização/exclusão arbitrárias; cupons são desativados para preservar utilizações históricas.

## Alterações implementadas neste conjunto

| Módulo | Alteração | Limite atual |
|---|---|---|
| Acesso | Controller via MediatR, commands/queries, IAccessRepository, entidades ProfilePermission e AccessChange no domínio | Contratos de leitura e edição agrupados ainda podem ser separados; auditoria permanece somente inclusão/consulta |
| Catálogo detalhado | Command/handler/validator; query tipada; contrato IProductRepository ampliado; UnitOfWork | Completar inventário dos CRUDs do cadastro original |
| Vitrine | Commands e queries separados; controller sem EF; IStorefrontRepository e DTOs | StoreCommerceService ainda contém parte da orquestração do checkout na infraestrutura |
| Consultas de pedidos, setores e estados | SQL transferido para OrderReadRepository e OrderWorkflowReadRepository | Revisar operações de escrita restantes |
| Cupons | Entidades Coupon e CouponRedemption no domínio; ICouponRepository com contrato base; handlers de criação e ativação; CouponCheckoutService na aplicação | Interface de resgates tem operações específicas para manter histórico; faltam operações adicionais de cadastro previstas no inventário geral |
| Relatórios | Queries e DTOs tipados; IReportRepository; montagem CSV e validação de período na aplicação | Sem CRUD: projeção de dados existentes |
| Notificações | Controllers via MediatR; entidades encapsuladas no domínio; INotificationRepository | Worker de entrega ainda precisa separar orquestração e persistência |
| Sessões e segurança | Commands/queries via MediatR; AuthSession encapsulada no domínio; IAccountSecurityRepository e transações na infraestrutura | Regras compartilhadas ainda agrupadas em serviços da aplicação; revisar granularidade dos contratos antigos |
| Dependências | Application sem pacotes Dapper e MySqlConnector; SqlConnectionFactory na infraestrutura | Revisar todos os adaptadores e serviços restantes |

## Pendências obrigatórias — não considerar a refatoração concluída

1. Revisar os contratos de sessão antigos (ISessionStore/ISessionAccounts) junto aos novos contratos, evitando sobreposição. AccountSecurityController já não depende da infraestrutura.
2. SessionController já usa MediatR; concluir a revisão da granularidade de SessionUseCases e AccountSecurity, que ainda agrupam regras compartilhadas na aplicação.
3. AsaasController e serviços de pagamento: separar casos de uso, gateway externo, repositórios e processamento de eventos, preservando locks, idempotência e transações.
4. CatalogImportsController: separar leitura de arquivo, validação de negócio, staging e aplicação do lote.
5. EngagementController: separar favoritos, avaliações, moderação e atendimento, com entidades encapsuladas e contratos próprios.
6. OperationsController: separar expedição, devoluções, eventos e ações financeiras.
7. StoreCommerceService: completar extração de regras e orquestração da infraestrutura; interfaces intermediárias não encerram essa pendência.
8. Inventariar cada entidade existente e verificar create/read/update/delete ou operação de negócio equivalente, interface completa, implementação, registro DI, validator e handler. O inventário total ainda não está concluído.
9. Completar factories/métodos das entidades ainda declaradas em Infrastructure e centralizar regras que hoje dependem de DbContext.
10. Separar mapas de autorização por controller/action, atualmente em AccessRequirements na aplicação, para a camada HTTP.

## Verificações

- Build da API em saída isolada, sem parar o processo do Visual Studio.
- 70 testes passaram após relatórios/notificações; posteriormente, 14 testes direcionados passaram após a mudança de segurança/sessões.
- Regressão em MySQL isolado 127.0.0.1:33077, com schemas temporários de teste.
- Smoke HTTP: autenticação, CSRF, isolamento entre lojas, checkout, repetição, cancelamento, permissões, revogação e troca de senha.
- Testes adicionados: aplicação sem dependências de banco, proteção das entidades de acesso e consulta de carrinho sem escrita.
- Não foram aplicadas alterações ao banco ecommercedb do usuário.

A conclusão de testes não representa conclusão de todos os módulos ou da arquitetura. Os itens pendentes acima permanecem necessários para atender ao pedido de refazer todo o projeto.
