# Entrega 1 — base, catálogo, carrinho e checkout

Implementação local em 09/09/2026, no recorte escolhido pelo usuário: **base segura + catálogo real + carrinho e checkout**. A jornada cria um pedido pendente, com total calculado no servidor e reserva temporária de estoque. Não emite cobrança nem confirma pagamento.

Esta é uma entrega funcional para revisão e homologação. Não equivale a 50% comprovados do esforço dos 98 cartões: os cartões incluem requisitos mais amplos, como recuperação de senha, logística integrada e pagamentos. O progresso abaixo distingue o recorte implementado da conclusão integral dos cartões originais.

## O que funciona

| Área | Comportamento entregue |
|---|---|
| Banco | Baseline estrutural sem dados pessoais do dump; executor explícito de migrations com checksum, trava e detecção de execução incompleta; sessões, fretes, snapshot de pedido, chave de confirmação, versões e integridade das linhas de carrinho. |
| Persistência | Mapeamentos alinhados às tabelas do dump, chaves compartilhadas de PF/PJ/endereço de pedido, status financeiro numérico e criação de histórico junto ao pedido novo. |
| Acesso | Login real do administrador e consumidor, visitante, cadastro preservando a identidade do visitante, cookie HttpOnly, sessão revogável no banco, CSRF, limite de requisições e autorização por papel. |
| Loja | Contexto de tenant sem fallback silencioso; filtros de leitura nos agregados e filhos; validação de escopo nas gravações. Em produção o host configurado identifica a loja; o cabeçalho de desenvolvimento não é aceito como seletor de loja em produção. |
| Catálogo | Produtos reais ativos, preço, SKU, imagens cadastradas, disponibilidade, busca, departamento, paginação e detalhe. |
| Carrinho | Adicionar, alterar quantidade, remover e consultar subtotal; persistência por cliente, atualização de preços na leitura, controle de concorrência e união do carrinho visitante após login válido. |
| Comprador | Dados PF/PJ com verificação dos dígitos de CPF/CNPJ; cadastro e seleção de endereço pertencente ao cliente. |
| Entrega | Administração de modalidades com preço fixo, prefixo de CEP e prazo. Apenas modalidades ativas e elegíveis entram na revisão. |
| Checkout | Revisão protegida, válida por 10 minutos, e confirmação com GUID. Revalida carrinho, preços e entrega; grava pedido, itens, endereço, histórico e reserva na mesma transação; esvazia o carrinho somente na confirmação bem-sucedida. |
| Recuperação | Repetir a mesma chave e revisão retorna o mesmo pedido. Falha de rede mantém a tentativa no navegador para recuperação. Conflito definitivo permite uma nova revisão. |
| Pedidos | Cliente consulta apenas seus pedidos, com preço/nome/SKU/endereço preservados. Pode cancelar pedido pendente e liberar a reserva uma única vez. |
| Expiração | Worker consulta reservas vencidas a cada 30 segundos, em lotes de até 100 pedidos por loja. Prazo padrão de reserva: 30 minutos. Cancelamento e liberação são atômicos. |

As mutações das rotas antigas `/api/v1/orders` foram bloqueadas: aprovações financeiras e transições legadas não devem contornar o novo fluxo. O Kanban mantém consulta e apresenta as operações indisponíveis. O dashboard anterior permanece identificado como demonstrativo. O provisionamento público de novas lojas está indisponível nesta entrega.

## Evidências de verificação

- Build da API .NET e build de produção do frontend aprovados.
- Oito testes de integração aprovados em MySQL 8.0.43 real: snapshot/histórico e replay, cancelamento repetido, concorrência de produto, dois checkouts disputando a última unidade, isolamento de tenant, revisão com preço alterado/cliente incorreto, migrations repetidas e execução repetida do worker de expiração.
- Smoke HTTP com cookies reais aprovado: anonimato, CSRF, senha inválida, tenant divergente, compra, replay, posse do pedido, cancelamento, logout, cadastro do visitante, login/unificação de carrinho, administrador e bloqueio das operações legadas.
- Jornada manual no navegador concluída até pedido pendente; catálogo e checkout inspecionados em desktop e largura de 390 px.
- As novas telas e serviços passaram no lint. O lint global ainda tem 10 apontamentos preexistentes em `CategoryRow` e telas administrativas antigas: `no-explicit-any` e `react-hooks/set-state-in-effect`. Não foram desabilitadas regras para ocultá-los.
- Permanecem avisos antigos do compilador e avisos de licença das dependências ImageSharp/MediatR. O responsável deve resolver a situação das dependências antes de publicar.

