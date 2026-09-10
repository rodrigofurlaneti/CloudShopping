# Matriz funcional

Implementado significa código de teste criado; não significa aprovado em navegador. A execução local depende da configuração de homologação.

| Fluxo | Estado |
|---|---|
| Criar, editar, recarregar e excluir departamento próprio | Implementado |
| Criar departamento e produto, localizar por SKU após recarga | Implementado |
| Registrar cliente, persistir sessão, sair e confirmar logout | Implementado |
| Adicionar produto, aumentar/reduzir quantidade, recarregar e remover | Implementado |
| Salvar comprador/endereço, revisar, confirmar pedido, recarregar e cancelar | Implementado; exige produto e frete de teste |
| Abrir protocolo, responder e consultar após recarga | Implementado |
| Cadastrar entrega, consultar após recarga e desativar | Implementado |
| Bloquear visitante nas 19 rotas administrativas | Implementado; verificação de acesso, não CRUD desses módulos |
| Rota inexistente e formulário administrativo vazio | Implementado |
| Cadastrar nova empresa e acessar como administrador | Ignorado explicitamente: /admin/register é um aviso; RegisterCompany não está roteado |
| Criar/editar/desativar funcionário | Ignorado explicitamente: não existe tela/rota no frontend |
| Login inválido, permissões por perfil, isolamento entre empresas | Pendente |
| Perfis, usuários administrativos, credenciais e revogação de sessões | Pendente |
| Editar produto, imagens, estoque, localização e variantes | Pendente |
| CRUD de banners, estados/setores, clientes administrativos | Pendente |
| Cupons válidos/expirados/limites, concorrência e dupla confirmação | Pendente |
| Importação CSV, validação e processamento da fila | Pendente |
| Favoritos, avaliações e moderação, notificações | Pendente |
| Atendimento pelo operador e encerramento do protocolo | Pendente |
| Asaas Direct/PlatformSplit, PIX/boleto/cartão, conciliação, cancelamento e estorno | Pendente; exige Sandbox |
| Separação, expedição, rastreio, entrega, devolução e estoque devolvido | Pendente |
| Relatórios/exportação e integrações externas futuras | Pendente |

Não existem testes de sucesso fictício para requisitos indisponíveis. A suíte não substitui os testes unitários, BDD ou de arquitetura, e não corrige as pendências anteriores dessas suítes.
