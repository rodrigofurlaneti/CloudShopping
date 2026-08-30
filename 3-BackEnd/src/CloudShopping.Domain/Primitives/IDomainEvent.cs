using MediatR;
using System;

namespace CloudShopping.Domain.Primitives
{
    /// <summary>
    /// Extende MediatR.INotification (via o pacote leve MediatR.Contracts) para que os eventos
    /// de domínio possam ser publicados e tratados através do pipeline de MediatR na camada de Application,
    /// sem que o Domain precise depender do pacote completo do MediatR.
    /// </summary>
    public interface IDomainEvent : INotification
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }
}
