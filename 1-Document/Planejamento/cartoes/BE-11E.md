# BE-11E — Executar cancelamento financeiro e reembolso

**Módulo:** 11-pagamentos  
**Camada:** Backend  
**Etapa:** Venda · **Prioridade:** P1  
**Estado:** Planejado — não iniciado  
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

Payment.Refund altera apenas estado local; reembolsar depende de operação externa e resultado confirmado.

**Evidência de origem:** CloudShopping.Application/Features/Orders; docs.asaas.com/docs/webhook-para-cobrancas. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [BE-11D](BE-11D.md) — Conciliar cobranças e recuperar falhas de integração

## Escopo e detalhes

- Expor solicitação autorizada de cancelamento/estorno integral ou parcial com motivo e chave idempotente
- Checar saldo reembolsável, status e capacidades do meio; persistir intenção, chamar adaptador e aguardar confirmação
- Tratar estorno pendente/negado, pagamento simultâneo a cancelamento e chargeback sem duplicar compensação

## Critérios de aceite

- [ ] Repetir mesma solicitação gera um pedido de estorno; soma não excede valor elegível
- [ ] Erro externo não apresenta estorno como concluído
- [ ] Corrida pagamento/cancelamento produz estado consistente e caso de tratamento quando necessário

## Entrega e verificação

Entregar implementação/contrato OpenAPI atualizado, testes dos cenários de aceite e evidência de integração em ambiente isolado. Operações monetárias só em sandbox durante a homologação. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [FE-11A](FE-11A.md), [BE-15](BE-15.md), [BE-17](BE-17.md), [BE-26](BE-26.md)




## Progresso da entrega 2

Status: **Parcial**. Consulte [implementação de pagamentos, evidências e pendências](../07-entrega-pagamentos-asaas.md). A escolha inclui recebimento direto e plataforma com split. Testes locais não substituem homologação Asaas nem encerram os critérios amplos deste cartão.
