# DEC-03 — Definir fluxo operacional, fiscal e limites da primeira versão

**Módulo:** 00-decisoes  
**Camada:** Definição  
**Etapa:** Fundação · **Prioridade:** P0  
**Estado:** Planejado — não iniciado  
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

Os status existentes representam etapas, mas não comprovam emissão fiscal, etiqueta ou rastreamento.

**Evidência de origem:** 3-BackEnd/src/CloudShopping.Domain/Entities/Orders/Order.cs; docs.asaas.com/docs/notas-fiscais. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

Nenhuma dependência técnica anterior. Decisões devem ser registradas antes da implementação dependente.

## Escopo e detalhes

- Definir estados financeiros separados de estados de atendimento e eventos que autorizam cada transição
- Definir integração fiscal de mercadorias com responsável contábil; Asaas NFS-e não deve ser assumida como NF-e de produtos
- Classificar módulos deste plano como lançamento, evolução ou condicionado; definir metas de carga, recuperação e acessibilidade

## Critérios de aceite

- [ ] Matriz de transições distingue autorização de cartão, confirmação, recebimento, estorno e chargeback
- [ ] Fluxo fiscal e logístico identifica provedor ou operação assistida verificável
- [ ] Há checklist de lançamento aprovado pelo responsável de produto antes da ativação comercial

## Entrega e verificação

Entregar decisão curta com alternativa escolhida, justificativa, responsável, data e impacto nos cartões; não marcar hipóteses como aprovadas. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [DB-12](DB-12.md), [DB-14](DB-14.md), [DB-25](DB-25.md)


