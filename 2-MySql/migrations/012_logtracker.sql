-- Diagnostic records; no FK to another application user table.
CREATE TABLE IF NOT EXISTS logtracker (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    TenantId INT NULL,
    AppUserId BIGINT NULL,
    -- Customer / Administrator / System: IDs de cliente e funcionário podem coincidir.
    ActorType VARCHAR(30) NULL,
    TraceId VARCHAR(128) NULL,
    DirectoryName VARCHAR(150) NULL,
    ClassName VARCHAR(150) NOT NULL,
    MethodName VARCHAR(150) NOT NULL,
    -- Success / Error / Cancelled; cancelamento não é necessariamente falha interna.
    Outcome VARCHAR(20) NOT NULL DEFAULT 'Error',
    IsSuccess TINYINT(1) NOT NULL DEFAULT 0,
    RequestAborted TINYINT(1) NOT NULL DEFAULT 0,
    ExecutionTimeMs BIGINT NULL,
    HttpMethod VARCHAR(10) NULL,
    HttpStatusCode INT NULL,
    ExceptionType VARCHAR(255) NULL,
    Message TEXT NULL,
    ErrorMessage TEXT NULL,
    StackTrace TEXT NULL,
    IpAddress VARCHAR(45) NULL,
    -- O gravador deve fornecer CreatedAt em UTC explicitamente.
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (Id),
    KEY ix_logtracker_tenant_date (TenantId, CreatedAt),
    KEY ix_logtracker_outcome_date (Outcome, CreatedAt),
    KEY ix_logtracker_trace (TraceId),
    KEY ix_logtracker_actor (TenantId, ActorType, AppUserId),
    KEY ix_logtracker_date (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
