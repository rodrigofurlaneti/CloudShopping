# Entrega 3 — operação e expansão dos módulos

Data: 09/09/2026. O usuário autorizou implementar todo o escopo mapeado. Esta execução ampliou o produto, mas **não concluiu 100% dos 98 cartões**. Há critérios internos ainda não implementados e integrações externas pendentes. Frete/etiquetas, fiscal e e-mail foram explicitamente deixados pelo usuário como **a definir**. Os critérios originais permanecem vigentes; “Parcial” não significa aceite integral.

## Funcionalidades implementadas nesta execução

| Módulo | Resultado disponível |
|---|---|
| Pedidos | Painel paginado em `/admin/orders`, filtro por número/etapa, detalhe, ações autorizadas pelo servidor e notas internas imutáveis com autor. Estado de atendimento separado do financeiro. |
| Expedição assistida | Processamento → separação → embalagem → postagem. Remessas parciais por item, transportadora/serviço/código/volumes persistidos. Não gera nem compra etiqueta. Bloqueia pedido sem pagamento elegível. |
| Rastreio | Operador registra trânsito, falha ou entrega com evidência. Entrega de uma remessa parcial não conclui o pedido todo. O cliente acompanha em Meus pedidos. |
| Devoluções | Cliente solicita itens entregues; aprovação/recusa administrativa; inspeção informa quantidade aprovada para reposição. Estoque retorna uma única vez, em transação. Estado financeiro permanece independente. |
| Notificações | Outbox persistida na transação do pedido e consumidor do portal. Trava entre workers, entrega local idempotente, retry e fila de falhas. `/notifications` para o cliente e `/admin/notifications` para acompanhamento. E-mail não é enviado. |
| Cupons | Cadastro e pausa em `/admin/coupons`; percentual ou valor fixo; vigência, subtotal mínimo e limites total/por cliente. Checkout revalida e preserva código/desconto no pedido. Cancelamento antes do pagamento libera o uso. |
| Favoritos | Adicionar no produto, consultar e remover em `/favorites`, com isolamento de cliente e loja. |
| Avaliações | Compra entregue obrigatória, nota/texto, pendência antes da publicação e moderação com motivo. Média considera só publicadas. Fila em `/admin/reviews`. Textos são exibidos como texto, sem executar HTML. |
| Atendimento | Protocolos relacionados opcionalmente a um pedido, mensagens, resposta administrativa e encerramento/reabertura. `/support` e `/admin/support`. Privacidade é uma categoria de solicitação; não executa anonimização automática. |
| Indicadores | Dashboard sem números fictícios, período UTC, totais reais, etapas, série diária, baixo estoque e CSV limitado a 5.000 pedidos. Valores aprovados são brutos, não saldo liquidado. |
| Importação | CSV com prévia persistida, execução em lotes pelo worker, resultado por linha e identificação do operador. Reenvio idêntico recupera o lote existente. Versão do produto impede sobrescrever alterações/reservas posteriores à prévia. |
| Catálogo | Descrição, marca, URL por slug, peso/dimensões, atributos e família/variante. Cada variante é um produto/SKU com preço e estoque próprios. Ficha em `/admin/catalog-details`; URL pública `/p/{slug}`; URLs antigas por ID continuam válidas. |
| Navegação | Menu administrativo adaptado para móvel; removidos a busca sem ação e o contador fictício de notificações do cabeçalho. |

## Persistência e regras de concorrência

Migrations incrementais `005_operations.sql` a `010_catalog_details.sql`, aplicadas somente ao banco sintético `127.0.0.1:33077/cloudshopping_dev`. O dump anexado não foi alterado. O executor continua verificando checksums e execuções incompletas. Não editar migrations já aplicadas.

As operações financeiras e de atendimento compartilham a trava por pedido para evitar corrida entre expedição e bloqueio financeiro. Mutações recebem versão do pedido e chave de operação; repetir a mesma chave recupera o resultado sem repetir o efeito, enquanto payload diferente gera conflito. O histórico novo preserva autor, ação, estado anterior/novo, horário UTC e hash da solicitação. Notas internas não são expostas ao cliente.

Pedidos existentes recebem `FulfillmentState=Unstarted`, sem presumir evidência logística a partir dos antigos IDs de status. Migrar atendimento de pedidos legados exige revisão operacional; o histórico original permanece preservado. Alterar cadastros de status não altera as regras canônicas do fluxo novo.

A outbox armazena IDs e tipo de evento, sem replicar credenciais ou documentos pessoais. A trava MySQL é liberada quando a conexão/processo encerra. Notificação no portal e conclusão do evento são confirmadas na mesma transação, com chave única do evento. Após falhas repetidas o item fica visível como falha. O painel permite reprocessá-lo preservando o ID de correlação. Esse consumidor não representa entrega por e-mail e não oferece semântica de envio único em provedores externos ainda não integrados.

Cupom é aplicado somente aos produtos, com arredondamento de duas casas e midpoint away from zero. Não há acumulação de cupons. O checkout exige total positivo. A utilização é reservada no fechamento do pedido; somente o cancelamento sem pagamento devolve o uso nesta versão. Estorno não reabre automaticamente a elegibilidade. A regra aprovada precisa ser apresentada comercialmente antes do lançamento.

