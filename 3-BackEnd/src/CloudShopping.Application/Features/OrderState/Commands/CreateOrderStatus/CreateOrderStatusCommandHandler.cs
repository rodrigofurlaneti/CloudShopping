using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.OrderStatus.Commands.CreateOrderStatus;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderState.Commands.CreateOrderStatus
{
    public sealed class CreateOrderStatusCommandHandler : IRequestHandler<CreateOrderStatusCommand, int>
    {
        private readonly AppDbContext _context; // Ou injete um IOrderStatusRepository
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderStatusCommandHandler(AppDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            // Cria a entidade mapeada conforme o seu banco de dados
            var orderStatus = new Domain.Entities.Orders.OrderStatus(request.OrderSectorId, request.Name);

            await _context.Set<Domain.Entities.Orders.OrderStatus>().AddAsync(orderStatus, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return orderStatus.Id;
        }
    }
}
