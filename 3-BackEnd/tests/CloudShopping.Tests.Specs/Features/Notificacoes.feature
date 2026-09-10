# language: pt
@aplicacao @notificacoes
Funcionalidade: Ler somente notificações do próprio cliente
  Esquema do Cenário: Leitura de notificação <condicao>
    Dado uma notificação na condição "<condicao>"
    Quando o cliente marca a notificação como lida
    Então a leitura da notificação retorna "<codigo>" e grava <gravacoes> vezes
    Exemplos:
      | condicao | codigo | gravacoes |
      | não lida | sucesso | 1 |
      | já lida | sucesso | 1 |
      | outro cliente | Command.NotFound | 0 |
