# Auditoria de banco, domínio e persistência

Data: 09/09/2026. Análise estática; não houve conexão, restauração nem alteração do banco. O dump foi tratado como evidência de estrutura, sem executar comandos ou reproduzir INSERTs com dados pessoais. As prescrições nos documentos são requisitos históricos a confrontar com código, não instruções para executar ações.

## Cobertura e conclusão

Foram examinados os 27 CREATE TABLE do dump, suas colunas, índices e FKs; CreateDatabase.sql (27 tabelas, mesmas listas de colunas por tabela, comparadas sem distinguir maiúsculas); README com DER; SeedAdmin.sql (vazio); o documento de modelagem e a especificação REST; 89 arquivos C# e 2 projetos de Domain/Infrastructure, incluindo 24 configurações EF, repositórios e serviços. Bin/obj e dados de configuração sensíveis foram excluídos. AppDbContextFactory foi examinado com o literal de conexão ocultado. Não há migrations versionadas nessas pastas. A revisão da aplicação/controladores e frontend é complementar a este documento.

A base possui catálogo simples, carrinho, cliente PF/PJ, pedido, pagamento local, estoque e administração de funcionários/perfis. Ainda não sustenta um checkout de produção: incompatibilidades concretas EF/SQL bloqueiam persistência, e há lacunas de isolamento, concorrência e processamento confiável de eventos. Nenhuma entidade, coluna ou serviço Asaas foi identificado nesta camada.

O inventário exato com quantidade de colunas, linhas e 37 FKs está em `inventario-tabelas.md`. O grafo `grafo-banco.json` registra somente relações extraídas das FKs.

## Divergências que devem ser corrigidas antes de novas features

Referências de código nesta seção partem de `3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/`; linhas de SQL referem-se ao dump anexo.

| Prioridade | Evidência | Impacto e trabalho necessário |
|---|---|---|
| P0 | CustomerAddressConfiguration.cs:11 usa CustomerAddresses; dump:27 cria addresses | Consultas/gravações de endereço vão à tabela errada; alinhar mapeamento e todos SQL Dapper com fonte canônica. |
| P0 | IndividualConfiguration.cs:12 e CompanyConfiguration.cs:12 usam Id; dump:383 e :155 usam CustomerId como PK | Configurar nome da coluna e geração da chave compartilhada corretamente; testar PF e PJ contra MySQL real. |
| P0 | OrderAddressConfiguration.cs:12 define PK Id e :13 OrderId; dump:412 tem somente OrderId | Escolher PK compartilhada ou migration explícita; não adicionar uma coluna apenas para contornar teste. |
| P0 | OrderStateHistoryConfiguration.cs:11 usa OrderStateHistories; dump:546 usa orderstatehistory | Histórico/checkout escrevem tabela inexistente. |
| P0 | PaymentConfiguration.cs:16 serializa PaymentStatusId como string; dump:617 é int com FK | Manter representação coerente entre enum, banco, Dapper e contrato. Separar status interno do estado textual do gateway. |
| P1 | CompanyConfiguration.cs:13-15, IndividualConfiguration.cs:13-14, ContactConfiguration.cs:14-17, CustomerAddressConfiguration.cs:14-20, OrderAddressConfiguration.cs:14-20, TenantConfiguration.cs:14 | EF permite comprimentos maiores que SQL (CPF 20/11, CNPJ 20/14, razão estadual 20/15, endereço rua 200/150, número 20/10, CEP 10/8, domínio 150/100 etc.). Normalizar documento/CEP antes de persistir; validações devem respeitar o contrato definitivo. |
| P1 | StoreBannerConfiguration.cs:40 cria índice simples; dump:862 cria unique (TenantId,DisplayOrder) | Resolver ordenação concorrente e política de unicidade; modelo EF precisa reproduzir a restrição adotada. |
| P1 | OrderSectorConfiguration.cs:33 e OrderStatusConfiguration.cs:35 usam Restrict para Tenant; dump:525 e :590 usam Cascade | Alinhar regras de remoção, retenção e migrations. Várias FKs SQL não estão reproduzidas nos mappings EF (por exemplo produto/departamento e pedido/cliente). |
| P1 | Dump tabelas em minúsculas; mappings usam PascalCase | Verificar lower_case_table_names no alvo e executar testes com configuração de produção; sensibilidade não foi verificada em servidor. |
| P1 | Documento de modelagem:50, especificação REST:1098-1099 versus Domain/Enums/OrderStatusEnum.cs:8-21 e CreateDatabase.sql:98-105 | Documentos dizem Shipped=3/Canceled=4; código e seed dizem 9/16. Atualizar contrato e não usar posição numérica como regra de negócio. |

