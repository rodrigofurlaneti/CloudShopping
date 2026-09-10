# language: pt
@aplicacao @sessoes @isolamento
Funcionalidade: Validar a identidade atual e o contexto da empresa
  Esquema do Cenário: Sessão precisa continuar válida no servidor
    Dado uma sessão administrativa na condição "<condicao>"
    Quando a sessão administrativa é validada
    Então a sessão é aceita <aceita>
    Exemplos:
      | condicao | aceita |
      | válida | true |
      | ausente | false |
      | expirada | false |
      | revogada | false |
      | outra loja | false |
      | outro titular | false |
      | outro papel | false |
      | senha alterada | false |
      | inativa | false |
      | sem permissão | false |
      | loja inativa | false |
  Esquema do Cenário: A loja solicitada não pode trocar o escopo autenticado
    Quando a loja é resolvida com autenticada <autenticada>, solicitada <solicitada> e rota <rota>
    Então a resolução da loja resulta em "<resultado>"
    Exemplos:
      | autenticada | solicitada | rota | resultado |
      | 1 | 1 | 1 | None |
      | 1 | 2 | 0 | Forbidden |
      | 1 | 1 | 2 | Forbidden |
      | 0 | 0 | 0 | Missing |
      | 0 | 2 | 0 | NotFound |
      | 0 | 1 | 1 | None |
