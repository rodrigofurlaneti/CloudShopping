# BE-11D — Conciliar cobranças e recuperar falhas de integração

**Módulo:** 11-pagamentos  
**Camada:** Backend  
**Etapa:** Venda · **Prioridade:** P1  
**Estado:** Planejado — não iniciado  
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

Webhook pode falhar e resposta de criação pode se perder; operação precisa recuperar estado sem cobrar outra vez.

**Evidência de origem:** módulo novo; docs.asaas.com/docs/polling-vs-webhooks-en. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [BE-11C](BE-11C.md) — Aplicar estados financeiros e efeitos uma única vez

## Escopo e detalhes

- Criar worker incremental por conta com janela sobreposta/cursor, limite de chamadas e backoff
- Conciliar tentativas sem retorno e eventos não vinculados; usar consulta do provedor e referência externa, tratando múltiplos resultados como divergência
- Criar fila de falhas/reprocessamento auditado e alertas de atraso; não depender de polling contínuo da vitrine

## Critérios de aceite

- [ ] Cobrança criada externamente com timeout local é vinculada sem nova cobrança
- [ ] Evento perdido é recuperado e passa pelo mesmo processador idempotente
- [ ] Falha permanente gera caso visível com correlationId; reiniciar worker não duplica efeito

## Entrega e verificação

Entregar implementação/contrato OpenAPI atualizado, testes dos cenários de aceite e evidência de integração em ambiente isolado. Operações monetárias só em sandbox durante a homologação. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [BE-11E](BE-11E.md), [FE-11A](FE-11A.md), [BE-21](BE-21.md), [BE-26](BE-26.md)


