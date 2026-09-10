namespace CloudShopping.Application.Abstractions.Files;

/// <summary>The caller owns Content and keeps it open until the operation completes.</summary>
public sealed record UploadFile(string FileName, long Length, Stream Content);