Dump e script inicial têm os mesmos 27 nomes de tabelas e listas de colunas; isso não valida igualdade completa de defaults, constraints ou ambiente de execução. O script contém linhas separadoras compostas apenas por hífens; validar execução real e converter a comentários SQL válidos quando necessário. Ele é criação inicial com dados de demonstração, sem trilha de evolução. SeedAdmin vazio não provisiona administrador.

## Defeitos e riscos demonstráveis no domínio

- **Checkout falha antes da persistência:** `CloudShopping.Domain/Entities/Orders/Order.cs:61` chama AddHistory na criação; `:260` passa Id ainda zero; `OrderStateHistory.cs:22` rejeita orderId <= 0. Aceite da correção: criar pedido novo com histórico associado por navegação/chave temporária e persistir uma vez em transação, preservando o histórico inicial.
- **Version não controla concorrência:** `Product.cs:19,:44,:86` possui Version=1 mas não incrementa; `ProductConfiguration.cs:42` não marca concurrency token. Dois pedidos podem ler o mesmo saldo e sobrescrever reserva. Aceite: disputando última unidade em MySQL, apenas uma compra reserva; conflito retorna resposta controlada, sem saldo negativo. Proteger também ajustes e confirmação/liberação.
- **Reserva não pertence ao pedido no banco:** somente contadores em products e log físico stockmovements (dump:706,:814). Não há reserva com owner, quantidade, expiração e estado. Definir uma reserva por pedido/item, operações idempotentes e rotina de vencimento; carrinho com validade de 30 dias não deve implicitamente bloquear saldo por 30 dias.
- **Publicação após commit sem durabilidade:** `AppDbContext.cs:78` salva e `:93` publica eventos; limpa eventos antes de publicar. Falha do handler deixa pedido persistido e efeitos incompletos. Usar outbox transacional + consumidor idempotente ou efetuar efeitos locais na mesma unidade transacional.
- **Cancelamento repetido pode emitir eventos de novo:** `Order.cs:230` só barra faixa 9-12, permitindo cancelamento de Canceled e outros estados indevidos; gera evento em :240. Aceite: repetição responde de forma estável e não libera estoque nem estorna novamente.
- **Aprovação pode regredir pedido:** `Order.cs:73` aprova Payment em Processing e define Paid sem verificar estado do pedido, total devido, gateway, pagamento concorrente ou valor acumulado. AddPendingPayment aceita método/valor sem invariantes no domínio. Definir máquina financeira independente da operacional e regra de pagamento tardio após expiração/cancelamento.
- **Operações logísticas são nomes de estados:** `Order.cs:148,:178` dizem gerar etiqueta/associar rastreio sem guardar URL, arquivo, transportadora ou código. Faturamento não guarda documento fiscal. Não considerar estes módulos implementados somente por existir enum/método.
- **Snapshot incompleto:** OrderItems tem apenas produto, quantidade e preço; faltam nome/SKU/variante, descontos, frete, impostos e identificação comercial histórica. OrderAddress só permite uma linha por pedido no dump. Especificar cobrança e entrega, complemento e destinatário.
- **Auditoria mutável:** OrderStateHistory declara append-only mas oferece UpdateNotes/Deactivate; repositório permite Remove. Faltam ator, origem, correlation ID e valores anteriores. StockMovements não tem TenantId/OrderId nem chave de deduplicação. Decidir retenção e histórico imutável de eventos relevantes.

## Isolamento e integridade

