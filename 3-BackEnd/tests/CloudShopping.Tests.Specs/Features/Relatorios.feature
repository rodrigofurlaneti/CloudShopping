# language: pt
@aplicacao @relatorios
Funcionalidade: Exportar relatórios com limites e células seguras
  Esquema do Cenário: Limites de exportação
    Quando o relatório exporta <pedidos> pedidos em um período de <dias> dias
    Então a exportação é "<resultado>" e consulta o repositório <consultas> vezes
    Exemplos:
      | pedidos | dias | resultado | consultas |
      | 0 | 1 | aceita | 1 |
      | 5000 | 367 | aceita | 1 |
      | 5001 | 1 | rejeitada | 1 |
      | 1 | 368 | rejeitada | 0 |
      | 1 | 0 | rejeitada | 0 |
  Esquema do Cenário: Proteção de fórmulas na planilha
    Então a célula CSV "<texto>" é protegida contra fórmula
    Exemplos:
      | texto |
      | =1+1 |
      | +1 |
      | -1 |
      | @soma |
