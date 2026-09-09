# BE-06A — Corrigir DTOs e atualização parcial de cadastro

**Módulo:** 06-clientes  
**Camada:** Backend  
**Etapa:** Fundação · **Prioridade:** P0  
**Estado:** Planejado — não iniciado  
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

DTOs de perfil/endereço não suportam edição sem perda de nascimento/IE/tipo/bairro.

**Evidência de origem:** CloudShopping.Application/Features/Customers/DTO; 4-FrontEnd/src/services/api.ts. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [BE-01](BE-01.md) — Alinhar persistência, contratos de erro e configuração

## Escopo e detalhes

- Retornar todos campos necessários ao round-trip e distinguir ausente de nulo em atualização
- Preservar tipo de endereço/bairro/complemento e campos não editados
- Documentar login por email ou username de forma coerente com onboarding

## Critérios de aceite

- [ ] Alterar só nome preserva nascimento e inscrição estadual
- [ ] Alterar rua de endereço Billing mantém tipo e demais campos
- [ ] Limpar campo explicitamente permitido é diferente de omitir campo

## Entrega e verificação

Entregar implementação/contrato OpenAPI atualizado, testes dos cenários de aceite e evidência de integração em ambiente isolado. Operações monetárias só em sandbox durante a homologação. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [BE-06](BE-06.md), [FE-06A](FE-06A.md)


