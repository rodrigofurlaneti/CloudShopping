# O que falta para concluir o CloudShopping

Auditoria de 11/09/2026. Escopo: backend, frontend, banco, planejamento, testes e preparação operacional.

## Parecer

O projeto já possui uma base funcional de ecommerce e operação assistida. Ainda não está concluído para lançamento nem para o escopo completo de plataforma SaaS previsto no planejamento. A maior parte da jornada básica existe; o trabalho restante concentra-se em corrigir contratos de cadastro, fechar integrações e operação, completar módulos administrativos e tornar a entrega reproduzível e homologada.

Não é correto converter os 98 cartões históricos em percentual de conclusão. O documento anterior registra 72 parciais e 26 planejados, mas já houve mudanças posteriores de código, contratos e testes. Esses números são histórico de planejamento, não medição atual de aceite.

Esta auditoria inventariou 688 arquivos C# de produção, 50 TS/TSX e 71 C# de testes; percorreu rotas, módulos, contratos, serviços centrais, migrations e documentos de aceite. Não significa leitura linha a linha de todos os arquivos, pentest, ensaio de carga ou homologação em navegador. Graphify permaneceu indisponível; a análise foi direta sobre os arquivos, sem geração de grafo certificado. Nenhum código de negócio ou banco foi alterado nesta tarefa.

## Evidências executadas

| Verificação | Resultado atual |
|---|---|
| Build da solução .NET | Passou |
| Build TypeScript/Vite | Passou |
| Unitários | 183 aprovados, zero falhas/ignorados |
| BDD | 154 aprovados, zero falhas/ignorados |
| Arquitetura | 21 aprovados e 3 falhas |
| Cobertura de linhas Domain | 54,21%; meta existente de 90% não atingida |
| Cobertura de linhas Application | 8,77%; meta existente de 90% não atingida |
| ESLint | 10 erros em 8 arquivos |
| Integração técnica/Asaas | 97 testes aprovados na auditoria imediatamente anterior; não repetidos nesta tarefa, sem mudanças de código desde aquela execução |
| E2E em navegador | Não executado nesta auditoria; matriz ainda contém fluxos pendentes e dois requisitos ignorados por falta de tela |
| Asaas externo | Sem homologação Sandbox registrada |

Evidências novas em `.local/project-audit/`: `build.log`, `Units.trx`, `BehaviorDrivenDevelopment.trx`, `Architecture.trx` e `coverage.log`. Frontend: `.local/project-audit-frontend-build.log` e `.local/project-audit-lint.json`. A medição de cobertura gerou `3-BackEnd/tests/CloudShopping.Tests.Units/TestResults/e31673b7f1894f75b276beffe322ae3a/34d5d5ad-0d8d-46d1-b90e-52cb45545dd2/coverage.cobertura.xml`. Os diretórios de resultados são ignorados pelo Git.

## Prioridades para concluir

### 1. Corrigir perda de dados no cadastro — prioridade imediata

O detalhe administrativo de cliente não retorna nascimento nem inscrição estadual. A tela carrega nome/razão social, mas mantém os demais campos vazios e os envia como `null` ao salvar. Os métodos do domínio substituem os valores existentes. Editar somente o nome pode, portanto, apagar nascimento ou inscrição estadual; também é necessário impedir reaproveitamento de estado entre clientes.

Evidências: `GetCustomerByIdQuery.cs` e seu handler; `services/api.ts` (`CustomerDetail`); `pages/admin/Customers.tsx`, carregamento em torno da linha 166 e gravação em torno da linha 242; `Individual.Update` e `Company.Update`.

Concluir: contrato de detalhe completo, preenchimento correto do formulário, semântica explícita entre omitir e limpar campos e testes de edição/reabertura sem perda. Revisar também tipo e campos dos endereços administrativos. Cartões BE-06A/FE-06A.

### 2. Homologar pagamentos de ponta a ponta

