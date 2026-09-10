# language: pt
@dominio @estoque
Funcionalidade: Proteger o estoque disponível e as reservas
  Esquema do Cenário: <operacao> respeita as quantidades comprometidas
    Dado um produto com estoque físico <inicial> e reserva <reserva>
    Quando o estoque recebe a operação "<operacao>" com quantidade <quantidade>
    Então o estoque termina físico <fisico>, reservado <reservado>, disponível <disponivel> e resultado "<resultado>"
    Exemplos:
      | inicial | reserva | operacao | quantidade | fisico | reservado | disponivel | resultado |
      | 10 | 0 | reservar | 3 | 10 | 3 | 7 | aceito |
      | 10 | 3 | reservar | 8 | 10 | 3 | 7 | rejeitado |
      | 10 | 0 | reservar | 0 | 10 | 0 | 10 | rejeitado |
      | 10 | 0 | reservar | -1 | 10 | 0 | 10 | rejeitado |
      | 10 | 3 | baixar | 3 | 7 | 0 | 7 | aceito |
      | 10 | 3 | baixar | 4 | 10 | 3 | 7 | rejeitado |
      | 10 | 3 | liberar | 2 | 10 | 1 | 9 | aceito |
      | 10 | 3 | liberar | 4 | 10 | 3 | 7 | rejeitado |
      | 10 | 3 | ajustar | 3 | 3 | 3 | 0 | aceito |
      | 10 | 3 | ajustar | 2 | 10 | 3 | 7 | rejeitado |
      | 10 | 0 | ajustar | -1 | 10 | 0 | 10 | rejeitado |
      | 10 | 3 | entrada | 2 | 12 | 3 | 9 | aceito |
      | 10 | 0 | entrada | 0 | 10 | 0 | 10 | rejeitado |
      | 10 | 0 | entrada | -1 | 10 | 0 | 10 | rejeitado |
