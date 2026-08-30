using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public sealed record OrderAddressViewModel(string Street, string Number, string Neighborhood, string City, string State, string ZipCode);
}
