# Entrega 2 — pagamentos Asaas

Implementação em 09/09/2026. Atende à escolha do usuário de oferecer **ambas as soluções**: recebimento direto pelo lojista e recebimento pela plataforma com split. Cada loja seleciona um modo; cada tentativa preserva a conta emissora e a distribuição usadas na criação. A integração foi testada localmente com MySQL real e respostas simuladas do Asaas. **Ainda não foi homologada no sandbox externo nem ativada em produção.**

## Comportamento entregue

| Camada | Implementação |
|---|---|
| Banco | Migration `004_asaas.sql`: conexões protegidas, clientes por conta, tentativa única por pedido, identificadores externos únicos, inbox idempotente, operações auditadas e estado financeiro separado do pedido. |
| Backend | Pix e boleto por cobrança; cartão à vista por checkout hospedado; conciliação pela API da conta emissora; webhook autenticado persistido antes da confirmação; worker; recuperação de resposta perdida; cancelamento e estorno integral. |
| Frontend | Pagamento em Meus pedidos, QR/copia e cola Pix, links de boleto/cartão, estado financeiro e bloqueios; configuração Asaas e painel de conciliação, recuperação e estornos no administrativo. |
| Estoque | Reserva mantida durante incerteza financeira; baixa e lançamento financeiro uma única vez; pagamento tardio após liberação exige análise; estorno não repõe automaticamente mercadoria física. |

Não há aprovação financeira por retorno do navegador. A aplicação consulta a cobrança e confere conta, referência, cliente, meio, valor e split antes de aprovar. Para Pix/boleto aguarda `RECEIVED`; para cartão também aceita `CONFIRMED`. A criação ou visualização de QR/link não significa pagamento.

## Configuração dos dois modos

Aplicar migrations com o executor existente, após backup e ensaio em homologação. Nesta entrega foi atualizado somente `127.0.0.1:33077/cloudshopping_dev`, com dados sintéticos. O dump anexado não foi alterado. Preservar as chaves de ASP.NET Data Protection entre deploys e réplicas: elas protegem credenciais e conteúdo da inbox.

O administrador acessa `/admin/asaas`. O painel financeiro fica em `/admin/payments`.

**Direto (`Direct`)**: escolher Sandbox, informar a API Key da conta do lojista e um token de webhook independente, sem espaços, entre 32 e 255 caracteres. Ao salvar, o backend consulta a carteira para verificar a conta. Cadastrar no painel Asaas a URL HTTPS pública formada pela origem da API e pelo `webhookPath` exibido na tela. O lojista recebe na própria conta; nenhuma regra de split é enviada.

**Plataforma com split (`PlatformSplit`)**: o operador configura os valores abaixo no servidor e o administrador da loja seleciona esse modo. Os percentuais não são aceitos do comprador ou de um formulário de checkout.

```text
Asaas__AllowProduction=false
Asaas__StoreUrls__1=https://loja.example.com
Asaas__Platform__Sandbox__ApiKey=<segredo da conta emissora da plataforma>
Asaas__Platform__Sandbox__WebhookToken=<token independente de 32 a 255 caracteres>
Asaas__Platform__Sandbox__MerchantWallets__1=<walletId do lojista 1>
Asaas__Platform__Sandbox__MerchantPercent__1=90
```

