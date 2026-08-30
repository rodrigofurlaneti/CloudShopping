using System.Collections.Generic;

namespace CloudShopping.Domain.Primitives
{
    /// <summary>
    /// Marcador não genérico para permitir que a infraestrutura (ex: DbContext)
    /// colete e despache os eventos de domínio de qualquer Aggregate Root rastreado,
    /// independentemente do tipo concreto de TId.
    /// </summary>
    public interface IHasDomainEvents
    {
        IReadOnlyCollection<IDomainEvent> GetDomainEvents();
        void ClearDomainEvents();
    }
}
