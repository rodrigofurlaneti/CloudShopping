using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Commands.UpdateStoreProfile;
public sealed class UpdateStoreProfileCommandHandler(IStorefrontRepository repository) : IRequestHandler<UpdateStoreProfileCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(UpdateStoreProfileCommand request, CancellationToken ct)
        => CommandExecution.Run<Unit>(async () =>
    {
        if (!TaxDocument.IsValid(request.TaxId) || (request.Type == "B2C" ? request.TaxId.Length != 11 : request.TaxId.Length != 14)) throw new ArgumentException("CPF/CNPJ inválido.");
        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.EmailUsed(email, request.CustomerId, ct)) throw new InvalidOperationException("Email já cadastrado. Entre com sua conta antes de continuar.");
        var c = await repository.Customer(request.CustomerId, ct) ?? throw new KeyNotFoundException();
        if (request.Type == "B2C") {
            if (c.Individual == null) c.RegisterAsB2C(request.TaxId, request.Name.Trim(), null);
            else { if (c.Individual.TaxId != request.TaxId) throw new InvalidOperationException("O documento cadastrado não pode ser trocado nesta tela."); c.UpdateB2CProfile(request.Name.Trim(), c.Individual.BirthDate); }
        } else {
            if (c.Company == null) c.RegisterAsB2B(request.TaxId, request.Name.Trim(), null);
            else { if (c.Company.BusinessTaxId != request.TaxId) throw new InvalidOperationException("O documento cadastrado não pode ser trocado nesta tela."); c.UpdateB2BProfile(request.Name.Trim(), c.Company.StateTaxId); }
        }
        c.ChangeEmail(email); await repository.Save(ct); return Unit.Value;
    
    });
}