`TenantProvider.cs:13-29` aceita X-Tenant-Id e cai em tenant 1 quando ausente/inválido. Vincular contexto a identidade autenticada ou domínio de loja validado; não usar fallback silencioso em produção.

`AppDbContext.cs:61-66` filtra apenas Customer, Order, Product e Department. CartRepository.cs:23/:46 consulta por ID/CustomerId sem join de tenant; EmployeeRepository.cs:23, EmployeeUserRepository.cs:23, ProfileRepository.cs:23 e ProfileUserRepository.cs:23 usam FindAsync sem filtro. OrderSector, OrderStatus, ProductImage, StockMovement, StoreBanner e histórico consultam filhos por ID. Isto evidencia ausência de defesa na persistência, não comprova isoladamente exploit em cada endpoint; validar também autorização nos handlers/controladores. Exigir testes cruzando duas lojas e dois clientes da mesma loja.

FK simples de Orders.CustomerId não garante que Customer.TenantId corresponde a Order.TenantId. O mesmo vale para Product/Department, EmployeeUser/Employee, ProfileUser/Profile/EmployeeUser e itens que ligam carrinho/pedido a produtos. Escolher FKs compostas quando aplicáveis e validações explícitas para recursos globais (TenantId nulo). Constraints UNIQUE com tenant nulo não impedem duplicação dos registros globais em MySQL; definir chave normalizada/global ou outra estratégia. Ausentes checks de quantidade positiva, preço/amount válido e 0 <= ReservedStock <= PhysicalStock; única CHECK do dump é comissão do funcionário.

Repositórios Remove fazem exclusão física; filtro IsActive não converte DELETE em soft delete. Aplicar política consistente e não propagar remoção para histórico comercial. Cadastro PF/PJ e endereço padrão dependem apenas da aplicação: banco não garante exclusividade PF versus PJ nem um endereço padrão por tipo. Cadastro de e-mail inativo também requer regra explícita para conflito com unique (TenantId,Email).

## Dependências sugeridas para os cartões

1. **DB baseline → BE mappings → testes MySQL:** migration inicial reproduzível, schema versionado, dados fictícios, índices/constraints, mapeamentos e contrato REST alinhados. Aceite inclui criação de tenant, PF/PJ, endereço, pedido, histórico e pagamento sem erro de tabela/coluna/conversão.
2. **DB integridade de tenant → BE contexto/autorização → FE sessões por loja:** definir globais, FKs, sessões e propriedade do cliente. Aceite exige isolamento negativo e positivo.
3. **DB reservas + idempotência + outbox → BE checkout/estoque → FE checkout:** estoque atômico, snapshot calculado no servidor, requisição repetida retorna mesmo pedido, expiração recuperável, nenhum efeito externo dentro de longa transação SQL.
4. **DB pagamento/gateway/webhook inbox/refund → BE Asaas adapter → FE pagamento:** configurar credencial por loja no servidor, mapear customer externo, charge externa, referência interna, moeda, método, valor bruto/líquido/taxas, vencimento, estado externo e interno. Chaves únicas por conta/ambiente e ID externo; inbox unique event ID; recibo de webhook durável; refund com valor/estado próprios. Não guardar número completo do cartão/CVV. O fluxo exato de conta direta versus subconta/split precisa de decisão comercial antes de implementar.
5. **BE webhook e reconciliação → FE estado assíncrono/admin financeiro:** autenticação do webhook, eventos duplicados/fora de ordem, valor/order/tenant corretos, retries com backoff e fila de falhas, reconciliation por API. Tela nunca confirma pagamento por retorno de navegador. Aceite inclui timeout de criação, aprovação tardia, pagamento duplicado, estorno parcial/total e falha de consumidor.
6. **DB logística/fiscal/pós-venda → BE integrações → FE operações:** dimensões/peso/endereço de origem, frete cotado e snapshot, remessa/etiqueta/rastreio, nota fiscal/documentos, devolução por item/motivo e inspeção antes de repor estoque. Estados do pedido não substituem estes registros.
7. **DB catálogo comercial/marketing → BE regras → FE vitrine/admin:** atributos/variantes, categoria hierárquica, descrição/SEO, marca, promoções/cupom com limites, reviews, wishlist e comunicação consentida. Definir escopo de lançamento sem alegar que estas features já existem.

