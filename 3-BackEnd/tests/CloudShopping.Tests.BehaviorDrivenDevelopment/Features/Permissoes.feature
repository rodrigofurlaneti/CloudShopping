# language: pt
@dominio @acesso
Funcionalidade: Conceder somente permissões coerentes
  Esquema do Cenário: Dependência de consulta antes de alteração
    Dado as permissões solicitadas "<permissoes>"
    Quando a política valida o conjunto de permissões
    Então o conjunto de permissões é "<resultado>"
    Exemplos:
      | permissoes | resultado |
      | catalog.read,catalog.write | aceito |
      | catalog.write | rejeitado |
      | stock.write | rejeitado |
      | catalog.read,stock.write | aceito |
      | finance.refund | rejeitado |
      | finance.read,finance.refund | aceito |
      | catalog.read,catalog.read | aceito |
      | inexistente.read | rejeitado |
      | * | rejeitado |
      | | aceito |
  Esquema do Cenário: Autorização efetiva
    Dado as permissões solicitadas "<permissoes>"
    Então a autorização para "<recurso>" deve ser <permitido>
    Exemplos:
      | permissoes | recurso | permitido |
      | * | finance.refund | true |
      | catalog.read | stock.write | false |
      | catalog.read | catalog.read | true |
      | | catalog.read | false |
