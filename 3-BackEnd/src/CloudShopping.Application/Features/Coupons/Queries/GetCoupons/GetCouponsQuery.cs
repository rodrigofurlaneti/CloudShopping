using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Coupons.ViewModels;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Coupons.Queries.GetCoupons;
public sealed record GetCouponsQuery(int Page = 1) : IRequest<IReadOnlyList<CouponView>>;
