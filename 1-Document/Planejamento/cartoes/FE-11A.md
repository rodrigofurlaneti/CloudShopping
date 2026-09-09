# FE-11A — Criar painel financeiro e retirar aprovações fictícias

**Módulo:** 11-pagamentos  
**Camada:** Frontend  
**Etapa:** Venda · **Prioridade:** P1  
**Estado:** Parcial — recorte implementado em 09/09/2026; critérios amplos ainda pendentes  
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

Kanban permite aprovar/recusar/estornar localmente. O operador deve observar e solicitar operações reais.

**Evidência de origem:** 4-FrontEnd/src/pages/admin/OrdersKanban.tsx; 4-FrontEnd/src/services/api.ts. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [BE-11D](BE-11D.md) — Conciliar cobranças e recuperar falhas de integração
- [BE-11E](BE-11E.md) — Executar cancelamento financeiro e reembolso
- [FE-03](FE-03.md) — Aplicar contexto de loja e telas de usuários/perfis

## Escopo e detalhes

- Exibir cobrança/tentativas, estado financeiro, valor, recebimento, estornos e divergências
- Substituir aprovação manual Asaas por informação do provedor; solicitar estorno com valor/motivo e estado pendente
- Permitir reprocessamento/consulta conforme permissão com trilha e confirmação específica de operação monetária

## Critérios de aceite

- [ ] Botão não transforma cobrança Asaas em paga sem evento confiável
- [ ] Estorno parcial mostra saldo restante e aguarda confirmação
- [ ] Falhas e cobrança desconhecida ficam visíveis ao operador; lista tem paginação/filtros reais

## Entrega e verificação

Entregar tela/fluxo integrado ao contrato aprovado, loading/vazio/erro/sucesso e evidência de uso em móvel/desktop. Incluir verificação de permissões e preservação de dados. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [FE-23](FE-23.md), [FE-26](FE-26.md)



## Progresso da entrega 1

Consultar o [recorte implementado, testes e pendências](../06-entrega-base-catalogo-checkout.md). Esta entrega não encerra automaticamente os critérios acima.


## Progresso da entrega 2

Status: **Parcial**. Consulte [implementação de pagamentos, evidências e pendências](../07-entrega-pagamentos-asaas.md). A escolha inclui recebimento direto e plataforma com split. Testes locais não substituem homologação Asaas nem encerram os critérios amplos deste cartão.
