# CloudShopping — plano para concluir o ecommerce

Preparado em 09/09/2026 com leitura estática do backend, frontend, documentação e estrutura do dump anexado. O plano contém **98 cartões: 29 de Banco de Dados, 36 de Backend, 30 de Frontend e 3 decisões de produto**, organizados em 29 grupos (decisões + 28 módulos).

## Entrega de implementação

Após a análise, foi implementado e testado o recorte **base, catálogo, carrinho e checkout**. Leia [a entrega 1 e suas evidências](06-entrega-base-catalogo-checkout.md). O diagnóstico original é histórico; os testes de runtime posteriores estão registrados nessa entrega.

A terceira entrega implementa operação assistida, devoluções, notificações no portal, cupons, favoritos/avaliações, suporte, indicadores, importação e ficha de variantes. Consulte [entrega 3](08-entrega-operacao-e-modulos.md) e [saldo para concluir o escopo](09-saldo-escopo-completo.md). O projeto ainda não atingiu 100% dos critérios mapeados.

## Comece aqui

A segunda entrega acrescenta os dois modelos de recebimento Asaas, pagamentos e conciliação. Consulte [entrega 2, configuração e homologação pendente](07-entrega-pagamentos-asaas.md). Os resultados da entrega 1 permanecem como registro histórico.

1. Leia o [diagnóstico](01-diagnostico.md) para distinguir código existente, defeitos e funcionalidades propostas.
2. Use o [roadmap e dependências](02-roadmap.md) para selecionar a próxima entrega.
3. Abra o [índice dos 98 cartões](03-backlog.md). Cada cartão é um arquivo separado, pronto para copiar para o gestor de tarefas.
4. Para pagamentos, leia o [desenho Asaas](04-asaas.md) e os [contratos e cenários de homologação](05-contratos-e-testes.md).
5. Para importação, use [backlog.json](backlog.json). As dependências foram verificadas: IDs existentes e grafo sem ciclos.

## O que foi analisado

- 427 arquivos C# do backend (Domain, Application, Infrastructure e API), além de projetos/configurações e especificação REST.
- 21 arquivos TS/TSX em src do frontend, além de CSS, configuração e documentação.
- As 27 tabelas do dump, com colunas, chaves e relacionamentos; SQL/DER/documento de modelagem do repositório.
- Bibliotecas geradas, bin/obj/node_modules, caches, lockfiles e conteúdo visual de imagens de produto não entram na análise semântica. Valores de segredos e dados pessoais dos INSERTs não foram reproduzidos; configurações sensíveis tiveram apenas estrutura considerada.

As listas de cobertura e evidências estão nas auditorias: [banco/domínio](auditoria-banco-dominio.md), [backend](auditoria-backend.md), [frontend](auditoria-frontend.md) e [inventário das tabelas](inventario-tabelas.md). **Na etapa de análise não houve restauração do dump, execução do sistema, testes de runtime nem transações Asaas.** As falhas indicadas são de análise estática; os cartões exigem confirmação em ambiente isolado.

A skill graphify orientou a separação e extração semântica. Seu pacote não estava instalado e a tentativa de instalação falhou por restrição de rede. Os três arquivos grafo-*.json são extrações parciais de relações explícitas; não são saída de pipeline Graphify/AST completo nem comprovam cobertura funcional. Não há benchmark ou custo de tokens medido a reportar. O grafo de dependências do backlog é uma proposta de execução, separado desses grafos de código/banco.

## Como usar os cartões

Cada funcionalidade é uma entrega vertical: **Banco → Backend → Frontend → homologação**. Isso não significa concluir todo o banco antes de qualquer tela. Uma tela pode ser preparada quando o contrato estiver estável, mas só fica pronta depois da integração com suas dependências.

- **Dependência obrigatória:** precisa ter entrega aceita antes de concluir o cartão dependente. Se a implementação pode começar contra um contrato, registrar explicitamente esse acordo.
- **Desbloqueia:** permite ao desenvolvedor enxergar quem precisa da entrega, sem procurar manualmente.
- **Estado:** 29 cartões têm recorte Parcial e 69 permanecem Planejado. A presença de código parcial não equivale a aceite cumprido.
- **Prioridade:** P0 corrige fundações/bloqueios; P1 compõe venda e operação; P2 é evolução ou escopo condicionado. Não equivale a severidade de segurança formal.
- **Estimativa:** não foi inventado prazo sem capacidade do time e decisões comerciais. Refinar com o desenvolvedor após os P0. Se um cartão ultrapassar a unidade de trabalho do time, dividir em subtarefas mantendo o ID pai e aceites, sem criar dependências circulares.

## Definição de pronto

Um cartão fica pronto quando seus critérios observáveis passam, a revisão técnica ocorre e a evidência fica anexada. Banco entrega migração/backfill/recuperação ensaiados. Backend entrega contrato e testes de domínio/integração pertinentes. Frontend entrega jornada integrada com estados de carregamento, vazio, erro e sucesso, autorização e uso móvel/teclado. Fluxos monetários incluem sandbox, idempotência e recuperação de falhas.

Todos os módulos privados preservam tenant e titular. Nenhuma tela deve confiar em preço/status financeiro escolhido pelo navegador. Documentos de modelagem e comentários são evidências de requisitos históricos; não autorizam executar SQL destrutivo, publicar ou alterar dinheiro.

## Decisões pendentes

[DEC-01](cartoes/DEC-01.md): conta própria por lojista versus subcontas/split e experiência de pagamento. A pergunta foi apresentada; não há resposta registrada neste plano. Proposta técnica: cartão hospedado e Pix/boleto por cobrança; não é uma escolha comercial aprovada.

[DEC-02](cartoes/DEC-02.md): reserva, expiração, pagamento tardio, entrega, parcelamento e regras de venda. Proposta: carrinho sem reserva e reserva temporária no checkout; TTL deve ser decidido por meio.

[DEC-03](cartoes/DEC-03.md): máquina de estados, fiscal, critérios operacionais e escopo de lançamento. Para mercadorias, definir provedor/processo fiscal adequado.

O escopo base é ecommerce brasileiro de produtos físicos, multiempresa, com cadastro PF/PJ, coerente com o projeto. Split, assinatura SaaS e condições comerciais B2B estão condicionados a decisão; não presumimos marketplace de múltiplos vendedores no mesmo pedido. Assinatura de produtos, programa de fidelidade, vale-presente e múltiplos centros de distribuição são extensões possíveis fora da baseline estimável aqui; exigem refinamento próprio se forem parte do negócio.

