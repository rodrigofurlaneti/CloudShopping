using System;
using System.Collections.Generic;
using MediatR;

namespace CloudShopping.Application.Features.Customers.Queries.GetCustomerById
{
    public sealed record CustomerAddressViewModel(int Id, string Street, string Number, string? Neighborhood, string City, string State, string ZipCode, bool IsDefault);

    public sealed record CustomerViewModel(
        int Id,
        string? Email,
        string CustomerTypeId,
        string? FullName,
        string? CompanyName,
        IReadOnlyCollection<CustomerAddressViewModel> Addresses);

    public sealed record GetCustomerByIdQuery(int Id) : IRequest<CustomerViewModel?>;
}