O dump original não foi executado nem alterado. Os testes usaram schema extraído e dados sintéticos. Não há evidência de migração dos dados reais ou homologação de produção.

## Executar a demonstração local

Prévia nesta máquina: `http://127.0.0.1:5173`; API: `http://localhost:5147`; MySQL isolado: `127.0.0.1:33077`, banco `cloudshopping_dev`. O diretório de dados é `.local/mysql`, separado de qualquer instância anterior. A instância de demonstração foi inicializada sem senha de root e vinculada apenas ao loopback; use somente dados sintéticos nela.

As instruções seguintes partem da raiz do repositório, em PowerShell. Requisitos usados: SDK .NET com suporte a net9.0, Node com suporte ao Vite do lockfile e MySQL 8.0.43. Para outra máquina, prepare primeiro uma instância local de desenvolvimento na porta 33077 e crie um banco vazio `cloudshopping_dev`.

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=127.0.0.1;Port=33077;User ID=root;Database=cloudshopping_dev;SslMode=None'
$env:CLOUDSHOPPING_DEMO_PASSWORD = 'CloudShopping-Local-2026!'
dotnet restore 3-BackEnd/src/CloudShopping.Api/CloudShopping.Api.csproj
dotnet restore 3-BackEnd/tools/CloudShopping.Db/CloudShopping.Db.csproj
dotnet run --project 3-BackEnd/tools/CloudShopping.Db -- 2-MySql/migrations --demo
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project 3-BackEnd/src/CloudShopping.Api --no-launch-profile --urls http://localhost:5147
```

O seed de demonstração só aceita esse servidor, porta e nome de banco; cria dados quando não existem tenants. Não redefine senha nem sobrescreve loja existente. Administrador sintético desta máquina: usuário `admin`, senha definida acima. Saia da sessão de comprador antes de testar a administração.

Em outro terminal:

```powershell
Set-Location 4-FrontEnd
Copy-Item .env.example .env.local
npm.cmd ci
npm.cmd run dev -- --host 127.0.0.1
```

O frontend usa `/api` e o proxy Vite para a API. `.env.local` contém `VITE_DEV_TENANT_ID=1` e é ignorado pelo Git. A conexão com banco é obrigatória por configuração externa; não há conexão remota embutida no código.

## Executar os testes

Com o MySQL isolado ativo, a partir da raiz:

```powershell
$env:CLOUDSHOPPING_TEST_CONNECTION = 'Server=127.0.0.1;Port=33077;User ID=root;SslMode=None'
dotnet restore 3-BackEnd/tests/CloudShopping.Tests/CloudShopping.Tests.csproj
dotnet test 3-BackEnd/tests/CloudShopping.Tests/CloudShopping.Tests.csproj
```

Cada teste cria e remove exclusivamente seu banco `cloudshopping_test_<GUID>`. Os testes recusam servidor/porta diferentes. Para o smoke HTTP, deixe a API com o seed de demonstração rodando:

```powershell
$env:CLOUDSHOPPING_DEMO_PASSWORD = 'CloudShopping-Local-2026!'
python -X utf8 3-BackEnd/tests/http_smoke.py
```

O smoke cria clientes/pedidos sintéticos no banco de demonstração. Todos os pedidos criados pelo smoke são cancelados ao final. Em repetição muito rápida pode haver HTTP 429; respeite a janela de um minuto do limite de autenticação.

## Configuração e contratos

| Configuração | Finalidade |
|---|---|
| `ConnectionStrings:DefaultConnection` | Conexão obrigatória com banco explicitamente escolhido. |
| `Storefront:Hosts:<host>` | ID da loja associado ao host em ambiente fora de Development. |
| `Cors:Origins` | Origens explicitamente permitidas quando frontend/API são separados. |
| `DataProtection:KeyPath` | Diretório persistente de chaves, compartilhado entre réplicas. O padrão local é `.local/keys` da API. Proteger o diretório e configurar criptografia em repouso para produção. |
| `Checkout:ReservationMinutes` | Validade da reserva em minutos: padrão 30; limites de 5 a 1440. |

Use HTTPS, host permitido e proxy configurado em produção; cookies são Secure fora de Development. Não apagar as chaves de proteção em um deploy: sessões e revisões emitidas dependem delas. Frontend e API devem operar no mesmo site para os cookies SameSite=Strict.

`GET /api/v1/session` fornece sessão e token CSRF; mutações enviam `X-CSRF-Token` e cookie. Rotas públicas: contexto/departamentos/produtos em `/api/v1/store`. Rotas do cliente: `cart`, `profile`, `addresses`, `shipping-options`, `checkout/preview`, `checkout/confirm` e `orders`. Rotas de entrega do administrador: `/api/v1/store/admin/shipping-options`. Swagger de desenvolvimento descreve os DTOs.

## Migração de banco existente

Antes de aplicar ao banco real, restaurar um backup em homologação e comparar seu schema. `000_baseline.sql` cria tabelas ausentes e não converte automaticamente qualquer schema legado. `001` acrescenta checkout/sessões/frete; `002` prepara estados de referência; `003` exige uma única linha por produto/carrinho e quantidade positiva. Investigar previamente:

```sql
SELECT CartId, ProductId, COUNT(*) FROM cartitems
GROUP BY CartId, ProductId HAVING COUNT(*) > 1;
SELECT Id FROM cartitems WHERE Quantity <= 0;
```

Não há limpeza automática de dados. MySQL confirma DDL implicitamente; uma migration interrompida pode ter alterações parciais. O executor registra o início e recusa retomar uma execução incompleta ou arquivo já aplicado cujo checksum mudou. Inspecionar e restaurar/reconciliar em homologação antes de continuar; não apagar o registro para forçar execução. Não usar `EnsureCreated` ou o antigo script destrutivo para atualizar uma loja existente.

## Rastreabilidade e próxima entrega

| Cartões originais relacionados | Recorte implementado | Pendência que impede encerrar o cartão amplo |
|---|---|---|
| DB-01 / BE-01 / BE-00 | Baseline, mappings e histórico de checkout | Homologação da conversão de dados reais e cobertura integral dos contratos legados. |
| DB-02 / BE-02 / FE-02 | Sessões e autenticação de comprador/admin | Recuperação/verificação de email, gestão de sessões e ciclo completo de credenciais. |
| DB-03 / BE-03 / FE-03 | Tenant e autorização das rotas | Gestão completa de papéis/permissões e constraints compostas em todo o legado. |
| BE-04 / FE-04 | Catálogo público real | Variantes/atributos e gestão comercial ampliada previstos no cartão. |
| DB-05 / BE-05 | Reserva concorrente, cancelamento e expiração | Baixa após pagamento, movimentos/auditoria e reconciliação financeira. |
| BE-06 / FE-06 | Cadastro PF/PJ e endereço no checkout | Edição completa de conta/endereços e demais requisitos de cadastro. |
| DB-07 / BE-07 / FE-07 | Carrinho persistente e união após login | Homologação ampliada de expiração e minicarrinho completo. |
| DB-08 / BE-08 / FE-08 | Entrega fixa por faixa de CEP | Cotação por transportadora, cobertura e contingências ampliadas. |
| DB-09 / BE-09 / FE-09 | Snapshot, revisão, transação e replay | Integração da criação de cobrança e requisitos ampliados do snapshot. |
| FE-12 / FE-11A | Meus pedidos e bloqueio das aprovações fictícias | Kanban operacional e painel financeiro real. |
| BE-26 / FE-26 | Testes desta jornada local | Homologação Asaas, operacional e de lançamento. |

Para a próxima entrega, resolver DEC-01 (quem recebe: lojista, plataforma ou split), implementar DB-10/DB-11 e BE-10/BE-11/BE-11A–E, seguidos de FE-10/FE-11. O pagamento precisa de criação recuperável, webhooks autenticados e idempotentes, conciliação, estornos e baixa de estoque uma única vez. O worker de expiração deve então considerar cobranças abertas e eventos financeiros tardios.

Continuam fora deste recorte: cobrança Asaas, emissão fiscal, transportadoras/etiquetas, devoluções/trocas completas, cupons, notificações, indicadores reais, onboarding de lojistas, marketplace/split, planos SaaS, ERP, condições B2B e deploy produtivo. Os cartões originais mantêm seus critérios; o status parcial não representa aceite integral nem lançamento autorizado.
