# language: pt
@aplicacao @catalogo
Funcionalidade: Atualizar catálogo sem sobrescrever alterações concorrentes
  Esquema do Cenário: Edição de catálogo <condicao>
    Dado uma edição de catálogo na condição "<condicao>"
    Quando o caso de uso atualiza os detalhes do catálogo
    Então a edição retorna "<codigo>" e persiste <gravacoes> vezes
    Exemplos:
      | condicao | codigo | gravacoes |
      | válida | sucesso | 1 |
      | ausente | Command.NotFound | 0 |
      | versão antiga | Command.Conflict | 0 |
      | duplicado | Command.Conflict | 0 |
      | atributos excedidos | Command.Invalid | 0 |
      | slug inválido | Command.Invalid | 0 |
