# Integração Asaas — desenho proposto

Verificação da documentação oficial em 09/09/2026. A integração abaixo é proposta; nenhum endpoint Asaas foi chamado, conta configurada ou valor movimentado.

## Conta emissora é a primeira decisão

[DEC-01](cartoes/DEC-01.md) permanece pendente. Duas alternativas: cada lojista usa sua conta emissora; ou a plataforma opera subcontas/divisão de recebimento. Em ambas, toda tentativa precisa identificar tenant, conta e ambiente. Não usar uma chave global implicitamente para vendas de todas as lojas.

Se split for adotado, incluir [DB-23](cartoes/DB-23.md), [BE-23](cartoes/BE-23.md) e [FE-23](cartoes/FE-23.md) no gate de lançamento. As regras e beneficiários são determinados pelo servidor e salvos por cobrança. O split exige identificar contas/recebedores e seus walletIds; validar condições do produto contratado e base de cálculo na documentação. [Asaas: Split](https://docs.asaas.com/docs/split).

## Formas de pagamento

| Forma | Proposta para esta versão | Apresentação |
|---|---|---|
| Pix | Criar cobrança associada ao pedido | Dados de QR/copia e cola fornecidos pela integração |
| Boleto | Criar cobrança associada ao pedido | Link e linha digitável quando disponíveis |
| Cartão | Criar Checkout hospedado Asaas | Redirecionamento; parcelamento conforme configuração aprovada |

A API de cobranças oferece modalidades avulsas/parceladas por Pix, boleto ou cartão; este desenho usa cobrança para Pix/boleto. [Criar cobrança](https://docs.asaas.com/reference/criar-nova-cobranca).

O endpoint de Checkout hospedado documenta PIX e CREDIT_CARD; boleto não deve ser prometido nesse mesmo componente. Criar Checkout inicia a jornada, e URLs de callback controlam redirecionamento, sem confirmar pagamento. A documentação orienta acompanhar eventos próprios de Checkout. [Criar Checkout](https://docs.asaas.com/reference/criar-novo-checkout).

No fluxo hospedado proposto, número de cartão e CVV não passam pelo aplicativo. Se futuramente for escolhido cartão transparente, criar refinamento específico de captura/tokenização, controles e requisitos aplicáveis; não reutilizar esta estimativa como se as duas jornadas fossem idênticas.

## Modelo de dados proposto

As tabelas abaixo são conceituais; nomes finais e migrations serão definidos nos cartões. Não existem hoje só por constarem neste plano.

| Registro | Responsabilidade | Chave de consistência |
|---|---|---|
| PaymentProviderAccounts | Conta, tenant, ambiente, configuração e referência ao segredo | Escopo tenant+conta+ambiente |
| CustomerProviderAccounts | Vínculo consumidor local ↔ pagador externo | Cliente+conta+ambiente |
| CheckoutAttempts | Repetição segura da confirmação do pedido | Tenant+titular+chave e hash do conteúdo |
| Payments/PaymentAttempts | Intenção, tentativa, valor e IDs externos, estado financeiro | Conta+ambiente+ID externo; chave da operação local |
| WebhookInbox | Recebimento durável/deduplicação de evento | Conta+ambiente+eventId |
| OutboxMessages | Efeito a publicar após commit, com retomada | Chave lógica de evento/efeito |
| RefundRequests | Solicitação e confirmação de estorno parcial/total | Chave de operação e pagamento original |
| StockReservations | Vínculo de quantidade ao pedido com prazo/estado | Pedido+item+operação |

Separar as seguintes medidas: total comercial do pedido, valor cobrado, valor confirmado, valor recebido, taxa, líquido e valor estornado. Um pagamento parcialmente estornado não torna o pedido inteiro reembolsado. Nunca reconstruir a venda usando preço/endereço atual do cadastro.

## Fluxo transacional

1. Identificar loja e titular; recalcular itens/frete/descontos no servidor.
2. Em transação local, validar versão/chave, criar pedido e snapshot, reservar estoque e persistir a intenção na outbox. Responder com referência recuperável.
3. O processador resolve conta/pagador, grava tentativa e chama Asaas fora da transação SQL longa.
4. Salvar ID/link/estado retornados. Se o resultado externo for incerto, marcar para conciliar antes de recriar.
5. Receber webhook autenticado, persistir Inbox e confirmar recebimento. Processar em worker com tenant explícito.
6. Aplicar a matriz financeira e efeitos locais de maneira idempotente; liberar/consumir reserva somente segundo a regra definida.
7. Atualizar UI consultando o backend. Conciliar divergências e pedidos sem evento; nunca pedir ao navegador que confirme o pagamento.

**Idempotência local e correlação externa são distintas:** externalReference ajuda a relacionar registros, mas não presumimos que repetir POST com a mesma referência impeça duplicação no provedor. Timeout após criação precisa de recuperação/consulta ou tratamento manual auditado quando não for possível identificar resultado com segurança. O próprio guia de erros orienta verificar a primeira criação antes de recriar Checkout. [Erros e boas práticas](https://docs.asaas.com/docs/erros-comuns-e-boas-pr%C3%A1ticas).

## Webhooks e estados

Validar o token próprio do webhook no header asaas-access-token e vincular ao cadastro da conexão; não usar o X-Tenant-Id do caller como autorização. A documentação diferencia token de webhook de API Key. [Configurar webhook](https://docs.asaas.com/docs/criar-novo-webhook-pela-aplicacao-web).

A entrega pode ocorrer mais de uma vez. O projeto deve responder 2xx após persistência durável, executar efeitos de forma assíncrona, deduplicar eventos e monitorar falhas/atrasos. Consulta de conciliação complementa os webhooks; a vitrine consulta nossa API, não faz polling direto constante no Asaas. [Polling versus webhooks](https://docs.asaas.com/docs/polling-vs-webhooks-en).

| Entrada externa | Tratamento interno proposto |
|---|---|
| Cobrança/Checkout criado | Aguardando; não liberar expedição |
| Cartão autorizado/risco pendente | Estado próprio; não equiparar a quitação |
| PAYMENT_CONFIRMED | Registrar confirmação; elegibilidade operacional segue DEC-03 |
| PAYMENT_RECEIVED | Registrar recebimento; não baixar estoque outra vez se a confirmação já produziu o efeito |
| Vencido/cancelado/expirado | Reavaliar reserva e permitir recuperação conforme política, sem ignorar recebimento tardio |
| Estorno solicitado/em processamento | Manter reembolso pendente |
| Estorno parcial/total confirmado | Atualizar valor e efeito correspondente uma vez |
| Chargeback | Abrir caso financeiro; não modelar simplesmente como recusa inicial |
| CHECKOUT_PAID | Registrar resultado da jornada e correlacionar cobrança(s); aplicar regra financeira sem duplicar evento de pagamento |

Os eventos de cobrança distinguem confirmação, recebimento, estornos e chargebacks; guardar o estado original recebido junto do estado canônico adotado. A matriz acima é nossa decisão de implementação, a validar com os payloads sandbox e regras do negócio. [Eventos de cobranças](https://docs.asaas.com/docs/webhook-para-cobrancas). Os eventos de Checkout e sua correlação precisam seguir o contrato específico. [FAQ do Checkout](https://docs.asaas.com/docs/faq-do-asaas-checkout).

## Corridas e exceções obrigatórias

- Pagamento depois de expirar reserva: tentar nova reserva atômica se a política permitir; sem estoque, bloquear envio e criar tratamento financeiro. Não reabrir silenciosamente pedido cancelado.
- Callback chega antes do webhook: mostrar aguardando confirmação e permitir atualizar/retomar.
- Webhook chega antes de salvar ID da cobrança: manter evento pendente de vínculo e conciliar; não descartá-lo.
- Evento atrasado indica estado antigo: não regredir estado confirmado; consultar provedor se a ordem não for suficiente.
- Cliente tenta pagar novamente: recuperar intenção existente; caso haja recebimento duplicado, registrar divergência e compensação, sem dupla expedição.
- Estorno falha ou fica pendente: não afirmar concluído nem repor estoque automaticamente. Retorno físico depende de inspeção de pós-venda.

Essas são escolhas de consistência do ecommerce, não promessas de ordenação/transação distribuída do Asaas.

## Fiscal e assinatura SaaS

Asaas documenta emissão de **NFS-e de serviços**. Isso não substitui, por inferência, a integração de nota fiscal de mercadorias. [Notas fiscais Asaas](https://docs.asaas.com/docs/notas-fiscais). O módulo 14 define provedor/processo próprio para as vendas; módulo 24 trata eventual mensalidade da plataforma, separando receitas, clientes externos e eventos das compras.

