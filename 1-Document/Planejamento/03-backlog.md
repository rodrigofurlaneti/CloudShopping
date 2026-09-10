# Backlog de conclusão do CloudShopping

98 cartões no escopo completo. Há implementação parcial relacionada a 72 cartões e 26 permanecem planejados; veja [entrega 1](06-entrega-base-catalogo-checkout.md) e [entrega 2](07-entrega-pagamentos-asaas.md) e [entrega 3](08-entrega-operacao-e-modulos.md). Veja o [saldo completo](09-saldo-escopo-completo.md). Cada título abre um cartão com contexto, escopo, dependências e critérios de aceite. A prioridade é da proposta, não uma medição do esforço. A etapa Venda é uma entrega integrada em homologação; produção exige os gates de Lançamento.

## 00-decisoes

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [DEC-01](cartoes/DEC-01.md) | Definição | Definir recebimento Asaas e modalidade de checkout | Fundação | — |
| [DEC-02](cartoes/DEC-02.md) | Definição | Fechar regras de venda, estoque e entrega | Fundação | — |
| [DEC-03](cartoes/DEC-03.md) | Definição | Definir fluxo operacional, fiscal e limites da primeira versão | Fundação | — |

## 01-base

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-00](cartoes/BE-00.md) | Backend | Corrigir criação inicial do histórico de pedido | Fundação | [BE-01](cartoes/BE-01.md) |
| [BE-00A](cartoes/BE-00A.md) | Backend | Implementar dispatcher de outbox e consumidores idempotentes | Fundação | [DB-00](cartoes/DB-00.md), [BE-01](cartoes/BE-01.md) |
| [BE-01](cartoes/BE-01.md) | Backend | Alinhar persistência, contratos de erro e configuração | Fundação | [DB-01](cartoes/DB-01.md) |
| [DB-00](cartoes/DB-00.md) | Banco de Dados | Persistir outbox transacional compartilhada | Fundação | [DB-01](cartoes/DB-01.md) |
| [DB-01](cartoes/DB-01.md) | Banco de Dados | Estabelecer baseline e migrações compatíveis com o dump | Fundação | — |
| [FE-01](cartoes/FE-01.md) | Frontend | Centralizar configuração e consumo da API | Fundação | [BE-01](cartoes/BE-01.md) |

## 02-identidade

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-02](cartoes/BE-02.md) | Backend | Implementar autenticação real do administrador e consumidor | Fundação | [DB-02](cartoes/DB-02.md), [BE-01](cartoes/BE-01.md) |
| [DB-02](cartoes/DB-02.md) | Banco de Dados | Persistir sessões e recuperação de acesso | Fundação | [DB-01](cartoes/DB-01.md) |
| [FE-02](cartoes/FE-02.md) | Frontend | Conectar login, recuperação e área autenticada | Fundação | [BE-02](cartoes/BE-02.md), [FE-01](cartoes/FE-01.md) |

## 03-tenant-rbac

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-03](cartoes/BE-03.md) | Backend | Resolver loja e autorizar cada operação | Fundação | [DB-03](cartoes/DB-03.md), [BE-02](cartoes/BE-02.md) |
| [DB-03](cartoes/DB-03.md) | Banco de Dados | Reforçar integridade de tenant e permissões | Fundação | [DB-01](cartoes/DB-01.md), [DB-02](cartoes/DB-02.md) |
| [FE-03](cartoes/FE-03.md) | Frontend | Aplicar contexto de loja e telas de usuários/perfis | Fundação | [BE-03](cartoes/BE-03.md), [FE-02](cartoes/FE-02.md) |

## 04-catalogo

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-04](cartoes/BE-04.md) | Backend | Publicar API de catálogo e completar gestão de produtos | Venda | [DB-04](cartoes/DB-04.md), [BE-03](cartoes/BE-03.md) |
| [DB-04](cartoes/DB-04.md) | Banco de Dados | Completar catálogo comercial e SKUs vendáveis | Venda | [DB-03](cartoes/DB-03.md) |
| [FE-04](cartoes/FE-04.md) | Frontend | Substituir vitrine simulada por catálogo real | Venda | [BE-04](cartoes/BE-04.md), [FE-03](cartoes/FE-03.md) |

