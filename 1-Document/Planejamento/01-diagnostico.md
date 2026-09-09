# Diagnóstico consolidado

## Conclusão

Existe uma base relevante de cadastros e operações administrativas, mas o ecommerce ainda não está pronto para vender. A jornada pública é demonstrativa, há incompatibilidades entre persistência e schema, o checkout possui bloqueio no domínio e não há integração Asaas. A primeira entrega deve estabilizar a base; as seguintes completam compra, pagamento e operação.

Arquitetura observada: API .NET 9, camadas Domain/Application/Infrastructure, MediatR/FluentValidation, EF Core e consultas Dapper sobre MySQL; frontend React/TypeScript/Vite. O alvo do dump informa MySQL 8.4.9 no Azure; compatibilidade e configuração do servidor não foram testadas.

## O que reaproveitar

| Módulo | Evidência atual | Classificação |
|---|---|---|
| Clientes PF/PJ, endereços | Entidades, handlers e telas administrativas com HTTP | Parcial; corrigir persistência, posse e round-trip |
| Produtos/estoque/imagens | CRUD, consulta paginada, entrada/ajuste e upload | Parcial; catálogo comercial e concorrência pendentes |
| Departamentos/banners | API e telas; algumas áreas da home usam API | Parcial; escopo tenant/estado/publicação pendentes |
| Funcionários/usuários/perfis | Domínio e endpoints; senha tem hash | Parcial; sessão/RBAC e telas completas pendentes |
| Pedido/Kanban | Estados, comandos, consultas e painel com HTTP | Parcial; contratos, histórico e integrações pendentes |
| Carrinho/checkout | Agregados e handlers; preços vêm do servidor | Parcial e bloqueado; API/jornada/transações a concluir |
| Login/vitrine/detalhe/dashboard | Layouts prontos; autenticação e catálogo/métricas simulados | Demonstrativo |
| Pagamento | Estados e operações locais | Integração externa ausente |
| Frete/etiqueta/nota/rastreio | Métodos e status/logs | Não equivalem a integração operacional |
| Cupom, reviews, favoritos, relatórios reais | Sem implementação completa localizada | Módulos novos |

## Bloqueios com efeito no plano

| Achado | Consequência | Cartões |
|---|---|---|
| Address→CustomerAddresses, histórico→OrderStateHistories no EF, diferentes do dump | Leitura/gravação pode atingir tabela inexistente | [DB-01](cartoes/DB-01.md), [BE-01](cartoes/BE-01.md) |
| PK PF/PJ/endereço do pedido e PaymentStatus string vs int divergem | Falha de materialização/persistência | [DB-01](cartoes/DB-01.md), [BE-01](cartoes/BE-01.md) |
| Order.Checkout cria histórico com Id=0 e factory rejeita | Pedido novo falha antes do EF | [BE-00](cartoes/BE-00.md) |
| Login BE devolve texto fixo e login FE simula sucesso | Sessão real não é estabelecida | [BE-02](cartoes/BE-02.md), [FE-02](cartoes/FE-02.md) |
| Header/fallback tenant 1 e consultas sem escopo consistente | Isolamento não é garantido em todos os caminhos | [DB-03](cartoes/DB-03.md), [BE-03](cartoes/BE-03.md), [FE-03](cartoes/FE-03.md) |
| Version não controla concorrência e reserva não tem titular/prazo | Disputa de estoque e recuperação inseguras | [DB-05](cartoes/DB-05.md), [BE-05](cartoes/BE-05.md) |
| Evento de cancelamento publicado após commit; mutação do handler sem novo commit | Liberação pode ficar não persistida, efeitos podem se perder | [DB-00](cartoes/DB-00.md), [BE-00A](cartoes/BE-00A.md), [BE-05](cartoes/BE-05.md) |
| Aprovação local ignora origem/total quitado | Estado pago pode não representar recebimento válido | [BE-11C](cartoes/BE-11C.md), [FE-11A](cartoes/FE-11A.md) |
| paid/invoiced/delivery-failed passam sentinelas inválidas ao validator | Botões chamam caminhos que falham | [BE-12](cartoes/BE-12.md), [FE-12](cartoes/FE-12.md) |
| Onboarding em cinco commits | Falha pode deixar tenant incompleto | [BE-19](cartoes/BE-19.md) |
| Edição FE não hidrata todos os campos e força Shipping | Alterar nome/rua pode apagar ou trocar dados existentes | [BE-06A](cartoes/BE-06A.md), [FE-06A](cartoes/FE-06A.md) |
| Kanban lê só primeira página de 200 e usa selectedOrder antigo | Pedidos ausentes e ações desatualizadas | [FE-12](cartoes/FE-12.md) |

Os caminhos/linhas e limites de cada conclusão estão nas três auditorias. O schema possui **27 tabelas e 37 FKs** no inventário; dump e script inicial têm os mesmos nomes/listas de colunas, o que não prova equivalência total de defaults/índices/configuração. O documento histórico de modelagem também tem enum de pedido desatualizado (Shipped/Canceled 3/4 versus 9/16 no código/seed).

## Política de preservação

Reaproveitar agregados, handlers, telas e contratos que cumpram o comportamento necessário. Corrigir por migrações incrementais. Não recriar banco para acomodar EF sem tratar legado. Não deduzir dados históricos ausentes a partir de cadastro atual sem marcar sua origem. Separar status financeiro de etapa logística e de customização visual do Kanban.

## Confiança e validação pendente

Os achados acima decorrem de leitura de código e estrutura; não foi feito pentest nem execução para afirmar exploração real de cada rota. Isolamento, migrations e concorrência devem ser testados contra MySQL compatível, com dois tenants e clientes da mesma loja. Não foi calculado percentual de conclusão: quantidade de handlers ou telas não mede prontidão comercial.

