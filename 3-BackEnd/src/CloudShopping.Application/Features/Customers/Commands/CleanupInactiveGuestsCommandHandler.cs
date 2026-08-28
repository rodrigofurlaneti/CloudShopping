using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class CleanupInactiveGuestsCommandHandler : IRequestHandler<CleanupInactiveGuestsCommand, Result<int>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CleanupInactiveGuestsCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<int>> Handle(CleanupInactiveGuestsCommand request, CancellationToken cancellationToken)
        {
            var inactiveGuests = await _customerRepository.GetInactiveGuestsAsync(days: 30, cancellationToken);
            int count = 0;
            foreach (var guest in inactiveGuests)
            {
                guest.Deactivate();
                _customerRepository.Update(guest);
                count++;
            }
            if (count > 0)
            {
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            return Result.Success(count);
        }
    }
}