Asaas Direct/PlatformSplit, Pix/boleto/cartão, inbox, conciliação e estorno integral já existem e receberam correções. Falta provar esses fluxos contra o provedor real: credenciais Sandbox, contas habilitadas, chave Pix, carteiras, comissão e URL HTTPS pública para webhook. Testar pagamento, cancelamento, estorno, duplicidade, perda de resposta, pagamento tardio e isolamento entre duas lojas.

Também faltam acompanhamento operacional da fila, recuperação de exceções financeiras e evolução para estorno parcial quando exigido pelo escopo. Não confundir confirmação de pagamento com saldo disponível para saque ou Escrow. Consulte [auditoria Asaas](14-auditoria-integracao-asaas.md), que contém contratos e checklist específicos.

### 3. Fechar fiscal, frete e expedição

Frete atual: tabela de opções com preço/prazo por prefixo de CEP. Expedição atual: operador informa transportadora, serviço e rastreio; atualizações são assistidas. Faltam adaptador de cotação contratado, etiquetas, rastreamento externo e tratamento de falhas de entrega conforme a operação definida.

Não há fluxo integrado de emissão/autorização de documento fiscal de mercadorias. O comando legado `MarkOrderAsInvoiced` não constitui emissão fiscal e as escritas legadas de pedido estão bloqueadas em `RequestGuards`. A operação atual precisa integrar o processo fiscal escolhido, persistir documento/status e disponibilizar consulta/rejeição/download. A definição fiscal deve ser validada com o responsável pela operação, sem presumir que toda modalidade de nota do Asaas atende ao negócio.

Evidências: `StorefrontRepository.Shipping`, `CommerceRecords.ShippingOption`, `OrderOperations.Dispatch/Track`, `RequestGuards`; ausência de módulos correspondentes nas migrations. Cartões 09, 13 e 14.

### 4. Completar acesso, funcionários e configuração de lojas

Existem login, visitante, sessão, troca de senha autenticada, revogação e permissões por perfil. Faltam recuperação de senha esquecida, verificação de email e uma jornada completa de gestão de funcionários/credenciais/vínculos. A tela de permissões vincula usuários existentes, mas não substitui o cadastro completo de funcionários.

O backend protege operações por permissão, e menus/rotas são filtrados. Os formulários de vários módulos ainda não adaptam suas ações à permissão de escrita: usuário de consulta pode encontrar botões que serão negados pelo servidor. Isso é pendência de UX, não evidência de bypass da autorização.

`/admin/register` mostra um aviso de provisionamento administrativo; `RegisterCompany.tsx` não está roteado. Faltam onboarding utilizável, configurações comerciais e identidade/conteúdo por loja, domínio e cadastro inicial consistente. Não basta expor a tela antiga: o fluxo de backend, permissões e criação da primeira conta precisa ser fechado e testado. Cartões 01–03 e 19.

### 5. Completar comunicação e pós-venda

O worker atual entrega notificações no portal, com outbox e retentativas. Não há adaptador de e-mail transacional. Faltam provedor, templates, preferências e confirmação de entrega/falha. Essa dependência também afeta recuperação e verificação de conta.

Devolução atual possui solicitação por item, decisão e inspeção com reposição de estoque. Faltam troca, logística reversa e vínculo completo com reembolso parcial. Atendimento possui protocolos, mas não implementa sozinho consentimentos, exportação e anonimização de dados. Faltam políticas e procedimentos de privacidade definidos para a operação. Cartões 15, 16 e 22.

### 6. Resolver as falhas de arquitetura e qualidade

As três falhas atuais são:

1. Controllers de Asaas, importação, atendimento e operação dependem diretamente da Infrastructure: separar casos de uso, contratos e adaptadores preservando idempotência, locks e transações.
2. O teste aponta 17 modelos persistidos de negócio fora do Domain, incluindo pagamentos, remessas, devoluções, suporte, avaliações, favoritos, importação e frete. Concluir encapsulamento e distribuição de responsabilidades.
3. A regra `Specs_reference_only_domain_and_application` tenta abrir `CloudShopping.Tests.Specs`, pasta que não existe mais. Corrigir o alvo para o projeto BDD atual e manter a política de dependências; não remover a regra para esconder falhas.

