# BE-11B — Receber e persistir webhooks autenticados

**Módulo:** 11-pagamentos  
**Camada:** Backend  
**Etapa:** Venda · **Prioridade:** P1  
**Estado:** Parcial — consulte evidências de entrega; aceite integral pendente.
**Responsável:** desenvolvedor da camada; revisão de domínio/integração pelo responsável técnico.

## Contexto

Asaas pode reenviar eventos; endpoint precisa confirmar recebimento somente após gravação durável.

**Evidência de origem:** docs.asaas.com/docs/polling-vs-webhooks-en; docs.asaas.com/docs/criar-novo-webhook-pela-aplicacao-web. Consulte as auditorias para caminhos e linhas completos. Referências a módulos novos são propostas, não código existente.

## Dependências obrigatórias

- [DB-11](DB-11.md) — Modelar tentativas, eventos e estornos
- [BE-10](BE-10.md) — Criar adaptador Asaas e configuração segura
- [BE-00A](BE-00A.md) — Implementar dispatcher de outbox e consumidores idempotentes

## Escopo e detalhes

- Criar endpoint exclusivo; validar asaas-access-token configurado, tamanho/payload e conta vinculada
- Persistir Inbox e devolver 2xx rápido após commit; deduplicar eventId por conta/ambiente
- Evento desconhecido deve ser registrado para inspeção; tolerar campos extras e processar fora do request

## Critérios de aceite

- [ ] Token inválido não grava evento de negócio
- [ ] Mesmo evento enviado 10 vezes resulta em um item processável; concorrência também deduplica
- [ ] Falha de banco não retorna sucesso; evento sobrevivente é processado após reiniciar serviço

## Entrega e verificação

Entregar implementação/contrato OpenAPI atualizado, testes dos cenários de aceite e evidência de integração em ambiente isolado. Operações monetárias só em sandbox durante a homologação. Aplicar a definição de pronto do [guia](../00-LEIA-ME.md).

**Desbloqueia:** [BE-11C](BE-11C.md), [BE-24](BE-24.md)




## Progresso da entrega 2

Status: **Parcial**. Consulte [implementação de pagamentos, evidências e pendências](../07-entrega-pagamentos-asaas.md). A escolha inclui recebimento direto e plataforma com split. Testes locais não substituem homologação Asaas nem encerram os critérios amplos deste cartão.
