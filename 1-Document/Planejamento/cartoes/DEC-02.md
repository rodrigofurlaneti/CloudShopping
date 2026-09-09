# DEC-02 — Fechar regras de venda, estoque e entrega

**Módulo:** 00-decisoes  
**Camada:** Definição  
**Etapa:** Fundação · **Prioridade:** P0  
**Estado:** Planejado — não iniciado  
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

O documento menciona reserva em carrinhos; o checkout atual reserva estoque apenas ao finalizar. Carrinho de 30 dias não deve implicar reserva de 30 dias sem decisão expressa.

**Evidência de origem:** 3-BackEnd/src/CloudShopping.Application/Features/Orders/Commands/Checkout/CheckoutCommandHandler.cs. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

Nenhuma dependência técnica anterior. Decisões devem ser registradas antes da implementação dependente.

## Escopo e detalhes

- Propor carrinho sem reserva e reserva curta no checkout; definir TTL por meio de pagamento
- Definir transportadora ou tabela de frete inicial, retirada, cobertura e produtos físicos/B2C/B2B
- Definir pagamento tardio: tentar reservar novamente; sem saldo, bloquear expedição e iniciar tratamento financeiro

## Critérios de aceite

- [ ] Regras descrevem expiração, corrida pagamento/cancelamento e boleto vencido
- [ ] Frete, parcelamento, arredondamento e política de devolução têm exemplos numéricos
- [ ] Decisões identificam dono e pendências; prazos comerciais não são inventados

## Entrega e verificação

Entregar decisão curta com alternativa escolhida, justificativa, responsável, data e impacto nos cartões; não marcar hipóteses como aprovadas. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [DB-05](DB-05.md), [DB-07](DB-07.md), [DB-08](DB-08.md)


