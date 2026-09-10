# language: pt
@dominio @carrinho
Funcionalidade: Gerenciar os itens do carrinho
  Esquema do Cenário: Alteração <operacao> dos itens
    Dado um carrinho com 2 unidades do produto 10 a 10 reais
    Quando o cliente executa "<operacao>" com quantidade <quantidade> no carrinho
    Então o carrinho deve ter <unidades> unidades e total <total> com resultado "<resultado>"
    Exemplos:
      | operacao | quantidade | unidades | total | resultado |
      | somar | 3 | 5 | 50 | aceito |
      | definir | 1 | 1 | 10 | aceito |
      | definir | 0 | 2 | 20 | rejeitado |
      | definir | -1 | 2 | 20 | rejeitado |
      | remover | 0 | 0 | 0 | aceito |
      | limpar | 0 | 0 | 0 | aceito |
      | ausente | 1 | 2 | 20 | rejeitado |
