# Auditoria da integração Asaas — 11/09/2026

## Parecer

A revisão encontrou e corrigiu divergências nos fluxos implementados. Isso não constitui homologação externa nem permite afirmar que a integração está “100% correta”. O aceite operacional depende de evidências no Sandbox, configuração das contas e entrega real de webhooks. Nenhuma cobrança, transferência ou alteração de conta foi executada no Asaas nesta auditoria.

Escopo: recebimento Direct e PlatformSplit, cliente, Pix dinâmico, boleto, cartão à vista em Checkout hospedado, cancelamento, estorno integral, conciliação, efeitos em estoque/expedição e recepção de webhooks. Foram inventariados os 18 anexos (incluindo cópias repetidas) e os quatro tópicos colados na solicitação. A documentação foi descoberta pelo [índice oficial](https://docs.asaas.com/llms.txt), seguida das páginas Markdown dos endpoints utilizados. As cópias consultadas estão em `.local/asaas-audit/`, ignoradas pelo Git. Graphify não estava disponível; esta é uma revisão direta do código, sem alegação de análise de grafo.

## Correções

| Achado | Efeito anterior | Comportamento corrigido |
|---|---|---|
| Cancelamento de Checkout aceitava qualquer HTTP de sucesso | Uma resposta `{}` liberava a reserva sem identificar o Checkout cancelado | Exige o mesmo `id` e `status=CANCELED`; resposta inconclusiva mantém reserva e aguarda webhook/conciliação |
| Comparação de dois IDs de Checkout nulos | Poderia dispensar a referência externa de uma cobrança de cartão | A alternativa por Checkout exige ID local não nulo e correspondente |
| Split bloqueado por divergência não era rejeitado | Podia aprovar pagamento e liberar expedição com `BLOCKED_BY_VALUE_DIVERGENCE` | Bloqueia também estornos de split em processamento/concluídos, duplicidade de carteiras e valor fixo inesperado |
| Estorno externo pendente não impedia novo POST | Solicitação administrativa podia iniciar outra devolução | Um estorno já observado impede novo pedido automático |
| `REFUND_IN_PROGRESS` após pagamento caía em divergência genérica | Estado local ficava `Review` em vez de acompanhar o estorno | Mantém `RefundPending` |
| Pedido em estorno ainda permitia expedição | O valor pago permanecia registrado e a logística podia continuar | Solicitação local ou estorno observado bloqueia expedição; não declara devolução concluída antecipadamente |
| Conclusão de estorno ignorava retornos de split incompletos | Registrava estorno integral mesmo com `refundedSplits[].done=false` | Mantém análise até os splits retornados confirmarem conclusão |
| Histórico de estornos com `hasMore=true` | Podia concluir a partir de uma lista parcial | Encaminha para análise; não inventa parâmetros de paginação ausentes da referência desse endpoint |
| Campos numéricos opcionais nulos | `TryGetDecimal` podia lançar exceção para JSON null | Aceita campos opcionais ausentes/nulos; identidade e valor da cobrança continuam conferidos |
| Lista malformada confundida com lista vazia | `{}` podia ser interpretado como inexistência de pagamentos/estornos | Ausência de array `data` ou array raiz exige conciliação |
| Parâmetros exclusivos de boleto enviados em Pix | Payload enviava cancelamento de registro e Correios também para Pix | Envia esses campos somente em `BOLETO` |

Fontes dos contratos: [cancelar Checkout](https://docs.asaas.com/reference/cancelar-um-checkout), [consultar cobrança](https://docs.asaas.com/reference/recuperar-uma-unica-cobranca), [listar cobranças e estados de split](https://docs.asaas.com/reference/listar-cobrancas), [estornos e devolução de splits](https://docs.asaas.com/docs/estornos), [listar estornos](https://docs.asaas.com/reference/listar-estornos-de-uma-cobranca), [criar cobrança](https://docs.asaas.com/reference/criar-nova-cobranca). Os bloqueios conservadores são decisões de segurança financeira da aplicação, não novos status atribuídos ao Asaas.

## Contratos conferidos

| Fluxo | Evidência no código | Referência |
|---|---|---|
| Autenticação e ambiente | Hosts fixos por ambiente, `access_token`, User-Agent, produção dependente de configuração e redirects HTTP desabilitados | [Autenticação](https://docs.asaas.com/docs/autenticação-1) |
| Identificação da conta | `GET wallets/`; carteira preservada como identidade da conta emissora | [WalletId](https://docs.asaas.com/reference/recuperar-walletid) |
| Cliente | Criação/atualização, ID persistido por conta e comprador, busca por referência após incerteza | [Criar cliente](https://docs.asaas.com/reference/criar-novo-cliente), [listar clientes](https://docs.asaas.com/reference/listar-clientes) |
| Notificações | `notificationDisabled=true` na criação e atualização do cliente | [Notificações](https://docs.asaas.com/docs/notificacoes) |
| Pix e boleto | `POST payments`, ID persistido, referência local, valor calculado pelo servidor e vencimento | [Criar cobrança](https://docs.asaas.com/reference/criar-nova-cobranca) |
| QR Code Pix | Consulta `payments/{id}/pixQrCode`; exibe `encodedImage` e `payload`; QR não aprova pagamento | [QR Code](https://docs.asaas.com/reference/obter-qr-code-para-pagamentos-via-pix) |
| Cartão | `POST checkouts`, `CREDIT_CARD`, `DETACHED`, 30 minutos, callback, itens e `splits`; confirma cobrança pela API | [Criar Checkout](https://docs.asaas.com/reference/criar-novo-checkout) |
| Cliente do Checkout | Envia `customer`, sem `customerData` simultâneo | [Identificação do cliente](https://docs.asaas.com/docs/como-informar-os-dados-do-cliente) |
| Link hospedado | Prioriza link válido retornado; fallback com `checkoutSession/show?id=` | [Link e callback](https://docs.asaas.com/docs/link-do-checkout-e-redirecionamento-do-cliente) |
| Webhook | Token independente, inbox persistida antes de HTTP 200, chave única conta/evento, worker e consulta autenticada | [Recepção](https://docs.asaas.com/docs/receba-eventos-do-asaas-no-seu-endpoint-de-webhook), [idempotência](https://docs.asaas.com/docs/como-implementar-idempotencia-em-webhooks) |
| Estorno | Pix/cartão em `/refund`, boleto em `/bankSlip/refund`; link do formulário não significa conclusão | [Estornar cobrança](https://docs.asaas.com/reference/estornar-cobranca), [estornar boleto](https://docs.asaas.com/reference/estornar-boleto) |

Há inconsistências na própria documentação: o guia de Checkout documenta `customer`, mas o schema OpenAPI consultado não o lista; o exemplo de `link` na referência usa caminho, enquanto o guia ensina query string. Foram preservados os comportamentos explicitamente descritos nos guias. Validá-los no Sandbox é parte obrigatória do aceite.

## Produtos documentados que não estão implementados

Os anexos também descrevem Pix Automático, assinaturas, Link de Pagamento, antecipação, notas fiscais, subcontas e integrações de terceiros/Flapp Store. O texto colado acrescenta transferências e Conta Escrow. A presença desses produtos na documentação não significa que sejam requisitos já entregues pelo CloudShopping.

Não existem fluxos próprios de recorrência, saques/Pix/TED, onboarding/KYC, emissão fiscal, antecipação, criação de chaves Pix ou configuração/liberação de Escrow. Checkout hospedado não equivale ao produto Link de Pagamento. Split existente utiliza carteiras previamente configuradas; não cria nem aprova subcontas.

Especialmente, split liquidado não comprova saldo disponível se o recebedor utiliza Escrow. O estado de pagamento do pedido não deve ser utilizado como saldo sacável. [Split com Escrow](https://docs.asaas.com/docs/split-para-contas-com-conta-escrow).

## Pendências para aceite externo

1. Configurar credenciais de Sandbox por conta emissora, carteiras recebedoras, percentual comercial e URL pública HTTPS da API. Informar segredos pelo mecanismo de configuração do servidor, sem registrá-los no Git.
2. Confirmar aprovação cadastral/KYC e manter chave Pix cadastrada em cada conta que recebe. A consulta de WalletId valida acesso à carteira, não aprovação cadastral, prontidão de Pix nem configuração de webhook. A documentação ainda descreve QR temporário sem chave, mas prevê descontinuação; esse fluxo não é garantia operacional. [Pix](https://docs.asaas.com/docs/cobrancas-via-pix).
3. Configurar os eventos utilizados: cobranças (criação, confirmação, recebimento, atualização, exclusão/restauração, vencimento, risco, estornos, chargebacks e split) e `CHECKOUT_CREATED`, `CHECKOUT_PAID`, `CHECKOUT_CANCELED`, `CHECKOUT_EXPIRED`. Não cadastrar categorias sem consumidor implementado. [Cobranças](https://docs.asaas.com/docs/webhook-para-cobrancas), [Checkout](https://docs.asaas.com/docs/eventos-para-checkout).
4. Executar Direct e PlatformSplit com Pix, boleto e cartão: pagamento, replay de webhook, cancelamento antes de pagar, estorno integral, pagamento tardio, divergência de split e isolamento entre duas lojas. Registrar ID do pedido, cobrança, checkout, evento, bruto/líquido, distribuição e resultado local, sem segredos.
5. Testar falhas de rede e retomada do worker, persistência das chaves Data Protection, rotação de credenciais e webhook com acesso externo. A recuperação de resposta perdida não deve criar outra cobrança.
6. Para boleto, validar o formulário de estorno e a conclusão externa. Se a resposta com `requestUrl` se perder, a referência de listagem de estornos não promete esse campo; investigar no Asaas, sem reenviar automaticamente a solicitação.
7. Definir acompanhamento operacional para inbox sem vínculo, estorno negado, criação incerta, `hasMore`, bloqueio financeiro e falhas de API. Ainda não há fila morta/alertas completos; a conciliação periódica não substitui monitoramento. Um estorno negado pode exigir tratamento manual e não é repetido automaticamente.

## Validação local

Os testes usam respostas Asaas simuladas e MySQL real, isolado em `127.0.0.1:33077`, com schemas temporários. A primeira tentativa ocorreu com o MySQL desligado; após iniciá-lo, houve também esgotamento de conexões na suíte ampla. O limite foi ajustado apenas nessa instância local. Um teste de relatório financeiro anterior estava lendo o envelope `Result` como se fosse `ReportSummary`; foi corrigido para validar sucesso e acessar `Value`.

Foi acrescentado `AsaasContractRegressionTests.cs`, com 22 casos para as divergências descritas, incluindo persistência do bloqueio de expedição quando a consulta do histórico de estorno falha. Os testes de pagamento existentes continuam cobrindo replay, concorrência, isolamento, perda de resposta, estoque, recuperação manual e estorno de boleto.

Resultado final: **97 testes aprovados, zero falhas, zero ignorados**, em 2 min 46 s. Evidência local: `.local/asaas-audit/asaas-final.trx` e `.local/asaas-audit/verification.log`. Build da API aprovado com zero erros; mantém aviso existente de licença ImageSharp. `git diff --check` aprovado. Não houve alteração de frontend ou migration nesta auditoria.

Reprodução a partir da raiz, com a instância MySQL isolada disponível:

```powershell
dotnet test 3-BackEnd/tests/CloudShopping.Tests --no-restore --logger 'trx;LogFileName=asaas-final.trx' --results-directory .local/asaas-audit
dotnet build 3-BackEnd/src/CloudShopping.Api/CloudShopping.Api.csproj --no-restore
git diff --check
```

Essas evidências demonstram os cenários automatizados locais; não comprovam liquidação, taxas, KYC, permissões de conta, comportamento real do Checkout ou entrega externa de webhooks. A homologação do Sandbox permanece pendente.