## 05-estoque

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-05](cartoes/BE-05.md) | Backend | Implementar ciclo atômico de reserva e baixa | Venda | [DB-05](cartoes/DB-05.md), [BE-03](cartoes/BE-03.md) |
| [DB-05](cartoes/DB-05.md) | Banco de Dados | Modelar reservas identificáveis e concorrência | Venda | [DB-04](cartoes/DB-04.md), [DEC-02](cartoes/DEC-02.md) |
| [FE-05](cartoes/FE-05.md) | Frontend | Completar painel de estoque e histórico | Venda | [BE-05](cartoes/BE-05.md), [FE-03](cartoes/FE-03.md) |

## 06-clientes

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-06](cartoes/BE-06.md) | Backend | Concluir jornada do consumidor e posse dos dados | Venda | [DB-06](cartoes/DB-06.md), [BE-02](cartoes/BE-02.md), [BE-03](cartoes/BE-03.md), [BE-06A](cartoes/BE-06A.md) |
| [BE-06A](cartoes/BE-06A.md) | Backend | Corrigir DTOs e atualização parcial de cadastro | Fundação | [BE-01](cartoes/BE-01.md) |
| [DB-06](cartoes/DB-06.md) | Banco de Dados | Completar cliente, endereços e vínculo externo | Venda | [DB-03](cartoes/DB-03.md) |
| [FE-06](cartoes/FE-06.md) | Frontend | Criar minha conta e cadastro no checkout | Venda | [BE-06](cartoes/BE-06.md), [FE-02](cartoes/FE-02.md), [FE-06A](cartoes/FE-06A.md) |
| [FE-06A](cartoes/FE-06A.md) | Frontend | Impedir perda de dados na edição de clientes | Fundação | [BE-06A](cartoes/BE-06A.md), [FE-01](cartoes/FE-01.md) |

## 07-carrinho

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-07](cartoes/BE-07.md) | Backend | Concluir API de carrinho e unificação de sessão | Venda | [DB-07](cartoes/DB-07.md), [BE-06](cartoes/BE-06.md), [BE-04](cartoes/BE-04.md) |
| [DB-07](cartoes/DB-07.md) | Banco de Dados | Garantir carrinho ativo e linhas únicas | Venda | [DB-06](cartoes/DB-06.md), [DB-04](cartoes/DB-04.md), [DEC-02](cartoes/DEC-02.md) |
| [FE-07](cartoes/FE-07.md) | Frontend | Implementar comprar, minicarrinho e página de carrinho | Venda | [BE-07](cartoes/BE-07.md), [FE-04](cartoes/FE-04.md), [FE-06](cartoes/FE-06.md) |

## 08-frete

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-08](cartoes/BE-08.md) | Backend | Implementar cálculo de frete e elegibilidade | Venda | [DB-08](cartoes/DB-08.md), [BE-07](cartoes/BE-07.md) |
| [DB-08](cartoes/DB-08.md) | Banco de Dados | Persistir configuração e cotações de frete | Venda | [DB-04](cartoes/DB-04.md), [DB-06](cartoes/DB-06.md), [DEC-02](cartoes/DEC-02.md) |
| [FE-08](cartoes/FE-08.md) | Frontend | Criar seleção de entrega no carrinho e checkout | Venda | [BE-08](cartoes/BE-08.md), [FE-07](cartoes/FE-07.md) |

## 09-checkout

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-09](cartoes/BE-09.md) | Backend | Fechar pedido de forma transacional e recuperável | Venda | [DB-09](cartoes/DB-09.md), [BE-05](cartoes/BE-05.md), [BE-07](cartoes/BE-07.md), [BE-08](cartoes/BE-08.md), [BE-00](cartoes/BE-00.md), [BE-00A](cartoes/BE-00A.md) |
| [DB-09](cartoes/DB-09.md) | Banco de Dados | Ampliar snapshot e idempotência do pedido | Venda | [DB-05](cartoes/DB-05.md), [DB-07](cartoes/DB-07.md), [DB-08](cartoes/DB-08.md), [DB-00](cartoes/DB-00.md) |
| [FE-09](cartoes/FE-09.md) | Frontend | Implementar revisão e confirmação de pedido | Venda | [BE-09](cartoes/BE-09.md), [FE-08](cartoes/FE-08.md) |