Complementos SaaS a decidir: planos e cobrança da plataforma são domínio distinto dos pagamentos de compra; onboarding de loja/domínio/marca/configurações, limites, suspensão e acesso de administrador da plataforma. B2B atual é cadastro PJ, sem tabela de preços, condições comerciais, limites de crédito ou aprovação de pedidos.

## Limitações

Não foram executados testes de integração, migrations ou consultas no MySQL; classificação de bloqueios deriva diretamente dos contratos estáticos. A confirmação de comportamento de produção exige ambiente isolado com mesmo engine/configuração. A proposta Asaas descreve responsabilidades de dados; detalhes de endpoints e eventos devem seguir a verificação da documentação oficial feita no planejamento principal.


# Manifesto de cobertura estática

Arquivos de código/projeto examinados nesta auditoria (não inclui bin/obj):

- 3-BackEnd/src/CloudShopping.Domain/CloudShopping.Domain.csproj
- 3-BackEnd/src/CloudShopping.Domain/Entities/Backoffice/Employee.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Backoffice/EmployeeUser.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Backoffice/Profile.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Backoffice/ProfileUser.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Carts/Cart.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Carts/CartItem.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Customers/Address.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Customers/Company.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Customers/Contact.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Customers/Customer.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Customers/Individual.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Orders/Order.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Orders/OrderAddress.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Orders/OrderItem.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Orders/OrderSector.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Orders/OrderStateHistory.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Orders/OrderStatus.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Orders/Payment.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Products/Department.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Products/Product.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Products/ProductImage.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Products/StockLocation.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Products/StockMovement.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Store/StoreBanner.cs
- 3-BackEnd/src/CloudShopping.Domain/Entities/Tenants/Tenant.cs
- 3-BackEnd/src/CloudShopping.Domain/Enums/AddressType.cs
- 3-BackEnd/src/CloudShopping.Domain/Enums/CustomerType.cs
- 3-BackEnd/src/CloudShopping.Domain/Enums/OrderSectorEnum.cs
- 3-BackEnd/src/CloudShopping.Domain/Enums/OrderStatusEnum.cs
- 3-BackEnd/src/CloudShopping.Domain/Enums/PaymentStatus.cs
- 3-BackEnd/src/CloudShopping.Domain/Enums/StockMovementType.cs
- 3-BackEnd/src/CloudShopping.Domain/Events/OrderCanceledDomainEvent.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/AggregateRoot.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/AuditableEntity.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/Entity.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/IDomainEvent.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/IHasDomainEvents.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/IMultiTenant.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/ValueObject.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/Results/Error.cs
- 3-BackEnd/src/CloudShopping.Domain/Primitives/Results/Result.cs
- 3-BackEnd/src/CloudShopping.Domain/ValueObjects/Email.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/CloudShopping.Infrastructure.csproj
- 3-BackEnd/src/CloudShopping.Infrastructure/DependencyInjection.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/AppDbContext.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/AppDbContextFactory.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/CartConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/CartItemConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/CompanyConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/ContactConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/CustomerAddressConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/CustomerConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/DepartmentConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/EmployeeUserConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/IndividualConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/OrderAddressConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/OrderConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/OrderItemConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/OrderSectorConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/OrderStateHistoryConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/OrderStatusConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/PaymentConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/ProductConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/ProductImageConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/ProfileConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/ProfileUserConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/StockMovementConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/StoreBannerConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Persistence/Configurations/TenantConfiguration.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/CartRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/CustomerRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/DepartmentRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/EmployeeRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/EmployeeUserRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/OrderRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/OrderSectorRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/OrderStateHistoryRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/OrderStatusRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/ProductImageRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/ProductRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/ProfileRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/ProfileUserRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/StockMovementRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/StoreBannerRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/TenantRepository.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Repositories/UnitOfWork.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Services/FileStorageService.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Services/PasswordHasher.cs
- 3-BackEnd/src/CloudShopping.Infrastructure/Services/TenantProvider.cs