## Operação da importação

Cabeçalho obrigatório: `sku,name,departmentId,price,physicalStock`. Separador vírgula, aspas CSV reconhecidas e preço com ponto decimal. Limite de 200 linhas/200 mil caracteres. O sistema não grava o arquivo original; mantém a representação das linhas para auditoria e retomada.

O administrador revisa a prévia e confirma a execução. O worker processa até 25 linhas por ciclo. Cada linha usa uma transação independente; linhas concluídas permanecem confirmadas se outra linha falhar. Estoque físico inferior ao reservado ou versão diferente da prévia bloqueia a linha. Falhas transitórias deixam trabalho pendente; erro de dados exige arquivo corrigido e nova prévia. Reenviar exatamente o mesmo conteúdo recupera o lote original, inclusive seu resultado. Nenhum ERP externo está conectado.

## Validação executada

- Build do backend e build TypeScript/Vite aprovados.
- Suíte completa: **45 testes aprovados**, sendo 43 cenários com MySQL isolado e 2 testes do gateway Asaas simulado. O teste de limite de cupom foi adicionalmente reforçado com produtos distintos para separar a concorrência do cupom da concorrência de estoque e passou novamente.
- Os testes novos cobrem operação de pedido, estorno separado de retorno físico, replay de remessa/inspeção, isolamento entre lojas/clientes, avaliação moderada, favoritos, atendimento, relatórios, outbox/rollback/dois consumidores, concorrência de cupom, importação e variantes.
- O runner agora libera o pool de cada schema descartável ao terminar o teste. Houve uma execução completa com quatro falhas intermitentes; os casos isolados passaram e a suíte completa posterior passou após o ajuste de limpeza. Relatório TRX em `.local/test-results/AMD_ISS_2026-09-09_17_18_01_net9.0.trx`.
- Smoke HTTP aprovado contra a API local: fluxos anteriores e rotas novas, posse de protocolo/pedido, autorização, nota auditada, bloqueio operacional de pedido não pago, favoritos, consultas de catálogo, relatórios e filas.
- ESLint dos arquivos novos/alterados desta entrega aprovado. O lint global ainda tem os dez erros históricos de outras telas; isso continua pendência de qualidade do projeto.
- Conferidos no navegador: login administrativo, dashboard com dados reais, lista/detalhe operacional e formulário de catálogo em viewport de 390 pixels. Essa inspeção não equivale a homologação E2E de todos os estados.

Nenhuma cobrança real, postagem de transportadora, emissão fiscal ou mensagem de e-mail foi executada. A homologação externa Asaas continua pendente de conta/credenciais e webhook HTTPS público.

## Limites que permanecem

Este é um avanço de implementação, não uma entrega comercial completa. Antes de operar em produção ainda é necessário concluir o [saldo do escopo](09-saldo-escopo-completo.md), com atenção a:

- Fiscal, transportadora/etiquetas/rastreio externo, e-mail e ERP: definir provedores e completar adaptadores/homologação. A postagem assistida não comprova autorização fiscal; não ativar expedição comercial sem definir o bloqueio fiscal aplicável.
- Devoluções: logística reversa, anexos privados, troca vinculada a novo pedido e integração de reembolso parcial ao caso. Inspeção física não solicita estorno automaticamente.
- Notificações: templates de e-mail, preferências/consentimentos, retenção, alertas e auditoria específica do reprocessamento. A fila atual cobre mudanças de pedido, financeiro e atendimento; protocolos e moderação ainda não produzem notificações.
- Promoções: segmentação por categoria/SKU/cliente, campanhas e limites adicionais previstos no cartão amplo.
- Catálogo: a família agrupa SKUs simples existentes; filtros avançados, SEO completo, redirecionamento de slug antigo e homologação ampliada do ciclo de imagens permanecem pendentes. Peso/dimensões zero significam ficha incompleta e não habilitam transportadora.
- Relatórios: taxas/líquido e estornos parciais externos não estão conciliados nos indicadores. A seleção é por data de criação do pedido, considerando o estado atual dos pagamentos desse conjunto.
- Assinaturas SaaS, preços/orçamentos B2B, onboarding/KYC/subcontas, repasses, permissões granulares, recuperação/verificação de acesso, conta/endereços completos, políticas/consentimentos, SEO/páginas institucionais e operações de produção ainda têm trabalho interno.
- Continuam necessários testes de restauração, carga, acessibilidade completa, pipeline/deploy, revisão de licenças e homologação integral dos critérios dos 98 cartões. Avisos de licença de dependências e dívida de lint anterior não foram eliminados.

## Reprodução local

API: `http://localhost:5147`; frontend: `http://127.0.0.1:5173`. O administrador de demonstração já existente acessa `/admin/dashboard`. Credenciais reais de Asaas não são necessárias para consultar os módulos internos.

Backend: `dotnet test 3-BackEnd/tests/CloudShopping.Tests --no-restore`. Frontend: `npm.cmd run build` em `4-FrontEnd`. Smoke: executar `3-BackEnd/tests/http_smoke.py` com `CLOUDSHOPPING_DEMO_PASSWORD` definido, somente no ambiente sintético esperado pelo script. Os comandos de teste não servem para validar pagamentos externos ou lançamento produtivo.
