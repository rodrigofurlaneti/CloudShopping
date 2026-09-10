# BE-00A — Implementar dispatcher de outbox e consumidores idempotentes

**Módulo:** 01-base  
**Camada:** Backend  
**Etapa:** Fundação · **Prioridade:** P0  
**Estado:** Parcial — consulte evidências de entrega; aceite integral pendente.
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

O mecanismo atual limpa eventos em memória e publica após persistência, sem garantia de retomada.

**Evidência de origem:** CloudShopping.Infrastructure/Persistence/AppDbContext.cs; CloudShopping.Application/Features/Orders/Event/OrderCanceledDomainEventHandler.cs. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [DB-00](DB-00.md) — Persistir outbox transacional compartilhada
- [BE-01](BE-01.md) — Alinhar persistência, contratos de erro e configuração

## Escopo e detalhes

- Gravar eventos duráveis no commit e implementar worker com lease, retry limitado, backoff e fila de falhas.
- Definir tenant explicitamente em execução de background, sem TenantProvider cair na loja 1.
- Registrar resultado idempotente de consumidores; efeitos locais críticos de estoque continuam na transação apropriada.

## Critérios de aceite

- [ ] Encerrar processo entre commit e publicação não perde evento.
- [ ] Reentrega não duplica efeito já confirmado.
- [ ] Falha permanente aparece em fila auditável e reprocessamento preserva correlationId.

## Entrega e verificação

Entregar implementação/contrato OpenAPI atualizado, testes dos cenários de aceite e evidência de integração em ambiente isolado. Operações monetárias só em sandbox durante a homologação. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [BE-09](BE-09.md), [BE-11B](BE-11B.md), [BE-11C](BE-11C.md), [BE-16](BE-16.md)




## Progresso da entrega 3

**Parcial.** Outbox transacional e consumidor idempotente do portal; e-mail e dispatcher externo pendentes. Consulte [implementação e evidências](../08-entrega-operacao-e-modulos.md). Os critérios completos deste cartão continuam pendentes de aceite.