## 10-asaas-config

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-10](cartoes/BE-10.md) | Backend | Criar adaptador Asaas e configuração segura | Venda | [DB-10](cartoes/DB-10.md), [BE-03](cartoes/BE-03.md) |
| [DB-10](cartoes/DB-10.md) | Banco de Dados | Modelar conexão Asaas por loja e ambiente | Venda | [DB-03](cartoes/DB-03.md), [DEC-01](cartoes/DEC-01.md) |
| [FE-10](cartoes/FE-10.md) | Frontend | Criar configuração de pagamentos do lojista | Venda | [BE-10](cartoes/BE-10.md), [FE-03](cartoes/FE-03.md) |

## 11-pagamentos

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-11](cartoes/BE-11.md) | Backend | Sincronizar pagador Asaas com vínculo local | Venda | [DB-11](cartoes/DB-11.md), [BE-06](cartoes/BE-06.md), [BE-10](cartoes/BE-10.md) |
| [BE-11A](cartoes/BE-11A.md) | Backend | Criar cobranças Pix/boleto e checkout de cartão | Venda | [BE-11](cartoes/BE-11.md), [BE-09](cartoes/BE-09.md) |
| [BE-11B](cartoes/BE-11B.md) | Backend | Receber e persistir webhooks autenticados | Venda | [DB-11](cartoes/DB-11.md), [BE-10](cartoes/BE-10.md), [BE-00A](cartoes/BE-00A.md) |
| [BE-11C](cartoes/BE-11C.md) | Backend | Aplicar estados financeiros e efeitos uma única vez | Venda | [BE-11B](cartoes/BE-11B.md), [BE-11A](cartoes/BE-11A.md), [BE-05](cartoes/BE-05.md), [BE-00A](cartoes/BE-00A.md) |
| [BE-11D](cartoes/BE-11D.md) | Backend | Conciliar cobranças e recuperar falhas de integração | Venda | [BE-11C](cartoes/BE-11C.md) |
| [BE-11E](cartoes/BE-11E.md) | Backend | Executar cancelamento financeiro e reembolso | Venda | [BE-11D](cartoes/BE-11D.md) |
| [DB-11](cartoes/DB-11.md) | Banco de Dados | Modelar tentativas, eventos e estornos | Venda | [DB-09](cartoes/DB-09.md), [DB-10](cartoes/DB-10.md), [DB-06](cartoes/DB-06.md), [DB-00](cartoes/DB-00.md) |
| [FE-11](cartoes/FE-11.md) | Frontend | Apresentar pagamento e acompanhar resultado real | Venda | [BE-11C](cartoes/BE-11C.md), [FE-09](cartoes/FE-09.md), [FE-10](cartoes/FE-10.md) |
| [FE-11A](cartoes/FE-11A.md) | Frontend | Criar painel financeiro e retirar aprovações fictícias | Venda | [BE-11D](cartoes/BE-11D.md), [BE-11E](cartoes/BE-11E.md), [FE-03](cartoes/FE-03.md) |

## 12-pedidos

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-12](cartoes/BE-12.md) | Backend | Corrigir contratos e máquina de estados operacional | Venda | [DB-12](cartoes/DB-12.md), [BE-11C](cartoes/BE-11C.md) |
| [DB-12](cartoes/DB-12.md) | Banco de Dados | Separar status canônico, etapas e auditoria de pedido | Venda | [DB-09](cartoes/DB-09.md), [DEC-03](cartoes/DEC-03.md) |
| [FE-12](cartoes/FE-12.md) | Frontend | Completar Kanban e área de meus pedidos | Venda | [BE-12](cartoes/BE-12.md), [FE-03](cartoes/FE-03.md), [FE-06](cartoes/FE-06.md) |

## 13-expedicao

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-13](cartoes/BE-13.md) | Backend | Integrar expedição e rastreio reais | Operação | [DB-13](cartoes/DB-13.md), [BE-12](cartoes/BE-12.md), [BE-08](cartoes/BE-08.md) |
| [DB-13](cartoes/DB-13.md) | Banco de Dados | Persistir remessas, etiquetas e rastreamento | Operação | [DB-08](cartoes/DB-08.md), [DB-12](cartoes/DB-12.md) |
| [FE-13](cartoes/FE-13.md) | Frontend | Criar operação de separação e remessas | Operação | [BE-13](cartoes/BE-13.md), [FE-12](cartoes/FE-12.md) |

