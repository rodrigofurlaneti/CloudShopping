using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Coupons.ViewModels;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Coupons.Commands.CreateCoupon;
public sealed record CreateCouponCommand(string Code, string Kind, decimal Value, decimal MinimumSubtotal, int UsageLimit, int PerCustomerLimit, DateTime StartsAt, DateTime EndsAt) : IRequest<Result<CouponView>>;
