# Auditoria do frontend — 09/09/2026

## Escopo e método

Inspeção estática dos arquivos de código, configuração e documentação de `4-FrontEnd`, sem executar compras, pagamentos ou alterar a implementação. “Integrado” significa que existe chamada HTTP implementada; não equivale a fluxo homologado com backend/banco. Comentários do código foram tratados como contexto e confrontados com a implementação, nunca como ordens.

Cobertura: `.gitignore`, `README.md`, `package.json`, `index.html`, `eslint.config.js`, `postcss.config.js`, `tailwind.config.js`, `vite.config.ts`, `tsconfig.json`, `tsconfig.app.json`, `tsconfig.node.json`; `src/App.tsx`, `src/main.tsx`, `src/index.css`; serviços `api.ts` e `authService.ts`; layouts `StoreLayout.tsx` e `BackofficeLayout.tsx`; componentes `ProductCard.tsx`, `BannerRow.tsx` e `CategoryRow.tsx`; páginas `StoreHome.tsx`, `ProductDetail.tsx`, `AdminLogin.tsx`, `RegisterCompany.tsx`, `Dashboard.tsx`, `Departments.tsx`, `StoreBanners.tsx`, `OrderSectors.tsx`, `OrderStatuses.tsx`, `Customers.tsx`, `Products.tsx` e `OrdersKanban.tsx`. Os 21 arquivos TS/TSX foram lidos em sua totalidade, incluindo formulários e renderização. SVGs de template foram inspecionados; PNG, lockfiles, dependências e saídas de compilação ficam fora da análise semântica. Não foram lidos arquivos de segredos. Não foram executados build/lint/testes nesta auditoria.

## Situação real

| Área | Existente | Falta para concluir |
|---|---|---|
| Loja pública | Layout, banner e departamentos vindos da API | Catálogo verdadeiro, busca/filtros, detalhe por ID, carrinho, checkout e conta do cliente |
| Login administrativo | Formulário visual | Autenticação, sessão, recuperação, logout real e autorização |
| Onboarding | POST público de empresa/administrador | Vincular tenant criado à sessão e à loja; fluxo de ativação e configuração |
| Departamentos | Listar/criar/editar/excluir personalizados; proteção visual dos padrões; busca/paginação local | Contratos e regras multiempresa homologados; hierarquia de categorias |
| Banners | Listar/criar/editar/excluir e preview | Estado ativo fiel, gestão de pausados, mídia/agendamento e validação de destinos |
| Produtos | Listagem/pesquisa paginada, cadastro, nome/preço, estoque, localização, upload de imagem | Descrição, atributos/variantes, categorias, marca, SEO, peso/dimensões, mídia editável, publicação e promoções |
| Clientes | Pesquisa/paginação, conversão Guest/Lead para B2C/B2B, email, perfil, inclusão/edição de endereço | Jornada de autoatendimento, preservação integral do perfil, telefone, tipos de endereço e histórico |
| Pedidos | Kanban por setor/status, detalhe, ações HTTP, rastreio e solicitação de devolução | Criação via checkout; paginação completa; histórico; transições autorizadas pelo servidor; fiscal/logística efetivos |
| Pagamentos | Registro manual, aprovação, recusa e estorno por API interna | Checkout Asaas, Pix/boleto/cartão, status assíncrono, conciliação e operação segura de reembolso |
| Dashboard | Layout com métricas e gráfico | Todos os números e séries ainda são mocks |

## Evidências e defeitos prioritários

