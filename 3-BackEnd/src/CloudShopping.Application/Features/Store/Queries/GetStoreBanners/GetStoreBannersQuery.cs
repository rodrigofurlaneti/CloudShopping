using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;

namespace CloudShopping.Application.Features.Store.Queries.GetStoreBanners
{
    public sealed record GetStoreBannersQuery() : IRequest<Result<IEnumerable<StoreBannerResponse>>>;
}