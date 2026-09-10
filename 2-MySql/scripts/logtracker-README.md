# Diagnóstico de processamento

Execute `criar-logtracker.sql` na conexão correta de ecommercedb. O script cria somente a tabela e índices: não apaga tabela anterior, não carrega os registros de outro sistema presentes no dump e não altera usuários.

Se já existir uma tabela logtracker, compare o resultado de SHOW CREATE TABLE com a definição antes de usar as consultas: IF NOT EXISTS não migra colunas existentes.

`consultar-logtracker.sql` separa erros de cancelamentos. AppUserId deve ser interpretado junto com TenantId e ActorType; não representa uma FK para uma tabela única de usuários. CreatedAt deve ser informado em UTC pelo gravador.

## Camadas implementadas

`Domain/Entities/Diagnostics/LogTracker` possui fábrica, setters privados, validação e atualização. `ILogTrackerRepository` estende o contrato genérico CRUD e oferece paginação. A implementação EF `LogTrackerRepository` aplica a empresa atual a leituras e escritas; registros sem empresa ficam fora dessa interface administrativa. O mapeamento está em `LogTrackerConfiguration`, registrado automaticamente pelo DbContext, e o contrato está registrado na injeção de dependência.

Application contém CreateLogTracker, UpdateLogTracker, DeleteLogTracker, GetLogTrackerById e GetLogTrackers, cada um em sua pasta de command/query, com handler e validator. Atualização e exclusão recebem ExpectedUpdatedAt; o EF também trata UpdatedAt como token de concorrência. Exclusão é física, conforme contrato CRUD solicitado. Nenhum endpoint público de edição/exclusão foi criado.

Existe também `migrations/012_logtracker.sql` para bancos inicializados pelo migrador do projeto. Não foi aplicado ao banco remoto. Tabelas preexistentes com outro formato precisam de migração específica; IF NOT EXISTS não ajusta colunas antigas.

## Captura automática ainda necessária

O CRUD permite gravação explícita pelos casos de uso, mas não intercepta automaticamente exceções. O diagnóstico atual RequestCancellationMiddleware escreve pelo ILogger; ele ainda não persiste nesta tabela.

O gravador deverá ser implementado na infraestrutura através de contrato da aplicação, com integração no tratamento global da API e nos workers. Usar conexão/transação independente, limite de tempo próprio (sem reutilizar RequestAborted), falha de logging sem substituir a exceção original e fallback para ILogger quando o MySQL estiver indisponível. Não gravar senhas, tokens, chaves Asaas, connection strings, corpos HTTP ou mensagens de exceção sem sanitização. Classificar cancelamentos explicitamente e correlacionar por TraceId, evitando eventos duplicados de handler e middleware.

Os scripts foram preparados no repositório; não foram executados no banco remoto.


## Captura no TenantsController

O TenantsController agora utiliza ProcessingLogFilter (resource filter), que delega a ProcessingLogRecorder em Application e IProcessingLogWriter em Infrastructure. Registra resultado HTTP, duração, TraceId e tipo/nomes dos métodos da exceção. Não serializa parâmetros, respostas, mensagens brutas de exceção ou caminhos de arquivos.

A gravação usa conexão independente com AutoEnlist=false e timeout de dois segundos, sem salvar a unidade de trabalho de negócio. Falha de persistência gera aviso no ILogger e preserva o resultado original. A tabela precisa existir com o schema 012. Sem empresa resolvida o registro tem TenantId nulo e deve ser consultado por operação administrativa de plataforma/SQL, não pela listagem de uma loja.

A captura cobre somente requisições que alcançam o filtro de TenantsController. Bloqueios anteriores por autenticação, middleware ou resolução de empresa e outros controllers/workers não estão abrangidos. O status de uma exceção ainda não tratada é registrado como 500 (ou 499 para RequestAborted); um middleware externo pode depois transformar o status HTTP.

Build e testes unitários verificados. A gravação real no banco remoto ainda não foi executada nesta alteração.