## 14-fiscal

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-14](cartoes/BE-14.md) | Backend | Implementar emissão fiscal e confirmação de autorização | Operação | [DB-14](cartoes/DB-14.md), [BE-12](cartoes/BE-12.md) |
| [DB-14](cartoes/DB-14.md) | Banco de Dados | Modelar documentos fiscais de mercadorias | Operação | [DB-09](cartoes/DB-09.md), [DEC-03](cartoes/DEC-03.md) |
| [FE-14](cartoes/FE-14.md) | Frontend | Exibir documentos e tratar rejeições fiscais | Operação | [BE-14](cartoes/BE-14.md), [FE-12](cartoes/FE-12.md) |

## 15-pos-venda

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-15](cartoes/BE-15.md) | Backend | Implementar política de devolução e troca | Operação | [DB-15](cartoes/DB-15.md), [BE-11E](cartoes/BE-11E.md), [BE-13](cartoes/BE-13.md) |
| [DB-15](cartoes/DB-15.md) | Banco de Dados | Modelar devolução e troca por item | Operação | [DB-11](cartoes/DB-11.md), [DB-13](cartoes/DB-13.md) |
| [FE-15](cartoes/FE-15.md) | Frontend | Criar autoatendimento e painel de devoluções | Operação | [BE-15](cartoes/BE-15.md), [FE-12](cartoes/FE-12.md) |

## 16-notificacoes

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-16](cartoes/BE-16.md) | Backend | Enviar mensagens transacionais com retry | Venda | [DB-16](cartoes/DB-16.md), [BE-02](cartoes/BE-02.md), [BE-09](cartoes/BE-09.md), [BE-00A](cartoes/BE-00A.md) |
| [DB-16](cartoes/DB-16.md) | Banco de Dados | Persistir eventos e entregas de mensagens | Venda | [DB-09](cartoes/DB-09.md), [DB-00](cartoes/DB-00.md) |
| [FE-16](cartoes/FE-16.md) | Frontend | Criar preferências e histórico de comunicação | Operação | [BE-16](cartoes/BE-16.md), [FE-06](cartoes/FE-06.md), [FE-03](cartoes/FE-03.md) |

## 17-promocoes

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-17](cartoes/BE-17.md) | Backend | Aplicar promoções no cálculo autoritativo | Evolução | [DB-17](cartoes/DB-17.md), [BE-09](cartoes/BE-09.md), [BE-11E](cartoes/BE-11E.md) |
| [DB-17](cartoes/DB-17.md) | Banco de Dados | Modelar cupons e regras promocionais | Evolução | [DB-09](cartoes/DB-09.md) |
| [FE-17](cartoes/FE-17.md) | Frontend | Criar gestão de promoções e aplicação de cupom | Evolução | [BE-17](cartoes/BE-17.md), [FE-09](cartoes/FE-09.md) |

## 18-busca-seo

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-18](cartoes/BE-18.md) | Backend | Concluir busca facetada e indexação pública | Evolução | [DB-18](cartoes/DB-18.md), [BE-04](cartoes/BE-04.md) |
| [DB-18](cartoes/DB-18.md) | Banco de Dados | Modelar categorias hierárquicas e metadados | Evolução | [DB-04](cartoes/DB-04.md) |
| [FE-18](cartoes/FE-18.md) | Frontend | Entregar navegação de categorias e páginas indexáveis | Evolução | [BE-18](cartoes/BE-18.md), [FE-04](cartoes/FE-04.md) |

## 19-loja-conteudo

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-19](cartoes/BE-19.md) | Backend | Concluir onboarding atômico e configuração pública | Venda | [DB-19](cartoes/DB-19.md), [BE-03](cartoes/BE-03.md) |
| [DB-19](cartoes/DB-19.md) | Banco de Dados | Completar configuração e conteúdo por loja | Venda | [DB-03](cartoes/DB-03.md) |
| [FE-19](cartoes/FE-19.md) | Frontend | Criar configuração inicial e identidade real da loja | Venda | [BE-19](cartoes/BE-19.md), [FE-03](cartoes/FE-03.md) |

