using CloudShopping.Domain.Primitives.Results;
using System.Text.Json;
using CloudShopping.Application.Abstractions.Data;
using MediatR;
namespace CloudShopping.Application.Features.Products.Commands.UpdateCatalogDetails;
public sealed record UpdateCatalogDetailsCommand(int Id, int Version, string Slug, string Description, string? Brand, decimal WeightKg,
    decimal WidthCm, decimal HeightCm, decimal LengthCm, string? FamilyCode, string? VariantLabel, Dictionary<string, string> Attributes) : IRequest<Result<Unit>>;
