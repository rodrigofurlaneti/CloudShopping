using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
namespace CloudShopping.Application.Features.Access.Queries;
public sealed record AccessOverview(IReadOnlyList<string> Available, IReadOnlyList<AccessProfile> Profiles);
