# BE-11C — Aplicar estados financeiros e efeitos uma única vez

**Módulo:** 11-pagamentos  
**Camada:** Backend  
**Etapa:** Venda · **Prioridade:** P1  
**Estado:** Planejado — não iniciado  
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

Order.UpdatePaymentApproved marca pedido pago sem checar quitação integral; estados locais são insuficientes.

**Evidência de origem:** CloudShopping.Domain/Entities/Orders/Order.cs; docs.asaas.com/docs/webhook-para-cobrancas. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [BE-11B](BE-11B.md) — Receber e persistir webhooks autenticados
- [BE-11A](BE-11A.md) — Criar cobranças Pix/boleto e checkout de cartão
- [BE-05](BE-05.md) — Implementar ciclo atômico de reserva e baixa
- [BE-00A](BE-00A.md) — Implementar dispatcher de outbox e consumidores idempotentes

## Escopo e detalhes

- Implementar worker Inbox com lock/versionamento e matriz DEC-03; validar conta, pedido, valor e vínculo externo
- Tratar confirmação/recebimento, risco/recusa, vencimento, cancelamento, estorno parcial/total e chargeback; separar jornada checkout do financeiro
- Emitir efeitos de estoque/histórico/notificação por outbox; evento atrasado não regride estado nem reabre pedido cancelado sem regra

## Critérios de aceite

- [ ] Recebido antes de confirmado não regride pedido; duplicata não duplica baixa nem histórico
- [ ] Valor divergente ou pagamento de pedido cancelado vai à análise/compensação, sem liberar expedição
- [ ] Autorização de cartão sem captura não conta como quitação; parcial não marca pedido integralmente estornado

## Entrega e verificação

Entregar implementação/contrato OpenAPI atualizado, testes dos cenários de aceite e evidência de integração em ambiente isolado. Operações monetárias só em sandbox durante a homologação. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [BE-11D](BE-11D.md), [FE-11](FE-11.md), [BE-12](BE-12.md)


