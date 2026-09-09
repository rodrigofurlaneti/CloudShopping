# Contratos propostos e plano de homologação

## Convenções

Os caminhos abaixo são **propostos**, não afirmações de endpoints já implementados. Reutilizar/versionar os endpoints existentes quando compatíveis. Registrar o contrato definitivo no OpenAPI antes de concluir os cartões de frontend. O frontend nunca envia Amount como autoridade de preço nem TenantId como prova de acesso.

| Operação proposta | Entrada principal | Saída/garantia | Cartão |
|---|---|---|---|
| POST /auth/login; GET /auth/me; POST /auth/logout | Credenciais/contexto validado | Sessão, usuário e permissões | BE-02 |
| GET /store/context | Domínio validado | Marca, tenant público e recursos habilitados, sem segredo | BE-03/BE-19 |
| GET /products; GET /products/{id-ou-slug} | Filtros/página | Catálogo publicável e SKU vendável | BE-04 |
| GET /me/cart; POST /me/cart/items; PATCH/DELETE de item | SKU/quantidade/versionamento | Carrinho do titular, totais autoritativos | BE-07 |
| POST /shipping/quotes | Carrinho/endereço | quoteId, preço, prazo, expiresAt e hash de conteúdo | BE-08 |
| POST /checkout/preview | Carrinho/endereço/quote/cupom opcional | Itens, subtotal, entrega, desconto, total e versão | BE-09/BE-17 |
| POST /checkout/confirm | Referências + Idempotency-Key | Pedido/publicId e resultado estável para mesmo conteúdo | BE-09 |
| POST /orders/{publicId}/payment-attempts | Meio/parcelas permitidas + chave | ID tentativa, estado, dados seguros de apresentação | BE-11A |
| GET /me/orders/{publicId} | Identidade do titular | Snapshot, estado e ações permitidas | BE-12 |
| POST /integrations/asaas/webhooks/{connectionPublicId} | Token próprio + payload externo | Inbox durável e resposta rápida | BE-11B |
| POST /admin/payments/{id}/refunds | Valor elegível, motivo, chave | Solicitação pendente ou resultado já conhecido | BE-11E |
| GET /admin/orders | Filtros/paginação | Lista por tenant, sem limite invisível de 200 | BE-12 |
| POST /admin/orders/{id}/transitions | Ação, versão, campos exigidos | Novo estado e allowedActions | BE-12 |

Se a integração usar cookies, definir CSRF, SameSite, Secure e política de origens. Se usar bearer token, definir validade, revogação e armazenamento adequados. Não deixar duas estratégias implícitas convivendo por acidente.

## Exemplo de contrato de confirmação

Exemplo ilustrativo do nosso backend, sem dados reais:

```json
{
  "cartId": 123,
  "cartVersion": 4,
  "addressId": 45,
  "shippingQuoteId": "quote_exemplo",
  "acceptedTotal": "139.90"
}
```

acceptedTotal é apenas confirmação do valor que o consumidor revisou; o servidor recalcula. Idempotency-Key identifica a intenção e deve ser reutilizado em timeout/retry do mesmo conteúdo. Conteúdo alterado com a mesma chave retorna conflito; uma compra realmente nova usa nova chave. Tenant e CustomerId são derivados/validados pelo contexto, não confiados ao body.

Definir representação monetária única no contrato (exemplo usa string decimal), BRL e arredondamento. No banco usar decimal, no C# decimal; não permitir que arredondamento binário do navegador determine o valor cobrado.

## Resposta de erro

Padronizar ProblemDetails com status, código estável, mensagem segura, traceId e errors por campo. Diferenciar: input inválido (400), sem sessão (401), sem permissão (403), recurso não acessível/encontrado segundo política uniforme (404), conflito de versão/preço/idempotência (409) e falha transitória de integração (status definido no contrato). Não expor SQL, stack trace, segredos nem dados de outro tenant.

## Cenários de aceite integrados

