using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Diagnostics;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
namespace CloudShopping.Infrastructure.Repositories;

public sealed class ProcessingLogWriter(IConfiguration configuration) : IProcessingLogWriter
{
    public async Task WriteAsync(LogTracker entry, CancellationToken cancellationToken)
    {
        var settings = new MySqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection is not configured.")) { AutoEnlist = false };
        await using var connection = new MySqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 2;
        command.CommandText = """
            INSERT INTO logtracker
            (TenantId, AppUserId, ActorType, TraceId, DirectoryName, ClassName, MethodName,
             Outcome, IsSuccess, RequestAborted, ExecutionTimeMs, HttpMethod, HttpStatusCode,
             ExceptionType, Message, ErrorMessage, StackTrace, IpAddress, CreatedAt, IsActive)
            VALUES (@TenantId, @AppUserId, @ActorType, @TraceId, @DirectoryName, @ClassName, @MethodName,
             @Outcome, @IsSuccess, @RequestAborted, @ExecutionTimeMs, @HttpMethod, @HttpStatusCode,
             @ExceptionType, @Message, @ErrorMessage, @StackTrace, @IpAddress, @CreatedAt, @IsActive)
            """;
        void Parameter(string name, object? value) => command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        Parameter("@TenantId", entry.TenantId); Parameter("@AppUserId", entry.AppUserId);
        Parameter("@ActorType", entry.ActorType); Parameter("@TraceId", entry.TraceId);
        Parameter("@DirectoryName", entry.DirectoryName); Parameter("@ClassName", entry.ClassName);
        Parameter("@MethodName", entry.MethodName); Parameter("@Outcome", entry.Outcome);
        Parameter("@IsSuccess", entry.IsSuccess); Parameter("@RequestAborted", entry.RequestAborted);
        Parameter("@ExecutionTimeMs", entry.ExecutionTimeMs); Parameter("@HttpMethod", entry.HttpMethod);
        Parameter("@HttpStatusCode", entry.HttpStatusCode); Parameter("@ExceptionType", entry.ExceptionType);
        Parameter("@Message", entry.Message); Parameter("@ErrorMessage", entry.ErrorMessage);
        Parameter("@StackTrace", entry.StackTrace); Parameter("@IpAddress", entry.IpAddress);
        Parameter("@CreatedAt", entry.CreatedAt); Parameter("@IsActive", entry.IsActive);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
