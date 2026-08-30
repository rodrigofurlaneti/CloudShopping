using CloudShopping.Application.Abstractions.Data;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderState.Commands.UpdateOrderStatus
{
    public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusCommandHandler(AppDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        async Task IRequestHandler<UpdateOrderStatusCommand, Unit>.Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var orderStatus = await _context.Set<Domain.Entities.Orders.OrderStatus>()
                .FirstOrDefaultAsync(os => os.Id == request.Id, cancellationToken);
            if (orderStatus is null)
            {
                throw new KeyNotFoundException("Status do pedido não encontrado.");
            }
            orderStatus.Update(request.OrderSectorId, request.Name);
            _context.Set<Domain.Entities.Orders.OrderStatus>().Update(orderStatus);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
