namespace CloudShopping.Application.Features.Notifications.ViewModels;
public sealed record NotificationView(string Id,int OrderId,string Kind,DateTime CreatedAt,DateTime? ReadAt);
public sealed record OutboxView(string Id,int OrderId,string Kind,string State,int Attempts,string? LastError,DateTime CreatedAt,DateTime? ProcessedAt);
