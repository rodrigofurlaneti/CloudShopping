# BE-11A — Criar cobranças Pix/boleto e checkout de cartão

**Módulo:** 11-pagamentos  
**Camada:** Backend  
**Etapa:** Venda · **Prioridade:** P1  
**Estado:** Parcial — consulte evidências de entrega; aceite integral pendente.
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

O fluxo existente apenas registra pagamentos locais; a proposta usa APIs Asaas com correlação persistida.

**Evidência de origem:** docs.asaas.com/reference/criar-nova-cobranca; docs.asaas.com/reference/criar-novo-checkout. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [BE-11](BE-11.md) — Sincronizar pagador Asaas com vínculo local
- [BE-09](BE-09.md) — Fechar pedido de forma transacional e recuperável

## Escopo e detalhes

- Criar intenção local antes da chamada; Pix/boleto por cobrança e cartão por checkout hospedado conforme DEC-01
- Persistir identificadores, links/QR/linha digitável quando retornados e valor autorizado pelo servidor; não coletar PAN/CVV na proposta hospedada
- Tratar timeout ambíguo como estado a conciliar; externalReference é correlação, não garantia automática de idempotência do provedor

## Critérios de aceite

- [ ] Pedido com mesmo intento não recebe duas cobranças em retry; resultado incerto bloqueia duplicação até conciliação
- [ ] Meio/parcelas/valor fora da configuração são recusados
- [ ] Resposta de criação e callback de navegador não aprovam pedido; links são do ambiente correto

## Entrega e verificação

Entregar implementação/contrato OpenAPI atualizado, testes dos cenários de aceite e evidência de integração em ambiente isolado. Operações monetárias só em sandbox durante a homologação. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [BE-11C](BE-11C.md), [BE-23](BE-23.md), [BE-27](BE-27.md)




## Progresso da entrega 2

Status: **Parcial**. Consulte [implementação de pagamentos, evidências e pendências](../07-entrega-pagamentos-asaas.md). A escolha inclui recebimento direto e plataforma com split. Testes locais não substituem homologação Asaas nem encerram os critérios amplos deste cartão.
