# CloudShopping.Tests.Arch

Projeto de testes de arquitetura em **net9.0**, com xUnit, FluentAssertions e NetArchTest.Rules. Referencia Domain, Application, Infrastructure e API para inspecionar as dependências reais compiladas. A inspeção do modelo EF não abre conexão de banco nem executa workers.

```powershell
dotnet test 3-BackEnd/tests/CloudShopping.Tests.Arch
```

Se a API estiver em execução no Visual Studio, pare antes de recompilar as DLLs. Alternativamente, use uma saída separada:

```powershell
dotnet test 3-BackEnd/tests/CloudShopping.Tests.Arch -p:BaseOutputPath=C:/Users/AMD/Documents/Codex/CloudShopping/.local/arch-build/
```

## Políticas executáveis

- Domain sem dependência de Application, Infrastructure, API, ASP.NET ou bibliotecas de banco.
- Application sem implementações externas, EF, Dapper, MySQL ou contratos HTTP.
- Infrastructure sem dependência de API.
- Controllers sem dependência de Infrastructure.
- Entidades com criação controlada, setters encapsulados e coleções protegidas.
- Modelos EF declarados no domínio.
- Contratos de repositório com implementação na infraestrutura.
- Requests com handler único; commands/queries em namespaces próprios; commands com validator.
- Query handlers sem dependência de IUnitOfWork.
- Specs com referências exclusivas a Domain/Application.

## Resultado inicial: 19 aprovados, 5 falhas em 24 verificações

As falhas são violações detectadas no projeto existente, não testes ignorados:

1. Controllers de Asaas, importação, atendimento e operação dependem da infraestrutura.
2. Application expõe IFormFile/contratos HTTP no upload de imagem.
3. Há modelos EF em Infrastructure (pagamentos, frete, importação, expedição, devolução, suporte, avaliações e favoritos).
4. Há commands fora do namespace por operação exigido pelo padrão adotado.
5. Há commands sem validator.

Execute novamente para obter a lista atualizada de tipos. Não foi criada uma lista de exceções para tornar esses testes verdes. Consequentemente, `dotnet test` da solução continuará falhando enquanto essas violações não forem corrigidas.

Esses testes verificam políticas estruturais; não provam, sozinhos, a qualidade completa do desenho DDD, limites de agregados ou adequação de regras de negócio. Novas políticas devem ser explicitadas e adicionadas quando o padrão do projeto evoluir.
