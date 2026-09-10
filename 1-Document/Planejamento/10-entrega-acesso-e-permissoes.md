# Entrega 4 — permissões por perfil e segurança da conta

## Implementado

- Migração `011_profile_permissions.sql`: permissões por perfil/loja e histórico imutável das alterações de permissões, com autor e valores anteriores/novos. Aplicada somente ao MySQL local isolado e aos bancos descartáveis de teste.
- O login administrativo aceita perfis ativos que possuam permissões. O papel técnico `Administrator` identifica acesso ao backoffice; acesso integral depende do perfil ativo `Administrador Geral`, representado por `*` na sessão.
- A API consulta as permissões efetivas a cada requisição. Catálogo, pedidos, clientes, financeiro, configurações, atendimento, avaliações, promoções e relatórios têm consulta/alteração separadas. Estoque e estorno possuem permissões próprias. Importação/criação de produto exigem estoque; decisão de devolução exige pedidos e estoque. Configuração Asaas usa configuração, não permissão de operação financeira.
- Controllers administrativos não mapeados ficam restritos ao administrador geral. Cadastro de usuários, perfis e vínculos permanece exclusivo dele. As rotas públicas e do consumidor mantêm suas regras de autenticação e posse.
- A autorização roda antes da resposta automática de validação de formulário. Alterações de permissões usam bloqueio por perfil, comparação com o conjunto esperado e transação com auditoria. Duas edições divergentes não sobrescrevem silenciosamente uma à outra.
- `/admin/access`: criação de perfil, edição de permissões e vínculo a usuário existente. O administrador geral não tem suas permissões integrais editadas pelo formulário. Menus e rotas administrativas consideram permissões da sessão; a API continua verificando o estado atual mesmo quando a tela está desatualizada.
- `/admin/security` e Minha conta: consulta de sessões ativas, encerramento individual de outras sessões ou de todas as outras; alteração de senha mediante prova da senha atual. Nova senha requer 12 caracteres e no máximo 72 bytes UTF-8. A troca grava senha e revogação de todos os acessos na mesma transação e exige novo login.
- Sessões são identificadas por conta, modalidade e loja. Visitantes não podem gerenciar credenciais. A listagem não expõe hashes de senha ou carimbos de credencial; mostra até 100 sessões por vencimento.
- Bloqueadas alterações nas rotas legadas de histórico de pedido, além das mutações legadas de pedido já bloqueadas. Notas novas continuam no fluxo operacional auditado.

## Validação

- Backend compilado; frontend TypeScript/Vite compilado; ESLint dos arquivos novos e alterados nesta etapa aprovado.
- Suíte completa: **53 testes aprovados**, sendo 46 cenários com MySQL e 7 testes sem MySQL. Os oito novos casos abrangem senha/sessões, escopo de conta/modalidade/loja, atualização de permissões/auditoria/conflito e matriz de operações sensíveis.
- TRX: `.local/test-results/AMD_ISS_2026-09-09_22_35_39_net9.0.trx`.
- `access_http_smoke.py` aprovado: funcionário com consulta não ajusta estoque, não estorna, não altera conexão Asaas, não administra usuários/perfis e não atravessa lojas; retirar uma permissão bloqueia a próxima requisição; retirar todas invalida o acesso. Revogar outra sessão e trocar senha invalida os cookies anteriores; novo login funciona.
- `http_smoke.py` anterior aprovado, incluindo checkout, replay, posse, pagamentos, operação e módulos internos.
- Navegador: conferidas sessões, formulário de senha, seleção de perfil e controles de permissão. O layout foi corrigido após inspeção visual. Testes de alteração de senha e concessão de permissões foram executados pela API somente com contas sintéticas locais, não pela UI de uma conta real.

Reprodução: aplicar as migrações com `CloudShopping.Db` no banco isolado; rodar `dotnet test 3-BackEnd/tests/CloudShopping.Tests --no-restore`; build/lint no frontend. Os dois scripts HTTP exigem `CLOUDSHOPPING_DEMO_PASSWORD` e validam a identificação da loja sintética local antes dos testes. O script de acesso cria perfis e usuários sintéticos.

## Pendências explícitas

Esta etapa **não conclui 100% do projeto**, nem o aceite integral dos cartões DB/BE/FE-02 e DB/BE/FE-03.

- Recuperação de senha sem sessão e confirmação de e-mail: dependem da implementação do fluxo de token de uso único e da escolha/configuração do provedor. Não há e-mail enviado nem recuperação fictícia disponível.
- Funcionários: completar cadastro/edição/desativação e gestão visual de vínculos, histórico de concessões/revogações de vínculo, proteção do último administrador e substituição do perfil especial baseado em nome por identidade explícita. As APIs legadas continuam restritas ao administrador geral; a nova auditoria cobre mudanças do conjunto de permissões, não todo o CRUD legado.
- Aprimorar cada tela antiga para ocultar/desabilitar individualmente ações sem permissão. Menus e acesso às páginas são filtrados; uma ação ainda visível que exija permissão ausente recebe 403 no servidor.
- Completar paginação de perfis/usuários e histórico visual, identificação amigável de dispositivos, auditoria/retencão de sessões e homologação ampliada da jornada administrativa.
- Permanecem os demais itens de `09-saldo-escopo-completo.md`, a dívida de lint global e os avisos de licença anteriores. Não houve deploy ou homologação externa Asaas, frete, fiscal ou e-mail.
