# language: pt
@aplicacao @cupons
Funcionalidade: Persistir cupons na loja atual com proteção de concorrência
  Esquema do Cenário: Cadastro <condicao>
    Dado um cadastro de cupom na condição "<condicao>"
    Quando o caso de uso cadastra o cupom na loja atual
    Então a operação de cupom retorna "<codigo>" e grava <gravacoes> vezes
    Exemplos:
      | condicao | codigo | gravacoes |
      | válido | sucesso | 1 |
      | duplicado | Command.Conflict | 0 |
      | valor inválido | Command.Invalid | 0 |
  Esquema do Cenário: Ativação <condicao>
    Dado um cadastro de cupom na condição "<condicao>"
    Quando o caso de uso altera a ativação do cupom
    Então a operação de cupom retorna "<codigo>" e grava <gravacoes> vezes
    Exemplos:
      | condicao | codigo | gravacoes |
      | válido | sucesso | 1 |
      | sem alteração | sucesso | 0 |
      | ausente | Command.NotFound | 0 |
      | concorrente | Command.Conflict | 0 |