Substituir `1` pelo TenantId real e repetir carteira, percentual e URL para cada loja. O exemplo de 90% é ilustrativo: o negócio ainda precisa aprovar taxas, comissão e responsabilidade operacional pelos estornos. O percentual é do valor líquido; o restante permanece com a conta emissora. A carteira recebedora deve ser diferente da emissora. [Split Asaas](https://docs.asaas.com/docs/split-de-pagamentos).

Webhook compartilhado de sandbox da plataforma: `/api/webhooks/asaas/platform-sandbox`. Produção usa `platform-production` e configurações equivalentes sob `Asaas__Platform__Production`. O emissor da plataforma não pode ser trocado pela tela após uso; isso exige migração financeira. Rotação de chave da mesma conta atualiza conexões históricas da loja reconfigurada; reconfigurar todas as lojas envolvidas quando a plataforma revogar uma chave compartilhada.

Produção permanece bloqueada por padrão. Só habilitar `Asaas__AllowProduction=true` após homologação, configuração dos ambientes reais, revisão de licenças/dependências e aprovação operacional. Nunca colocar API Keys no frontend, no Git ou em documentação. A URL de retorno da loja vem do servidor; não é fornecida pelo comprador.

## Webhook e conciliação

Configurar envio com token no cabeçalho `asaas-access-token`. Habilitar eventos de cobrança, pagamento recebido/confirmado, exclusão, vencimento, estorno, chargeback e split, além de `CHECKOUT_CREATED`, `CHECKOUT_PAID`, `CHECKOUT_CANCELED` e `CHECKOUT_EXPIRED`. Incluir os eventos de estorno em andamento, parcial e negado. As referências oficiais apresentam a lista atual: [cobranças](https://docs.asaas.com/docs/webhook-para-cobrancas), [checkout](https://docs.asaas.com/docs/eventos-para-checkout) e [recepção autenticada](https://docs.asaas.com/docs/receive-asaas-events-at-your-webhook-endpoint).

O endpoint recebe sem sessão do comprador; o token do Asaas é obrigatório. Ele identifica a conta sem confiar em um TenantId recebido no corpo. Eventos duplicados têm chave única por conta/evento. O worker consulta a API autenticada e aplica efeitos sob trava por pedido. Respostas perdidas não provocam nova criação automática de cobrança.

Monitorar `asaasinbox` com `ProcessedAt IS NULL`, idade de `ReceivedAt`, `Attempts` e `LastError`, além das tentativas `Unknown`, `Review`, `CancelPending` e `RefundPending`. Eventos sem vínculo são mantidos para nova tentativa; ainda não existe tela de fila morta, alerta automático ou política de retenção. Não apagar a inbox para contornar falhas.

Uma tentativa desconhecida pode ser recuperada no painel financeiro informando o ID `pay_...` encontrado na conta emissora. A referência externa é `csp_` seguida do ID local da tentativa. A recuperação confere os dados remotamente antes de vincular e registra o administrador. Se um checkout foi criado, mas sua resposta e identificação se perderam antes de existir uma cobrança, investigar na conta Asaas; não liberar estoque nem repetir a criação às cegas.

## Cancelamento, estorno e operação

- Solicitar cancelamento não equivale a confirmação. A reserva é liberada depois da confirmação financeira de encerramento; uma cobrança incerta mantém a reserva.
- Cobrança recebida exige estorno. O painel pede confirmação de estorno integral e registra o operador; repetir a ação não gera outra solicitação automática.
- Para Pix/cartão, a integração solicita estorno e consulta seu andamento. Para boleto, abre o fluxo específico e apresenta ao comprador o link para informar dados bancários/documentos. Só registra `Refunded` quando a consulta comprova estorno integral concluído (`DONE`). [Estornar cobrança](https://docs.asaas.com/reference/estornar-cobranca), [estornar boleto](https://docs.asaas.com/reference/estornar-boleto), [consultar estornos](https://docs.asaas.com/reference/listar-estornos-de-uma-cobranca).
- Pagamento tardio, valor divergente, chargeback ou estorno parcial exige análise e bloqueia a expedição. A operação logística deve respeitar `FulfillmentBlocked`; o módulo completo de expedição ainda não foi entregue.
- Mercadoria devolvida precisa de conferência física; não há reposição automática no estorno.

## Evidências de validação

27 testes automatizados aprovados: 25 cenários com banco MySQL isolado (8 da base e 17 de pagamentos) e 2 testes do gateway HTTP. Cobertura inclui replay, concorrência, perda de resposta, expiração incerta, conta/cliente/valor divergente, split, cartão hospedado, estorno de boleto, evento de outra conta, cancelamento ambíguo, recuperação manual e produto retirado do catálogo depois da reserva. O Asaas é simulado nesses testes; não são evidência de liquidação externa.

Build .NET e build TypeScript/Vite aprovados. Smoke HTTP aprovado contra a API e banco locais: sessão, tenant, checkout, autorização de administrador/cliente, CSRF e contrato de pagamento não configurado. ESLint dos arquivos novos de pagamentos aprovado. O lint global conserva a dívida anterior de 10 erros registrada na entrega 1. Build apresenta aviso de licença ImageSharp; a revisão de licenças é pendência de lançamento. Nenhuma transação real foi executada.

Para reproduzir: `dotnet test 3-BackEnd/tests/CloudShopping.Tests --no-restore`; `npm.cmd run build` em `4-FrontEnd`; smoke `3-BackEnd/tests/http_smoke.py` somente com o ambiente de demonstração isolado e `CLOUDSHOPPING_DEMO_PASSWORD` configurado.

## Homologação restante e cartões

No sandbox externo, executar Direct e PlatformSplit com Pix, boleto e cartão; comparar bruto, líquido e recebedores; repetir webhooks; simular perda de resposta; cancelar antes do pagamento; estornar cada meio e aguardar conclusão; verificar pagamento tardio, isolamento entre duas lojas, reinício do worker e persistência das chaves. Registrar IDs/evidências sem segredos. A URL localhost não recebe webhooks externos. [Ambiente sandbox](https://docs.asaas.com/docs/sandbox-2).

A escolha de dois modelos resolve a direção técnica de DEC-01, mas titularidade real, percentuais comerciais e responsáveis ainda precisam ser preenchidos por loja. DB-10/11, BE-10/11/11A–E, FE-10/11/11A avançam parcialmente; DB-05/BE-05 recebem baixa financeira e auditoria. DB-23/BE-23 têm configuração e execução de split, sem onboarding/KYC completo. Os cartões amplos continuam com seus critérios originais e não estão integralmente aceitos.

Limites deste recorte: cartão à vista; uma tentativa por pedido, sem troca de meio após iniciar; sem cadastro automático de subcontas/KYC, gestão de saques, estorno parcial solicitado pelo painel, devoluções físicas completas, emissão fiscal, transportadora, alertas operacionais ou deploy. A próxima entrega pode estruturar operação de pedidos, expedição e devoluções, após a homologação financeira.