## 20-engajamento

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-20](cartoes/BE-20.md) | Backend | Implementar favoritos e moderação de avaliações | Evolução | [DB-20](cartoes/DB-20.md), [BE-06](cartoes/BE-06.md), [BE-04](cartoes/BE-04.md) |
| [DB-20](cartoes/DB-20.md) | Banco de Dados | Modelar favoritos e avaliações | Evolução | [DB-06](cartoes/DB-06.md), [DB-04](cartoes/DB-04.md), [DB-09](cartoes/DB-09.md) |
| [FE-20](cartoes/FE-20.md) | Frontend | Criar favoritos e avaliações na loja | Evolução | [BE-20](cartoes/BE-20.md), [FE-04](cartoes/FE-04.md), [FE-06](cartoes/FE-06.md) |

## 21-relatorios

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-21](cartoes/BE-21.md) | Backend | Publicar indicadores e exportações reais | Operação | [DB-21](cartoes/DB-21.md), [BE-11D](cartoes/BE-11D.md), [BE-12](cartoes/BE-12.md) |
| [DB-21](cartoes/DB-21.md) | Banco de Dados | Preparar dados e índices para indicadores | Operação | [DB-11](cartoes/DB-11.md), [DB-12](cartoes/DB-12.md) |
| [FE-21](cartoes/FE-21.md) | Frontend | Substituir dashboard fictício e criar relatórios | Operação | [BE-21](cartoes/BE-21.md), [FE-03](cartoes/FE-03.md) |

## 22-privacidade-suporte

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-22](cartoes/BE-22.md) | Backend | Implementar suporte, preferências e direitos do titular | Venda | [DB-22](cartoes/DB-22.md), [BE-06](cartoes/BE-06.md), [BE-19](cartoes/BE-19.md) |
| [DB-22](cartoes/DB-22.md) | Banco de Dados | Persistir consentimentos e solicitações de atendimento | Venda | [DB-06](cartoes/DB-06.md), [DB-19](cartoes/DB-19.md) |
| [FE-22](cartoes/FE-22.md) | Frontend | Criar central de ajuda e preferências de privacidade | Venda | [BE-22](cartoes/BE-22.md), [FE-06](cartoes/FE-06.md), [FE-19](cartoes/FE-19.md) |

## 23-subcontas-split

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-23](cartoes/BE-23.md) | Backend | Implementar onboarding e split de pagamento | Condicional | [DB-23](cartoes/DB-23.md), [BE-10](cartoes/BE-10.md), [BE-11A](cartoes/BE-11A.md) |
| [DB-23](cartoes/DB-23.md) | Banco de Dados | Modelar subcontas e comissões quando contratadas | Condicional | [DEC-01](cartoes/DEC-01.md), [DB-10](cartoes/DB-10.md), [DB-11](cartoes/DB-11.md) |
| [FE-23](cartoes/FE-23.md) | Frontend | Criar painel de habilitação e repasses | Condicional | [BE-23](cartoes/BE-23.md), [FE-10](cartoes/FE-10.md), [FE-11A](cartoes/FE-11A.md) |

## 24-saas-planos

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-24](cartoes/BE-24.md) | Backend | Cobrar plano SaaS e aplicar limites | Condicional | [DB-24](cartoes/DB-24.md), [BE-10](cartoes/BE-10.md), [BE-11B](cartoes/BE-11B.md), [BE-19](cartoes/BE-19.md) |
| [DB-24](cartoes/DB-24.md) | Banco de Dados | Modelar assinatura e limites da plataforma | Condicional | [DB-03](cartoes/DB-03.md), [DB-10](cartoes/DB-10.md) |
| [FE-24](cartoes/FE-24.md) | Frontend | Criar plano, faturas e administração da plataforma | Condicional | [BE-24](cartoes/BE-24.md), [FE-19](cartoes/FE-19.md) |

## 25-producao

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-25](cartoes/BE-25.md) | Backend | Criar pipeline, observabilidade e operação de workers | Fundação | [BE-01](cartoes/BE-01.md), [BE-03](cartoes/BE-03.md), [DB-25](cartoes/DB-25.md) |
| [DB-25](cartoes/DB-25.md) | Banco de Dados | Preparar recuperação, desempenho e deploy de schema | Fundação | [DB-01](cartoes/DB-01.md), [DEC-03](cartoes/DEC-03.md) |
| [FE-25](cartoes/FE-25.md) | Frontend | Concluir acessibilidade, responsividade e deploy web | Fundação | [FE-01](cartoes/FE-01.md), [FE-02](cartoes/FE-02.md) |

