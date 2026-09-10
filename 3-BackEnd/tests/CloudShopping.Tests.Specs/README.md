# CloudShopping.Tests.Specs

Projeto BDD em **net9.0**, com Reqnroll.xUnit 3.3.4, Microsoft.NET.Test.Sdk 17.14.1, xunit.runner.visualstudio 2.8.2, FluentAssertions 6.12.2, Moq 4.20.72 e coverlet.collector 6.0.4.

Referências de projeto: **Domain e Application somente**. Não conecta a MySQL nem faz chamadas ao Asaas. Mocks substituem contratos de repositório e serviços; handlers, validators, pipeline e entidades reais executam as regras. Isso não substitui os testes de integração existentes em CloudShopping.Tests.

## Executar

A partir da raiz do repositório:

```powershell
dotnet test 3-BackEnd/tests/CloudShopping.Tests.Specs
dotnet test 3-BackEnd/tests/CloudShopping.Tests.Specs --collect:"XPlat Code Coverage" --settings 3-BackEnd/tests/CloudShopping.Tests.Specs/coverage.runsettings
```

No Visual Studio, abra `3-BackEnd/src/CloudShopping.sln`, compile e use o Test Explorer. Os arquivos `.feature.cs` são gerados pelo Reqnroll e não devem ser editados ou versionados.

## Organização

- `Features`: critérios de negócio em Gherkin, português; exemplos com casas decimais usam vírgula.
- `Steps`: bindings por assunto; fixtures isoladas por cenário; Moq verifica que rejeições não persistem alterações.
- Tags: `@dominio`, `@aplicacao` e módulos (`@estoque`, `@sessoes`, etc.).
- `coverage.runsettings`: cobertura somente de Domain/Application.

## Cobertura implementada

154 exemplos executáveis: estoque/reservas, carrinho, limites do pipeline, catálogo/conflitos, cupons/cadastro/ativação, clientes/documentos, pedidos/pagamento/estorno/logística, permissões/auditoria, sessões/isolamento, autenticação/cadastro/visitante, notificações e relatórios/CSV.

**Ainda não cobre todos os cenários do ecommerce.** Veja `COBERTURA.md` e `inventario-casos-de-uso.csv`: os CRUDs não exercitados, regras de checkout, integração financeira, importação, suporte e operações precisam de cenários adicionais. Regras ainda implementadas na infraestrutura não podem ser exercitadas aqui sem violar a restrição de referências; sua migração para Application/Domain continua necessária. A quantidade de exemplos não representa um percentual de conclusão.

## Adicionar cenários

1. Escreva uma regra observável: situação inicial, ação e consequência de negócio.
2. Use o handler/caso de uso real e mocks apenas para portas externas.
3. Em rejeições, verifique também ausência de gravação, emissão de sessão ou efeito externo.
4. Inclua caso positivo, limites, ausência de recurso, concorrência, isolamento e cancelamento quando aplicáveis.
5. Não use `Pending`, `Skip` ou asserts vazios para representar cobertura faltante.

Documentação do runner: https://docs.reqnroll.net/en/stable/integrations/xunit.html
