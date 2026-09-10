using CloudShopping.Domain.Primitives.Results;
using MediatR;
using CloudShopping.Application.Abstractions.Services;
namespace CloudShopping.Application.Features.Storefront.Queries.GetPostalAddress;
public sealed record GetPostalAddressQuery(string ZipCode) : IRequest<Result<PostalAddress?>>;
