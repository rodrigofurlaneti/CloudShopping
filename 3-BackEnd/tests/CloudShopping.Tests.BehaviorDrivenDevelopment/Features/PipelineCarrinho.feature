# language: pt
@aplicacao @pipeline @carrinho
Funcionalidade: Validar comandos antes de executar efeitos no carrinho
  Esquema do Cenário: Dados de alteração <operacao> e quantidade <quantidade>
    Quando o pipeline altera o carrinho do cliente <cliente>, produto <produto>, operação "<operacao>" e quantidade <quantidade>
    Então a alteração pelo pipeline é aceita <aceita> e chama o serviço <chamadas> vezes
    Exemplos:
      | cliente | produto | operacao | quantidade | aceita | chamadas |
      | 1 | 10 | add | 1 | true | 1 |
      | 1 | 10 | set | 999 | true | 1 |
      | 1 | 10 | add | 1000 | false | 0 |
      | 1 | 10 | add | 0 | false | 0 |
      | 1 | 10 | set | -1 | false | 0 |
      | 0 | 10 | add | 1 | false | 0 |
      | 1 | 0 | add | 1 | false | 0 |
      | 1 | 10 | desconhecida | 1 | false | 0 |
      | 1 | 10 | remove | 0 | true | 1 |
      | 1 | 0 | clear | 0 | true | 1 |
