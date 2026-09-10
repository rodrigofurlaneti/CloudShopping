# language: pt
@aplicacao @autenticacao
Funcionalidade: Cadastrar e autenticar clientes e administradores
  Esquema do Cenário: Autenticação <operacao> em condição <condicao>
    Dado uma autenticação na condição "<condicao>"
    Quando o caso de uso executa "<operacao>"
    Então a autenticação retorna "<resultado>", grava conta <contas> vezes e emite sessão <sessoes> vezes
    Exemplos:
      | operacao | condicao | resultado | contas | sessoes |
      | login administrativo | válida | aceita | 0 | 1 |
      | login administrativo | ausente | credenciais inválidas | 0 | 0 |
      | login administrativo | senha incorreta | credenciais inválidas | 0 | 0 |
      | login administrativo | funcionário inativo | proibida | 0 | 0 |
      | login administrativo | sem permissões | proibida | 0 | 0 |
      | login cliente | válida | aceita | 0 | 1 |
      | login cliente | ausente | credenciais inválidas | 0 | 0 |
      | login cliente | senha incorreta | credenciais inválidas | 0 | 0 |
      | login cliente | sessão administrativa | proibida | 0 | 0 |
      | cadastro | válida | aceita | 1 | 1 |
      | cadastro | email duplicado | conflito | 0 | 0 |
      | cadastro | senha curta | dados inválidos | 0 | 0 |
      | cadastro | senha excedida | dados inválidos | 0 | 0 |
      | cadastro | sessão administrativa | proibida | 0 | 0 |
      | visitante | válida | aceita | 1 | 1 |
      | visitante | sessão administrativa | conflito | 0 | 0 |