O diagnóstico histórico de cinco falhas já não é o resultado atual. O upload com contrato HTTP e parte de commands/validators avançaram; não devem ser repetidos automaticamente como achados ainda presentes.

Além disso, reduzir os 10 erros de lint, aumentar cobertura com cenários reais até a meta adotada e completar casos BDD. Os maiores vazios de cobertura estão na Application. `StoreCommerceService`, `AsaasPayments`, `OrderOperations` e serviços de engajamento/importação ainda concentram regras na infraestrutura. A presença de centenas de testes aprovados não encerra essas pendências.

### 7. Preparar operação e implantação

Não foram encontrados workflows de CI/CD em `.github/workflows` nem configuração versionada de implantação. Existem migrations com checksum e detecção de aplicação interrompida, mas isso não comprova migração do legado ou restauração de produção.

Concluir: ambiente de homologação, pipeline de build/testes/migração/deploy, domínios e TLS, configuração de proxy/hosts/CORS/cookies, segredos, usuário de banco adequado, persistência dos uploads e chaves Data Protection, backup/restauração ensaiados e procedimento de recuperação. Os uploads hoje ficam no filesystem da API.

Logs e rastreamento de processamento existem. Faltam evidências de alertas operacionais, acompanhamento de workers/filas, disponibilidade das dependências e testes de carga. `/health` responde um status fixo; não verifica prontidão de MySQL ou trabalhadores. RPO/RTO, retenção, responsáveis e infraestrutura continuam por definir. Revisar também os avisos de licença das dependências antes do lançamento.

### 8. Executar o aceite E2E real

A matriz de navegador ainda não cobre pagamentos, operação logística, perfis/permissões, importação, diversos cadastros e integrações externas. Cadastro de empresa e funcionário estão explicitamente ignorados por falta de interface. Preparar ambiente isolado, executar a jornada completa e registrar evidências em desktop, mobile e teclado. Corrigir também os caminhos antigos de projetos nas instruções de execução das suítes.

Aceite mínimo: cadastro/login → catálogo → endereço/frete → pedido → pagamento Sandbox → separação → emissão fiscal/processo definido → envio → entrega → devolução/estorno; testar também recusa, repetição, expiração, indisponibilidade e isolamento de loja.

## Saldo funcional de todos os grupos do planejamento

“Parcial” abaixo significa que há implementação, com critérios restantes. “Ausente” se refere ao fluxo integrado, não à inexistência absoluta de classes ou ideias relacionadas.

