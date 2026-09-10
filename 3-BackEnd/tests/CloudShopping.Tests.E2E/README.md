# CloudShopping.Tests.E2E

Projeto net9.0 com Selenium.WebDriver/Selenium.Support 4.40.0, xUnit 2.9.2, xunit.runner.visualstudio 2.8.2 e Microsoft.NET.Test.Sdk 17.14.1. Não referencia as camadas do backend nem abre conexão SQL: testa pelo navegador o frontend, API e persistência reais.

## Estado

Compilação e descoberta de testes verificadas. **Fluxos ainda não executados contra homologação:** URL e configuração do ambiente precisam ser confirmadas. A suíte não representa cobertura funcional completa. Dois requisitos estão explicitamente ignorados por falta de interface: cadastro de empresa e funcionário. Veja `CENARIOS.md`.

## Preparação local

1. Inicie uma instância dedicada do backend com `ConnectionStrings__DefaultConnection` apontando para o banco de homologação correto, com schema e dados de teste preparados. Não reutilize automaticamente a conexão de desenvolvimento. Este projeto não troca a connection string, não executa migrations e não apaga bancos.
2. Inicie o frontend com API/proxy direcionado exclusivamente a essa instância. No modo Vite de desenvolvimento, confira `VITE_DEV_TENANT_ID` para a empresa de teste. Reinicie o Vite após alterar variáveis. O Selenium usa a mesma resolução de empresa do navegador.
3. Prepare administrador dessa empresa, produto de teste com estoque disponível >= 3 e entrega ativa que atenda ao CEP 01001000. O checkout reserva estoque e depois cancela o próprio pedido; se falhar antes do cancelamento, pode deixar a reserva para inspeção/expiração.
4. Instale Chrome. Selenium Manager resolve o driver; sem rede, forneça `E2E_DRIVER_DIRECTORY` com chromedriver compatível com o Chrome instalado. Não há instalação de browser embutida nos testes.

Configure no terminal que vai executar os testes (senha via variável de ambiente local, nunca no repositório):

```powershell
$env:E2E_BASE_URL = 'http://localhost:5173' # exemplo; confirme a URL
$env:E2E_ENVIRONMENT = 'Homologacao'
$env:E2E_ADMIN_USERNAME = 'usuario-de-teste'
# Defina E2E_ADMIN_PASSWORD no ambiente local sem versionar a senha.
$env:E2E_PRODUCT_ID = '123' # substitua pelo produto de teste da empresa
$env:E2E_HEADLESS = 'true' # false para acompanhar o Chrome

dotnet test 3-BackEnd/tests/CloudShopping.Tests.E2E --filter 'Category=ReadOnly'
dotnet test 3-BackEnd/tests/CloudShopping.Tests.E2E --filter 'Category=Mutation' --logger trx
dotnet test 3-BackEnd/tests/CloudShopping.Tests.E2E --logger trx
```

`E2E_ENVIRONMENT` é uma declaração do operador, **não comprova** qual banco a API utiliza. Essa conferência deve ser feita na configuração efetiva do backend. Somente URLs loopback são aceitas nesta fase. Configuração ausente falha com mensagem explícita, em vez de aprovar ou ignorar silenciosamente testes.

No Visual Studio, o processo do runner precisa herdar as mesmas variáveis. Rodar toda a solução inclui esta suíte e exige ambiente E2E preparado.

## Isolamento e evidências

Cada teste abre um Chrome limpo e encerra seu próprio driver. Testes não dependem de ordem nem compartilham login. Execução é sequencial para reduzir interferência no banco. Há esperas explícitas, sem pausas fixas. Cadastros usam nomes E2E e emails únicos em example.test; nenhum email é enviado pelo próprio teste.

Departamento CRUD exclui somente o registro que criou; entrega criada é desativada. Clientes, protocolos e produtos criados permanecem no banco de homologação para inspeção. Use banco dedicado e política de reset externa à suíte; não há limpeza global ou exclusão de registros anteriores.

Opcional: `E2E_CAPTURE_FAILURES=true` salva screenshots de falhas em `bin/.../TestResults/screenshots`. Deixe desativado quando houver dados sensíveis. Screenshots e configurações locais são ignorados pelo Git; não são coletados cookies, senhas ou HTML.

Chamadas de pagamento Asaas não são disparadas por estes testes. Cobrir cobrança/conciliação/estorno requer um cenário próprio com conta Sandbox configurada e eventos verificáveis; não basta abrir a página de pagamento.