## 26-homologacao

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-26](cartoes/BE-26.md) | Backend | Homologar jornada completa e Asaas sandbox | Lançamento | [BE-25](cartoes/BE-25.md), [DB-26](cartoes/DB-26.md), [BE-11D](cartoes/BE-11D.md), [BE-11E](cartoes/BE-11E.md), [BE-12](cartoes/BE-12.md), [BE-13](cartoes/BE-13.md), [BE-14](cartoes/BE-14.md), [BE-15](cartoes/BE-15.md), [BE-16](cartoes/BE-16.md), [BE-19](cartoes/BE-19.md), [BE-22](cartoes/BE-22.md), [BE-21](cartoes/BE-21.md) |
| [DB-26](cartoes/DB-26.md) | Banco de Dados | Homologar migração e invariantes da venda | Lançamento | [DB-25](cartoes/DB-25.md), [DB-11](cartoes/DB-11.md), [DB-12](cartoes/DB-12.md), [DB-13](cartoes/DB-13.md), [DB-14](cartoes/DB-14.md), [DB-15](cartoes/DB-15.md), [DB-22](cartoes/DB-22.md), [DB-21](cartoes/DB-21.md) |
| [FE-26](cartoes/FE-26.md) | Frontend | Validar experiência integrada e checklist de lançamento | Lançamento | [BE-26](cartoes/BE-26.md), [FE-25](cartoes/FE-25.md), [FE-11](cartoes/FE-11.md), [FE-11A](cartoes/FE-11A.md), [FE-12](cartoes/FE-12.md), [FE-13](cartoes/FE-13.md), [FE-14](cartoes/FE-14.md), [FE-15](cartoes/FE-15.md), [FE-19](cartoes/FE-19.md), [FE-22](cartoes/FE-22.md), [FE-05](cartoes/FE-05.md), [FE-16](cartoes/FE-16.md), [FE-21](cartoes/FE-21.md) |

## 27-b2b-comercial

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-27](cartoes/BE-27.md) | Backend | Implementar condições B2B e conversão de orçamento | Condicional | [DB-27](cartoes/DB-27.md), [BE-09](cartoes/BE-09.md), [BE-11A](cartoes/BE-11A.md) |
| [DB-27](cartoes/DB-27.md) | Banco de Dados | Modelar preços por cliente e orçamento B2B | Condicional | [DB-06](cartoes/DB-06.md), [DB-09](cartoes/DB-09.md) |
| [FE-27](cartoes/FE-27.md) | Frontend | Criar compra empresarial e orçamento | Condicional | [BE-27](cartoes/BE-27.md), [FE-09](cartoes/FE-09.md), [FE-06](cartoes/FE-06.md) |

## 28-integracoes-catalogo

| ID | Camada | Cartão | Etapa | Depende de |
|---|---|---|---|---|
| [BE-28](cartoes/BE-28.md) | Backend | Importar catálogo e preparar adaptador ERP | Evolução | [DB-28](cartoes/DB-28.md), [BE-04](cartoes/BE-04.md), [BE-05](cartoes/BE-05.md), [BE-25](cartoes/BE-25.md) |
| [DB-28](cartoes/DB-28.md) | Banco de Dados | Persistir importações e vínculos com ERP | Evolução | [DB-04](cartoes/DB-04.md), [DB-05](cartoes/DB-05.md) |
| [FE-28](cartoes/FE-28.md) | Frontend | Criar importação assistida e relatório de erros | Evolução | [BE-28](cartoes/BE-28.md), [FE-04](cartoes/FE-04.md) |

## Importação

[backlog.json](backlog.json) contém ID, título, módulo, camada, contexto, detalhes, critérios, prioridade, dependências e cartões desbloqueados. Não é formato específico de Jira/Trello: mapear campos ao importar. Criar todos os cartões primeiro e ligar dependências pelo ID depois; preservar IDs ao dividir atividades.

