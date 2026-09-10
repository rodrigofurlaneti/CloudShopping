# Saldo do escopo completo — 98 cartões

O usuário autorizou todo o escopo. Este documento registra o que ainda precisa ser concluído, sem confundir implantação de um recorte com aceite integral. Nenhum cartão amplo foi marcado como concluído nesta entrega. Contar cartões parciais não mede percentual de projeto pronto.

Estado atual: **72 parciais e 26 planejados**. Critérios detalhados continuam nos cartões vinculados.

## Sequência restante de implementação

1. Fechar fundamentos pendentes: permissões granulares, recuperação/verificação de acesso, dados cadastrais/endereço, invariantes e migração/restauração do legado.
2. Completar operação: prova fiscal, etiqueta/transportadora, conferência de separação, devolução/troca e reembolso parcial; homologar os dois modos Asaas.
3. Completar comunicação, políticas/consentimentos, configuração de loja, SEO, promoções segmentadas, relatórios líquidos e critérios restantes dos módulos já iniciados.
4. Implementar planos/assinaturas SaaS, onboarding/KYC/repasse, preços e orçamentos B2B e adaptador ERP.
5. Homologar os critérios restantes, corrigir dívida de qualidade, medir carga/restauração e preparar pipeline/deploy com responsáveis e ambiente definido.

## Dependências externas e decisões

- Frete/etiquetas, fiscal e e-mail: **a definir**, conforme resposta do usuário.
- ERP: provedor e fonte de verdade não definidos.
- Asaas: conta sandbox/credenciais, webhook HTTPS público e homologação dos meios/contas ainda não executados.
- SaaS/B2B/marketplace: preços, vigência, limites, comissão, política de crédito e contratos operacionais reais não foram fornecidos; não foram inventados.
- Produção: infraestrutura, domínios, responsáveis, políticas fiscais/comerciais, retenção e RPO/RTO precisam ser fechados. Há trabalho interno pendente independentemente dessas decisões.

## Rastreabilidade de cada cartão

