# language: pt
@dominio @cupons
Funcionalidade: Validar descontos, vigência e limites de cupons
  Esquema do Cenário: Restrições de cadastro
    Dado um cupom do tipo "<tipo>" com valor <valor>, mínimo <minimo>, limite <limite> e limite por cliente <cliente>
    Quando o cupom é criado
    Então o cadastro do cupom é "<resultado>"
    Exemplos:
      | tipo | valor | minimo | limite | cliente | resultado |
      | Fixed | 10 | 0 | 2 | 1 | aceito |
      | Percent | 100 | 0 | 2 | 1 | aceito |
      | Percent | 101 | 0 | 2 | 1 | rejeitado |
      | Fixed | 0 | 0 | 2 | 1 | rejeitado |
      | Fixed | -1 | 0 | 2 | 1 | rejeitado |
      | Fixed | 10,001 | 0 | 2 | 1 | rejeitado |
      | Fixed | 10 | -1 | 2 | 1 | rejeitado |
      | Fixed | 10 | 0 | 0 | 1 | rejeitado |
      | Fixed | 10 | 0 | 2 | 3 | rejeitado |
      | Fixed | 10 | 0 | 2 | 0 | rejeitado |
      | Outro | 10 | 0 | 2 | 1 | rejeitado |
  Esquema do Cenário: Normalização do código
    Dado o código do cupom é "<codigo>"
    Quando o cupom é criado
    Então o cadastro do cupom é "<resultado>"
    Exemplos:
      | codigo | resultado |
      | promo-10 | aceito |
      | a | rejeitado |
      | promo! | rejeitado |
      | | rejeitado |
  Cenário: Cupom expirado não pode ser cadastrado
    Dado a vigência do cupom já terminou
    Quando o cupom é criado
    Então o cadastro do cupom é "rejeitado"
  Esquema do Cenário: Contador de utilizações
    Quando o cupom é utilizado <uso> vezes e liberado <liberacao> vezes
    Então o contador do cupom é <contador> e a utilização foi "<resultado>"
    Exemplos:
      | uso | liberacao | contador | resultado |
      | 2 | 0 | 2 | aceita |
      | 3 | 0 | 2 | rejeitada |
      | 2 | 1 | 1 | aceita |
      | 0 | 1 | 0 | rejeitada |