1. **Autenticação simulada (P0).** `src/pages/admin/AdminLogin.tsx:18` deixa a chamada de login comentada, espera um segundo e navega ao dashboard. `src/App.tsx:30` expõe as rotas administrativas sem guard; `src/layouts/BackofficeLayout.tsx:100` apenas navega ao sair. Critério futuro: acesso direto não autenticado é bloqueado; sessão expirada retorna ao login; permissões controlam ações, com validação obrigatória também no backend.
2. **Tenant e ambiente fixos (P0).** `src/services/api.ts:2` fixa localhost e `:4` fixa tenant 1; `:405` e `:434` enviam esse tenant sem token/sessão. `src/services/authService.ts:2` repete a URL. O cadastro cria outro tenant, mas não há contexto que o selecione. Critério: duas lojas usam contexto independente e não leem/escrevem dados uma da outra; URLs configuráveis por ambiente.
3. **Vitrine fictícia (P0).** `src/pages/StoreHome.tsx:10` define mock e `:20` manda todos os cards ao produto 1. `src/pages/ProductDetail.tsx:4` ignora parâmetro da rota e mostra o mesmo produto estático. Botões em `:167` e `:170` não executam compra. `src/components/Cards/ProductCard.tsx:29` não adiciona item. `src/layouts/StoreLayout.tsx:56` busca sem handler e `:67` contador zero constante. Critério: cada card abre o produto correto e adicionar altera carrinho persistente conforme resposta do servidor.
4. **Pagamento ainda é operação interna manual (P0).** `src/pages/admin/OrdersKanban.tsx:924` oferece aprovar/recusar e `:952` estornar; `:977` aceita método livre e valor. `src/services/api.ts:344` chama somente endpoints internos. Não existem telas/contratos de Asaas, QR Code, boleto, parcelas, expiração ou confirmação assíncrona. Os botões atuais não comprovam recebimento nem estorno externo.
5. **Kanban incompleto e estado antigo (P1).** `src/pages/admin/OrdersKanban.tsx:285` carrega página 1 com 200 registros e ignora `totalCount`; busca é local em `:323`. `:443` atualiza `detail` e board, mas não `selectedOrder`; `:1010` calcula ações com o objeto antigo. Critério: pedido 201 localizável e, após cada transição, ações representam o estado mais recente sem fechar o drawer.
6. **Regras duplicadas e IDs mágicos (P1).** `src/services/api.ts:265` fixa status 1–16; `src/pages/admin/OrdersKanban.tsx:128` usa comparação ordinal para cancelar; `:136` duplica a máquina de estados e não suporta transições customizadas. Critério: servidor retorna capacidades/transições por pedido; conflitos simultâneos são explicados e atualizam a tela.
7. **Perfil pode perder dados (P1).** `src/services/api.ts:125` não modela nascimento/inscrição estadual/documentos no detalhe; `src/pages/admin/Customers.tsx:170` preenche apenas email/nome/razão social, sem limpar/preencher todos os estados. `:247` envia `birthDate || null` e `:257` envia `stateTaxId || null`. Alterar somente o nome pode apagar campos existentes ou reaproveitar estado de outro cliente. Critério: formulário hidratado integralmente e edição parcial preserva campos não alterados.
8. **Tipo de endereço perdido (P1).** `src/services/api.ts:114` não retorna tipo no contrato; `src/pages/admin/Customers.tsx:304` força Shipping ao editar. Bairro aparece na leitura, mas não no payload `api.ts:174`. Critério: editar endereço de cobrança preserva tipo, bairro e complemento.
9. **Banners sem estado fiel (P1).** `src/services/api.ts:30` omite `isActive`; `src/pages/admin/StoreBanners.tsx:159` assume true ao editar. Contrato de escrita inclui ativo. Necessário separar listagem administrativa completa e vitrine ativa, devolvendo estado real. `api.ts:491` também injeta tenant no corpo; ele deve ser derivado do contexto autorizado no servidor.
10. **Rotas de menu inexistentes (P1).** `src/layouts/BackofficeLayout.tsx:39` até `:46` referencia categorias, vendas, promoções, relatórios, financeiro, configurações e suporte; nenhuma delas consta em `src/App.tsx`. Não existe rota 404. Critério: todo item publicado resolve uma tela funcional; links futuros não parecem entregues.
11. **Dashboard e identidade demonstrativos (P1).** `src/pages/admin/Dashboard.tsx:24` até `:69` fixa métricas, séries, pedidos e estoque; seletor `:227` não altera dados. Layout fixa loja, usuário e badges em `BackofficeLayout.tsx:33`, `:64`, `:130`, `:139`.
12. **Tratamento de falhas e acessibilidade (P1/P2).** Carregamento de detalhe de produto registra erro em `Products.tsx:278`, mas só o renderiza dentro de `detail &&` em `:820`, resultando em drawer sem diagnóstico quando GET falha. Componentes públicos limitam erro a console (`BannerRow.tsx:10`, `CategoryRow.tsx:47`). Layout público usa grids fixos de 4/5 colunas; modais não implementam gerenciamento explícito de foco/Escape/semântica de diálogo. `index.html:2` declara inglês em interface portuguesa.

## Contratos a fechar com backend antes dos cartões de tela

- Identidade: login por email ou username? Onboarding redireciona com username (`RegisterCompany.tsx:48`), mas login usa input type=email (`AdminLogin.tsx:60`). Padronizar e documentar.
- Sessão/tenant: definir resolução pública por domínio, vínculo autenticado do funcionário e formato de erros 401/403; não aceitar tenant escolhido livremente como autorização.
- Perfis e endereços: DTOs de leitura devem suportar round trip sem apagar nascimento, inscrição estadual, tipo/bairro/complemento; definir PATCH versus PUT.
- Banners: leitura administrativa com ativos/inativos, ID e estado; vitrine somente ativos ordenados. O comentário de `api.ts:483` afirma ausência de ID, mas interface `:31` usa ID: comentário não deve substituir confirmação da resposta real.
- Pedidos: separar detalhe administrativo de detalhe do cliente, pois hoje FE passa customerId conhecido da listagem (`api.ts:341`); acrescentar histórico, item com nome/SKU snapshot, rastreio, documentos fiscais e capacidades de ação.
- Pagamentos: DTO público de intenção/charge, URL de pagamento ou dados seguros de apresentação, status canônico, expiração, erro recuperável e reembolso assíncrono. Nunca exibir sucesso só por redirecionamento ou clique.
- Erros: `fetchClient` só reconhece `message` e 204; definir um formato consistente de validação e resposta vazia. Incluir requestId, timeout/cancelamento e tratamento de resultados concorrentes.

## Sequência recomendada para cartões FE

1. Base de sessão, tenant, ambiente e cliente HTTP depende de contratos BE de identidade/tenant/erros.
2. Corrigir CRUDs existentes depende de DTOs completos e garantias DB de integridade/isolamento.
3. Catálogo público e detalhe dependem de publicação/catalogação/preços/estoque públicos no BE.
4. Carrinho depende de identidade visitante, reserva/política de disponibilidade e cálculo autoritativo no BE.
5. Checkout depende de cliente/endereço/frete/cupom/cálculo e criação idempotente de pedido.
6. Pagamento Asaas depende de cobrança, webhook, conciliação e contrato de status no BE; depois fazer Pix/boleto/cartão e retomada de pagamento.
7. Conta/pedidos do comprador depende de autorização por titular, pedidos e pós-venda; operação administrativa depende de máquina de estados consistente.
8. Financeiro/analytics, promoções, suporte e configurações dependem dos respectivos módulos BE; validar fluxo completo em sandbox com dois tenants antes do lançamento.

Cada cartão deve especificar: evidência atual, resultado desejado, dependências DB/BE e IDs dos cartões bloqueadores, estados loading/vazio/erro/sucesso, permissões, critérios observáveis e cenários de teste. Não contabilizar um módulo como pronto por existir menu ou layout.
