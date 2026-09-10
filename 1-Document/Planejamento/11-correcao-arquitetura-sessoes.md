# Correção de arquitetura — sessão e resolução da loja

A crítica do usuário procede: as entregas anteriores introduziram acesso direto à persistência e regras de aplicação em controllers/adaptadores HTTP. Esta correção não equivale à adequação arquitetural de todo o projeto.

## Recorte corrigido

- `CloudShopping.Api/Security/StoreSecurity.cs`: somente adaptação HTTP (claims, cookies, leitura de host/header e tradução do resultado para HTTP). Sem DbContext, EF ou MySQL.
- `CloudShopping.Api/Controllers/SessionController.cs`: somente entrada/saída HTTP, chamada dos casos de uso e emissão/remoção do cookie. Sem consultas ao banco, manipulação de entidades ou unificação de carrinhos.
- `CloudShopping.Application/Features/Sessions/SessionUseCases.cs`: login administrativo/consumidor, cadastro, visitante, logout e orquestração da unificação do carrinho após autenticar o destino.
- `CloudShopping.Application/Features/Sessions/SessionLifecycle.cs`: criação/validade/revogação de sessão e decisão de resolução da loja. Confere vencimento, revogação, credencial, atividade, permissão, sujeito, papel e tenant. Usa TimeProvider para decisões temporais.
- `CloudShopping.Application/Abstractions/Data/ISessionStore.cs` e `ISessionAccounts.cs`: portas de persistência com contratos explícitos, sem IQueryable/DbContext.
- `CloudShopping.Infrastructure/Repositories/SessionStore.cs` e `SessionAccounts.cs`: implementação EF, projeções, persistência e transação. Queries de validação sem contexto prévio incluem TenantId explícito.
- Entidades existentes Customer e Cart continuam responsáveis pelas mutações de domínio. Nenhuma tabela/migração ou contrato JSON público foi alterado.

## Validação

Build da API aprovado em saída isolada `.local/architecture-build`, sem interromper a API do usuário. Dez testes unitários novos de SessionLifecycle aprovados. Smoke HTTP anterior aprovado na porta 5159 com o banco sintético cloudshopping_dev: autenticação, cookies, CSRF, cadastro preservando visitante, unificação do carrinho, posse, tenant, checkout e rotas dos demais módulos.

Também aprovado o smoke HTTP de permissões, revogação de sessões e troca de senha. A instância temporária da porta 5159 foi encerrada ao concluir a verificação.

## Dívida que continua explícita

Ainda existem dependências de infraestrutura nos controllers AccountSecurity, Access, Asaas, CatalogDetails, CatalogImports, Coupons, Engagement, Notifications, Operations, Reports e Storefront, além do RequestGuards. Também existem casos de uso em serviços da infraestrutura e dependências técnicas legadas na Application (por exemplo Dapper/MySqlConnector). Não foi apenas escondida essa dívida atrás de interfaces nem declarada conformidade integral com DDD/Clean Architecture.

Próximos recortes devem extrair casos de uso por módulo, mantendo transações e idempotência, criar portas de persistência/integração na Application, manter invariantes nas entidades de domínio e deixar adapters EF/Asaas/HTTP na infraestrutura/API. Cada extração deve ser validada pelos cenários existentes antes de seguir ao próximo módulo.
