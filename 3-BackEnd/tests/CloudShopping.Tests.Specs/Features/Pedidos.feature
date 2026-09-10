# language: pt
@dominio @pedidos @pagamentos
Funcionalidade: Preservar consistência financeira dos pedidos
  Esquema do Cenário: Confirmação de pagamento e repetição do evento
    Dado um pedido de 100 reais com frete 15 e reserva "<inicial>"
    Quando o provedor confirma o pagamento "pay-1" <vezes> vezes
    Então o pedido fica financeiro "<financeiro>", reserva "<reserva>" e bloqueado <bloqueado>
    E o pedido possui 1 pagamentos
    Exemplos:
      | inicial | vezes | financeiro | reserva | bloqueado |
      | ativa | 1 | Paid | Consumed | false |
      | ativa | 2 | Paid | Consumed | false |
      | liberada | 1 | PaidReview | Released | true |
  Cenário: Estorno exige bloqueio operacional
    Dado um pedido de 100 reais com frete 15 e reserva "ativa"
    Quando o provedor confirma o pagamento "pay-1" 1 vezes
    E o provedor confirma o estorno
    Então o pedido fica financeiro "Refunded", reserva "Consumed" e bloqueado true
    E o pedido possui 1 pagamentos
  Esquema do Cenário: Pedido pendente não pode pular etapas logísticas
    Dado um pedido de 100 reais com frete 15 e reserva "ativa"
    Quando a operação logística "<operacao>" é solicitada
    Então a etapa logística deve ser recusada sem mudar o pedido pendente
    Exemplos:
      | operacao |
      | faturar |
      | processar |
      | separar |
      | embalar |
      | etiquetar |
      | despachar |
  Esquema do Cenário: Desconto não abate frete nem excede produtos
    Dado um pedido de 100 reais com frete 15 e reserva "ativa"
    Quando o desconto aplicado no pedido é <valor>
    Então o pedido totaliza <total> e o desconto é "<resultado>"
    Exemplos:
      | valor | total | resultado |
      | 10 | 105 | aceito |
      | 100 | 15 | aceito |
      | 101 | 115 | rejeitado |
      | -1 | 115 | rejeitado |
