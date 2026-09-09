# FE-06A — Impedir perda de dados na edição de clientes

**Módulo:** 06-clientes  
**Camada:** Frontend  
**Etapa:** Fundação · **Prioridade:** P0  
**Estado:** Planejado — não iniciado  
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

Customers preenche poucos campos e pode reenviar nulos ou estado de outro cliente; endereço é forçado para Shipping.

**Evidência de origem:** 4-FrontEnd/src/pages/admin/Customers.tsx:170; 4-FrontEnd/src/pages/admin/Customers.tsx:304. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [BE-06A](BE-06A.md) — Corrigir DTOs e atualização parcial de cadastro
- [FE-01](FE-01.md) — Centralizar configuração e consumo da API

## Escopo e detalhes

- Hidratar/resetar formulário integralmente ao abrir/trocar cliente
- Enviar somente alterações ou DTO completo fiel ao contrato; preservar tipo de endereço
- Tratar falha de detalhe sem abrir formulário com estado anterior

## Critérios de aceite

- [ ] Editar nome não apaga nascimento/IE
- [ ] Trocar cliente não reutiliza campos do anterior
- [ ] Editar endereço de cobrança mantém Billing/bairro e cancelamento não persiste alterações

## Entrega e verificação

Entregar tela/fluxo integrado ao contrato aprovado, loading/vazio/erro/sucesso e evidência de uso em móvel/desktop. Incluir verificação de permissões e preservação de dados. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [FE-06](FE-06.md)


