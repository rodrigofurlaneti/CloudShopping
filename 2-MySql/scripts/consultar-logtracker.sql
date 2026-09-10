USE ecommercedb;

-- Últimos erros de processamento.
SELECT Id, TenantId, TraceId, ClassName, MethodName, ExceptionType,
       ErrorMessage, ExecutionTimeMs, HttpStatusCode, CreatedAt
FROM logtracker
WHERE Outcome = 'Error'
ORDER BY CreatedAt DESC, Id DESC
LIMIT 100;

-- Cancelamentos separados de falhas internas.
SELECT Id, TenantId, TraceId, ClassName, MethodName, RequestAborted,
       ExecutionTimeMs, CreatedAt
FROM logtracker
WHERE Outcome = 'Cancelled'
ORDER BY CreatedAt DESC, Id DESC
LIMIT 100;