| ID | Preparação/ação | Resultado verificável | Responsáveis |
|---|---|---|---|
| T01 | Subir schema vazio e migrar cópia sanitizada do legado | Mesmo modelo final; nenhum pedido perdido; restore ensaiado | DB-01/DB-26 |
| T02 | Criar pedido novo com histórico inicial | Pedido/itens/endereço/histórico ligados e salvos juntos | BE-00 |
| T03 | Usuário loja A troca header/body/ID por recurso B | Nenhuma leitura/mutação não autorizada | BE-03/FE-03 |
| T04 | Cliente A tenta pedido/endereço de cliente B da mesma loja | Posse protegida, além do isolamento de tenant | BE-06/BE-12 |
| T05 | Duas compras simultâneas da última unidade | Uma reserva; outra recebe indisponibilidade/conflito; saldos válidos | DB-05/BE-05 |
| T06 | Confirmar checkout duas vezes, perder resposta e recarregar | Um pedido, uma reserva, recuperação por chave | BE-09/FE-09 |
| T07 | Preço/CEP/quantidade muda após cotação | Cotação/preview revalidados, nova revisão; nenhum valor adulterado | BE-08/BE-09 |
| T08 | Timeout depois de Asaas criar cobrança | Estado incerto conciliado sem repetir cegamente criação | BE-11A/BE-11D |
| T09 | Mesmo webhook 10 vezes, inclusive concorrente | Uma transição/baixa/notificação por efeito | BE-11B/BE-11C |
| T10 | Token webhook inválido e evento de conta errada | Não aplicar mutação financeira | BE-11B/BE-11C |
| T11 | Webhook chega antes do vínculo local ou fora de ordem | Pendente é retomado; estado não regride | BE-11C/BE-11D |
| T12 | Callback de sucesso sem confirmação financeira | Tela aguarda; não autoriza expedição | FE-11 |
| T13 | Pagamento após reserva expirada e estoque esgotado | Não expedir; abrir tratamento/compensação conforme política | BE-05/BE-11C |
| T14 | Estorno parcial, duplicado ou negado | Saldo correto, uma solicitação por chave, pendência real exibida | BE-11E/FE-11A |
| T15 | Chargeback após pedido enviado | Caso financeiro separado, sem segunda baixa/restauração automática | BE-11C |
| T16 | Encerrar worker entre commit e publicação | Evento recuperado; sem perda e sem efeito duplicado | BE-00A/BE-25 |
| T17 | Editar só nome e rua em cadastro existente | Nascimento/IE/tipo/bairro preservados | BE-06A/FE-06A |
| T18 | Mais de 200 pedidos e duas ações seguidas no Kanban | Todos localizáveis; ações refletem estado novo | FE-12 |
| T19 | Gerar etiqueta/nota com falha do provedor | Não afirmar artefato existente; retry recuperável | BE-13/BE-14 |
| T20 | Devolver parcialmente, receber item duas vezes | Inspeção controla reposição e nenhum efeito duplicado | BE-15 |
| T21 | Enviar mensagem com provedor indisponível | Pedido/pagamento mantidos; retry e erro visível | BE-16 |
| T22 | Relatório com cancelamento/estorno/período de fronteira | Totais conciliam e timezone não duplica registros | BE-21 |
| T23 | Fluxo móvel, teclado, erro de API e sessão expirada | Jornada compreensível, recuperável e sem dados de outra sessão | FE-25/FE-26 |

## Condições para considerar a entrega pronta

Executar testes em MySQL compatível, dados sintéticos de dois tenants e sandbox Asaas; anexar evidências dos eventos e resultados com dados redigidos. Testes unitários validam regras, integração confirma persistência/concorrência, e E2E confirma jornada. Não substituir teste de gateway por clicar em aprovar no Kanban.

[DB-26](cartoes/DB-26.md), [BE-26](cartoes/BE-26.md) e [FE-26](cartoes/FE-26.md) são gates. Esta análise não executou esses cenários e não autoriza ativação de produção. O responsável de release deve confirmar decisões pendentes, configuração de conta, meios habilitados, operação fiscal/logística, alertas e recuperação antes de publicar comercialmente.

