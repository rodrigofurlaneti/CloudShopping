# CloudShopping.Tests.Units

Testes unitários em net9.0, com xUnit, FluentAssertions, Moq e coverlet.collector. O projeto referencia exclusivamente Domain e Application. Não usa banco, API, Asaas ou Infrastructure. Dependências de persistência são substituídas por mocks; entidades e handlers executados são reais.

## Execução

Na raiz do repositório:

```powershell
dotnet test 3-BackEnd/tests/CloudShopping.Tests.Units
& ./3-BackEnd/tests/CloudShopping.Tests.Units/Test-Coverage.ps1 -NoRestore
```

O segundo comando executa os testes, gera Cobertura XML em uma pasta exclusiva por execução e exige pelo menos **90% das linhas em cada camada separadamente**. Retorna código 1 se qualquer camada ficar abaixo da meta ou se os testes falharem. Ausência de relatório/camada também é erro. Branches são informados, mas não integram esse limite de linhas.

`coverage.runsettings` inclui integralmente os assemblies Domain e Application. Apenas código marcado `GeneratedCodeAttribute` é excluído; métodos assíncronos não são excluídos. Resultados BDD e de integração não são somados ao percentual desta suíte.

## Resultado verificado em 10/09/2026

**129 testes passaram; nenhum ignorado. A meta de 90% NÃO foi atingida.**

| Camada | Linhas | Branches |
|---|---:|---:|
| Domain | 50,37% | 40,60% |
| Application | 6,38% | 12,78% |

O comando de cobertura permanece falhando por esse motivo. Não interpretar a aprovação dos testes como aprovação da cobertura.

## Cenários presentes

- Domain: produtos, reservas e movimentações de estoque, carrinho, cupons, clientes B2C/B2B, CPF/CNPJ, pagamento idempotente, reserva encerrada e transições logísticas de pedidos.
- Application: adicionar item, criar/atualizar/excluir departamento, entrada e ajuste de estoque, isolamento entre empresas, auditoria de movimentação, validação antes dos efeitos, conversão de falhas em Result, propagação de cancelamento, criação/validação/revogação de sessões e expiração com relógio controlado.
- Asserts verificam estado resultante, código de erro e ausência/presença de gravações; não apenas ausência de exceções.

## Trabalho restante

`cobertura-pendente.csv` registra classes com linhas não executadas na última medição. Os maiores blocos em Application incluem checkout, cadastro de empresa, criação de produtos e validators de funcionários. Também faltam testes de diversos CRUDs, consultas, segurança de contas, relatórios, notificações e cenários complementares nas regras já testadas.

O pedido de cobertura de 90% permanece incompleto. As violações anteriores da suíte de arquitetura também não foram corrigidas por este projeto de testes.
