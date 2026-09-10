# Cobertura inicial e trabalho restante

Última execução: 10/09/2026. **154 cenários passaram; nenhum ignorado ou pendente no runner.**

Cobertura de linhas medida pelo coverlet, exclusivamente nesta suíte:

| Camada | Linhas cobertas |
|---|---:|
| Domain | 38.58% |
| Application | 5.01% |

Percentuais de linhas não medem todos os cenários de negócio, combinações, concorrência real ou integrações. Estes números demonstram que **a cobertura não está completa**.

## Cenários implementados

| Área | Consequências verificadas |
|---|---|
| Estoque | reserva disponível, baixa e liberação acima da reserva, entrada e ajuste inválidos |
| Carrinho | somar, definir, remover, limpar, item ausente e quantidade inválida |
| Pipeline | rejeição antes de chamar o serviço, limites 1/999, titular e operação inválidos |
| Catálogo | versão antiga, ausência, slug duplicado/inválido, atributos excedidos e ausência de commit na rejeição |
| Cupons | valor, precisão monetária, tipo, vigência, código, limites, contador, cadastro na loja atual, duplicidade e ativação concorrente |
| Clientes | transições B2C/B2B e dígitos verificadores de documentos |
| Pedidos | pagamento repetido, pagamento após liberação, estorno, bloqueio de etapas e limites de desconto |
| Acesso | dependências de permissões, administrador geral, concorrência, autorização e gravação de auditoria |
| Sessões | expiração, revogação, identidade, credencial alterada, empresa inativa e isolamento |
| Autenticação | login administrativo/cliente, cadastro, visitante, email duplicado, senha e papéis incompatíveis |
| Notificações | propriedade e preservação da data da primeira leitura |
| Relatórios | limite de 5.000 linhas, período, exportação vazia e proteção de fórmulas CSV |

## Inventário e complementação

`inventario-casos-de-uso.csv` lista **140 handlers**. A indicação de referência em binding é somente rastreabilidade textual, não declaração de cobertura completa. SessionUseCases e SessionLifecycle têm cenários próprios e não aparecem como handlers diretos nesse CSV.

Faltam cenários diretos para vários CRUDs originais (tenants, departamentos, funcionários, usuários, perfis, banners, endereços, imagens, localização, estados e setores), fluxo logístico completo, troca de senha e revogação com transações, unificação de carrinho, regras complementares de produto/cliente e limites negativos de baixa/liberação de estoque. Esses itens devem receber exemplos e asserts de efeitos observáveis.

Checkout transacional, conciliação Asaas, frete, importações, suporte, moderação, expedição, devoluções e entrega de notificações ainda contêm lógica na infraestrutura. Parte já tem testes em CloudShopping.Tests, mas **esses testes não foram copiados para Specs**, pois isso violaria a referência exclusiva a Domain/Application. Depois da separação de camadas, portar os cenários para as portas correspondentes.

A suíte de arquitetura permanece vermelha nas violações reais; não há baseline permissiva ou skips. Nenhuma mudança de regra de negócio foi aplicada para acomodar os testes.
