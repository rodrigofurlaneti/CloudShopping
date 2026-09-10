using CloudShopping.Domain.Entities.Diagnostics;
namespace CloudShopping.Application.Abstractions.Data;

// Independent diagnostic persistence; never commits the business unit of work.
public interface IProcessingLogWriter
{
    Task WriteAsync(LogTracker entry, CancellationToken cancellationToken);
}
