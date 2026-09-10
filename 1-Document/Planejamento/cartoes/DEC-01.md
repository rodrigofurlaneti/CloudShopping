# DEC-01 — Definir recebimento Asaas e modalidade de checkout

**Módulo:** 00-decisoes  
**Camada:** Definição  
**Etapa:** Fundação · **Prioridade:** P0  
**Estado:** Parcial — consulte evidências de entrega; aceite integral pendente.
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

A aplicação é multi-tenant; a conta que emite a cobrança determina credenciais, vínculo do cliente e recebedor.

**Evidência de origem:** 1-Document/Projeto_ Plataforma E-commerce Multi-Tenant (SaaS).md; docs.asaas.com/reference/criar-novo-checkout. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

Nenhuma dependência técnica anterior. Decisões devem ser registradas antes da implementação dependente.

## Escopo e detalhes

- Registrar se cada lojista recebe em conta própria ou se há subcontas/split; não presumir marketplace
- Adotar como proposta inicial cartão em página hospedada Asaas e Pix/boleto via cobrança; confirmar experiência e parcelamento
- Documentar ambiente sandbox, responsável pela conta e meios habilitados; decisão pendente de negócio, sem bloquear diagnóstico

## Critérios de aceite

- [ ] Documento identifica titular da cobrança, taxas/comissão e responsável por estornos
- [ ] Há exemplos de pedido de duas lojas e respectiva conta emissora
- [ ] Se split for escolhido, DB-23/BE-23/FE-23 tornam-se pré-requisitos de produção; sem decisão, não ativar pagamento real

## Entrega e verificação

Entregar decisão curta com alternativa escolhida, justificativa, responsável, data e impacto nos cartões; não marcar hipóteses como aprovadas. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [DB-10](DB-10.md), [DB-23](DB-23.md)




## Progresso da entrega 2

Status: **Parcial**. Consulte [implementação de pagamentos, evidências e pendências](../07-entrega-pagamentos-asaas.md). A escolha inclui recebimento direto e plataforma com split. Testes locais não substituem homologação Asaas nem encerram os critérios amplos deste cartão.