| Cartão | Estado | Evidência mais recente | Trabalho restante |
|---|---|---|---|
| [DEC-01 — Definir recebimento Asaas e modalidade de checkout](cartoes/DEC-01.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DEC-02 — Fechar regras de venda, estoque e entrega](cartoes/DEC-02.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DEC-03 — Definir fluxo operacional, fiscal e limites da primeira versão](cartoes/DEC-03.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-01 — Estabelecer baseline e migrações compatíveis com o dump](cartoes/DB-01.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-01 — Alinhar persistência, contratos de erro e configuração](cartoes/BE-01.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-01 — Centralizar configuração e consumo da API](cartoes/FE-01.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-02 — Persistir sessões e recuperação de acesso](cartoes/DB-02.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-02 — Implementar autenticação real do administrador e consumidor](cartoes/BE-02.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-02 — Conectar login, recuperação e área autenticada](cartoes/FE-02.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-03 — Reforçar integridade de tenant e permissões](cartoes/DB-03.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-03 — Resolver loja e autorizar cada operação](cartoes/BE-03.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-03 — Aplicar contexto de loja e telas de usuários/perfis](cartoes/FE-03.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-04 — Completar catálogo comercial e SKUs vendáveis](cartoes/DB-04.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Ficha comercial, slug, atributos e família de SKUs; ciclo amplo de imagens/filtros pendente. |
| [BE-04 — Publicar API de catálogo e completar gestão de produtos](cartoes/BE-04.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Ficha comercial, slug, atributos e família de SKUs; ciclo amplo de imagens/filtros pendente. |
| [FE-04 — Substituir vitrine simulada por catálogo real](cartoes/FE-04.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Ficha comercial, slug, atributos e família de SKUs; ciclo amplo de imagens/filtros pendente. |
| [DB-05 — Modelar reservas identificáveis e concorrência](cartoes/DB-05.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-05 — Implementar ciclo atômico de reserva e baixa](cartoes/BE-05.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-05 — Completar painel de estoque e histórico](cartoes/FE-05.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-06 — Completar cliente, endereços e vínculo externo](cartoes/DB-06.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [BE-06 — Concluir jornada do consumidor e posse dos dados](cartoes/BE-06.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-06 — Criar minha conta e cadastro no checkout](cartoes/FE-06.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-07 — Garantir carrinho ativo e linhas únicas](cartoes/DB-07.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-07 — Concluir API de carrinho e unificação de sessão](cartoes/BE-07.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-07 — Implementar comprar, minicarrinho e página de carrinho](cartoes/FE-07.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-08 — Persistir configuração e cotações de frete](cartoes/DB-08.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-08 — Implementar cálculo de frete e elegibilidade](cartoes/BE-08.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-08 — Criar seleção de entrega no carrinho e checkout](cartoes/FE-08.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-09 — Ampliar snapshot e idempotência do pedido](cartoes/DB-09.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-09 — Fechar pedido de forma transacional e recuperável](cartoes/BE-09.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-09 — Implementar revisão e confirmação de pedido](cartoes/FE-09.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-10 — Modelar conexão Asaas por loja e ambiente](cartoes/DB-10.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-10 — Criar adaptador Asaas e configuração segura](cartoes/BE-10.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-10 — Criar configuração de pagamentos do lojista](cartoes/FE-10.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-11 — Modelar tentativas, eventos e estornos](cartoes/DB-11.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-11 — Sincronizar pagador Asaas com vínculo local](cartoes/BE-11.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-11A — Criar cobranças Pix/boleto e checkout de cartão](cartoes/BE-11A.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-11B — Receber e persistir webhooks autenticados](cartoes/BE-11B.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-11C — Aplicar estados financeiros e efeitos uma única vez](cartoes/BE-11C.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-11D — Conciliar cobranças e recuperar falhas de integração](cartoes/BE-11D.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-11E — Executar cancelamento financeiro e reembolso](cartoes/BE-11E.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-11 — Apresentar pagamento e acompanhar resultado real](cartoes/FE-11.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-11A — Criar painel financeiro e retirar aprovações fictícias](cartoes/FE-11A.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-12 — Separar status canônico, etapas e auditoria de pedido](cartoes/DB-12.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Fluxo operacional auditado e paginado; equivalência legada/customizações e kanban por coluna pendentes. |
| [BE-12 — Corrigir contratos e máquina de estados operacional](cartoes/BE-12.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Fluxo operacional auditado e paginado; equivalência legada/customizações e kanban por coluna pendentes. |
| [FE-12 — Completar Kanban e área de meus pedidos](cartoes/FE-12.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Fluxo operacional auditado e paginado; equivalência legada/customizações e kanban por coluna pendentes. |
| [DB-13 — Persistir remessas, etiquetas e rastreamento](cartoes/DB-13.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Remessas parciais e rastreio assistido; adaptador, etiqueta e conferência ampliada pendentes. |
| [BE-13 — Integrar expedição e rastreio reais](cartoes/BE-13.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Remessas parciais e rastreio assistido; adaptador, etiqueta e conferência ampliada pendentes. |
| [FE-13 — Criar operação de separação e remessas](cartoes/FE-13.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Remessas parciais e rastreio assistido; adaptador, etiqueta e conferência ampliada pendentes. |
| [DB-14 — Modelar documentos fiscais de mercadorias](cartoes/DB-14.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [BE-14 — Implementar emissão fiscal e confirmação de autorização](cartoes/BE-14.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [FE-14 — Exibir documentos e tratar rejeições fiscais](cartoes/FE-14.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-15 — Modelar devolução e troca por item](cartoes/DB-15.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Solicitação por item e inspeção com reposição idempotente; troca/logística reversa/reembolso parcial pendentes. |
| [BE-15 — Implementar política de devolução e troca](cartoes/BE-15.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Solicitação por item e inspeção com reposição idempotente; troca/logística reversa/reembolso parcial pendentes. |
| [FE-15 — Criar autoatendimento e painel de devoluções](cartoes/FE-15.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Solicitação por item e inspeção com reposição idempotente; troca/logística reversa/reembolso parcial pendentes. |
| [DB-16 — Persistir eventos e entregas de mensagens](cartoes/DB-16.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Notificações reais no portal com outbox; e-mail, templates e preferências pendentes. |
| [BE-16 — Enviar mensagens transacionais com retry](cartoes/BE-16.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Notificações reais no portal com outbox; e-mail, templates e preferências pendentes. |
| [FE-16 — Criar preferências e histórico de comunicação](cartoes/FE-16.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Notificações reais no portal com outbox; e-mail, templates e preferências pendentes. |
| [DB-17 — Modelar cupons e regras promocionais](cartoes/DB-17.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Cupons com vigência, limites e snapshot no pedido; segmentação/campanhas pendentes. |
| [BE-17 — Aplicar promoções no cálculo autoritativo](cartoes/BE-17.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Cupons com vigência, limites e snapshot no pedido; segmentação/campanhas pendentes. |
| [FE-17 — Criar gestão de promoções e aplicação de cupom](cartoes/FE-17.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Cupons com vigência, limites e snapshot no pedido; segmentação/campanhas pendentes. |
| [DB-18 — Modelar categorias hierárquicas e metadados](cartoes/DB-18.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [BE-18 — Concluir busca facetada e indexação pública](cartoes/BE-18.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [FE-18 — Entregar navegação de categorias e páginas indexáveis](cartoes/FE-18.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-19 — Completar configuração e conteúdo por loja](cartoes/DB-19.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [BE-19 — Concluir onboarding atômico e configuração pública](cartoes/BE-19.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [FE-19 — Criar configuração inicial e identidade real da loja](cartoes/FE-19.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-20 — Modelar favoritos e avaliações](cartoes/DB-20.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Favoritos e avaliação verificada moderada; denúncia e ciclo completo de autoria pendentes. |
| [BE-20 — Implementar favoritos e moderação de avaliações](cartoes/BE-20.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Favoritos e avaliação verificada moderada; denúncia e ciclo completo de autoria pendentes. |
| [FE-20 — Criar favoritos e avaliações na loja](cartoes/FE-20.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Favoritos e avaliação verificada moderada; denúncia e ciclo completo de autoria pendentes. |
| [DB-21 — Preparar dados e índices para indicadores](cartoes/DB-21.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Indicadores por período e CSV reais; taxas/líquido/estornos parciais e desempenho pendentes. |
| [BE-21 — Publicar indicadores e exportações reais](cartoes/BE-21.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Indicadores por período e CSV reais; taxas/líquido/estornos parciais e desempenho pendentes. |
| [FE-21 — Substituir dashboard fictício e criar relatórios](cartoes/FE-21.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Indicadores por período e CSV reais; taxas/líquido/estornos parciais e desempenho pendentes. |
| [DB-22 — Persistir consentimentos e solicitações de atendimento](cartoes/DB-22.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Atendimento e protocolo de privacidade; consentimentos/exportação/anonimização/anexos pendentes. |
| [BE-22 — Implementar suporte, preferências e direitos do titular](cartoes/BE-22.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Atendimento e protocolo de privacidade; consentimentos/exportação/anonimização/anexos pendentes. |
| [FE-22 — Criar central de ajuda e preferências de privacidade](cartoes/FE-22.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Atendimento e protocolo de privacidade; consentimentos/exportação/anonimização/anexos pendentes. |
| [DB-23 — Modelar subcontas e comissões quando contratadas](cartoes/DB-23.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-23 — Implementar onboarding e split de pagamento](cartoes/BE-23.md) | Parcial | [07](07-entrega-pagamentos-asaas.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-23 — Criar painel de habilitação e repasses](cartoes/FE-23.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-24 — Modelar assinatura e limites da plataforma](cartoes/DB-24.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [BE-24 — Cobrar plano SaaS e aplicar limites](cartoes/BE-24.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [FE-24 — Criar plano, faturas e administração da plataforma](cartoes/FE-24.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-25 — Preparar recuperação, desempenho e deploy de schema](cartoes/DB-25.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [BE-25 — Criar pipeline, observabilidade e operação de workers](cartoes/BE-25.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [FE-25 — Concluir acessibilidade, responsividade e deploy web](cartoes/FE-25.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-26 — Homologar migração e invariantes da venda](cartoes/DB-26.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [BE-26 — Homologar jornada completa e Asaas sandbox](cartoes/BE-26.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [FE-26 — Validar experiência integrada e checklist de lançamento](cartoes/FE-26.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [DB-27 — Modelar preços por cliente e orçamento B2B](cartoes/DB-27.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [BE-27 — Implementar condições B2B e conversão de orçamento](cartoes/BE-27.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [FE-27 — Criar compra empresarial e orçamento](cartoes/FE-27.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-28 — Persistir importações e vínculos com ERP](cartoes/DB-28.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Importação CSV assistida com prévia e worker; ERP e vínculos externos pendentes. |
| [BE-28 — Importar catálogo e preparar adaptador ERP](cartoes/BE-28.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Importação CSV assistida com prévia e worker; ERP e vínculos externos pendentes. |
| [FE-28 — Criar importação assistida e relatório de erros](cartoes/FE-28.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Importação CSV assistida com prévia e worker; ERP e vínculos externos pendentes. |
| [BE-00 — Corrigir criação inicial do histórico de pedido](cartoes/BE-00.md) | Parcial | [06](06-entrega-base-catalogo-checkout.md) | Implementação parcial; concluir e homologar todos os critérios do cartão. |
| [BE-06A — Corrigir DTOs e atualização parcial de cadastro](cartoes/BE-06A.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [FE-06A — Impedir perda de dados na edição de clientes](cartoes/FE-06A.md) | Planejado | Sem entrega registrada | Implementar o escopo e executar todos os critérios de aceite. |
| [DB-00 — Persistir outbox transacional compartilhada](cartoes/DB-00.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Outbox transacional e consumidor idempotente do portal; e-mail e dispatcher externo pendentes. |
| [BE-00A — Implementar dispatcher de outbox e consumidores idempotentes](cartoes/BE-00A.md) | Parcial | [08](08-entrega-operacao-e-modulos.md) | Outbox transacional e consumidor idempotente do portal; e-mail e dispatcher externo pendentes. |