| Grupo | Situação observada | Falta para encerrar |
|---|---|---|
| Base e multiempresa | Parcial, isolamento e resolução de tenant presentes | Provisionamento, configuração operacional e migração do legado homologados |
| Sessão/login/visitante | Parcial | Recuperação/verificação de conta e E2E de segurança |
| Funcionários/perfis | Parcial | CRUD/jornada de funcionários, credenciais, vínculos e ações visuais por permissão |
| Produtos/imagens/variantes | Parcial | Fechar aceites de cadastro/edição, storage durável e testes E2E |
| Estoque | Reserva/baixa/reposição presentes | Homologar concorrência/carga, operação completa e dados legados |
| Clientes PF/PJ | Parcial, com defeito concreto | Corrigir preservação de campos e completar cadastro/contatos |
| Endereços | Parcial | Completar edição administrativa e consistência de tipos/campos |
| Carrinho | Funcional no recorte atual | Aceite de expiração/sessões, recuperação de falhas e jornada em navegador |
| Frete | Tabela por prefixo de CEP | Cotação externa e regras comerciais de entrega definidas |
| Checkout | Pedido/reserva/snapshot/idempotência presentes | Homologação ponta a ponta e regras finais de venda |
| Pagamentos | Integração implementada e testes simulados | Sandbox externo, exceções operacionais e estorno parcial previsto |
| Operação de pedidos | Fluxo assistido presente | Critérios completos, vínculo fiscal e decisão sobre Kanban/customizações |
| Expedição | Remessas e rastreio manuais presentes | Etiquetas, transportadora e conferência completa |
| Fiscal | Fluxo integrado ausente | Provedor/processo, persistência, emissão, autorização e rejeições |
| Devoluções/trocas | Devolução/inspeção presentes | Troca, logística reversa e reembolso parcial conciliado |
| Notificações | Portal com outbox presente | E-mail, templates, preferências e acompanhamento de entrega |
| Promoções | Cupons básicos presentes | Campanhas/segmentação e demais aceites comerciais |
| Categorias/busca/SEO | Departamentos e busca básica presentes | Hierarquia/facetas, metadados, sitemap e páginas indexáveis por loja/produto |
| Configuração da loja | Muito limitada | Identidade, dados comerciais, conteúdo/políticas e onboarding |
| Favoritos/avaliações | Presentes, com moderação | Denúncias, ciclo completo de autoria e E2E |
| Relatórios | Indicadores e CSV presentes | Taxas, líquido, comissões, estorno parcial e desempenho; hoje são projeções por pedidos |
| Suporte/privacidade | Protocolos presentes | Consentimentos, preferências, anexos e atendimento completo das solicitações de dados |
| Subcontas/repasse | Split com carteiras configuradas | Onboarding/KYC, comissão/contratos e painel de repasses; Escrow/saques não implementados |
| Planos SaaS | Ausente | Planos, cobrança recorrente, limites, inadimplência e administração da plataforma |
| Operação técnica | Logs/migrations/workers presentes | CI/CD, deploy, backups ensaiados, alertas, carga e recuperação |
| Homologação | Testes locais parciais | Aceite E2E, Sandbox externo e checklist operacional completo |
| B2B comercial | Cadastro PJ presente | Preço por cliente, orçamento, condições comerciais e conversão em pedido |
| Importação/ERP | CSV assistido presente | Provedor ERP, vínculo externo, fonte de verdade e sincronização |

Há um `OrdersKanban.tsx` antigo, mas a rota administrativa de pedidos usa `OrderOperationsPage`. Não contar a simples presença do arquivo como entrega do Kanban previsto. Da mesma forma, cadastro PJ não equivale a vendas B2B completas e produtos com variantes não equivalem a um marketplace com múltiplos vendedores por pedido.

## Ordem de execução sugerida

1. Corrigir os contratos de cliente e os defeitos conhecidos; atualizar a documentação de testes e o backlog com o estado real.
2. Fechar acesso/funcionários/onboarding, iniciar a separação dos módulos ainda fora da arquitetura e corrigir lint/testes estruturais.
3. Definir provedores de e-mail, frete e fiscal e implementar essas jornadas verticais.
4. Homologar pagamentos, entrega e pós-venda; finalizar as exceções financeiras.
5. Preparar infraestrutura e recuperação; completar E2E, cobertura, carga e critérios de lançamento.
6. Concluir o escopo de plataforma: planos SaaS, subcontas/repasse, B2B e ERP, além dos critérios restantes de conteúdo/SEO/promoções/privacidade.

Os itens do passo 6 pertencem ao escopo amplo registrado. Se o objetivo for lançar primeiro uma loja operada manualmente, a redução precisa ser uma decisão explícita de produto; não deve ser confundida com conclusão de todo o projeto.

## Decisões externas ainda necessárias

- Provedores de envio de e-mail, cotação/etiqueta, fiscal e ERP.
- Contas e credenciais Sandbox, responsáveis financeiros, carteiras e regras de comissão.
- Regras finais de venda, prazo de reserva, entrega, devolução/troca e condições B2B.
- Planos, preços e limites SaaS, quando incluídos no lançamento.
- Infraestrutura/domínios, responsáveis, backup/restauração e políticas operacionais.

Essas escolhas não impedem corrigir os defeitos de cadastro, arquitetura, lint, cobertura e testes já identificados.
