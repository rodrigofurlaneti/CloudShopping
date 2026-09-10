# language: pt
@dominio @clientes
Funcionalidade: Preservar o tipo cadastral e validar documentos
  Esquema do Cenário: Conversão cadastral
    Dado um cliente do tipo "<origem>"
    Quando o cliente solicita conversão para "<destino>"
    Então o cliente permanece no tipo "<final>" com resultado "<resultado>"
    Exemplos:
      | origem | destino | final | resultado |
      | Guest | B2C | B2C | aceito |
      | Guest | B2B | B2B | aceito |
      | B2C | B2B | B2C | rejeitado |
      | B2B | B2C | B2B | rejeitado |
  Esquema do Cenário: Dígitos verificadores de CPF e CNPJ
    Então o documento fiscal "<documento>" tem validade <valido>
    Exemplos:
      | documento | valido |
      | 52998224725 | true |
      | 11144477735 | true |
      | 11222333000181 | true |
      | 11111111111 | false |
      | 52998224724 | false |
      | 00000000000000 | false |
      | 123 | false |
