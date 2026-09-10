# language: pt
@aplicacao @acesso
Funcionalidade: Alterar permissões com autorização, concorrência e auditoria
  Esquema do Cenário: Substituição de permissões <condicao>
    Dado uma substituição de permissões na condição "<condicao>"
    Quando o caso de uso substitui as permissões
    Então a substituição retorna "<codigo>" e registra <gravacoes> alterações
    Exemplos:
      | condicao | codigo | gravacoes |
      | válida | sucesso | 1 |
      | sem alteração | sucesso | 0 |
      | sem autorização | Command.Forbidden | 0 |
      | ausente | Command.NotFound | 0 |
      | administrador geral | Command.Conflict | 0 |
      | concorrente | Command.Conflict | 0 |
