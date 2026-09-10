using CloudShopping.Application.Abstractions.Data;
using MediatR;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Products.ViewModels;
namespace CloudShopping.Application.Features.Products.Queries.GetCatalogDetails;
public sealed record GetCatalogDetailsQuery(int Page) : IRequest<IReadOnlyList<CatalogDetailsView>>;
