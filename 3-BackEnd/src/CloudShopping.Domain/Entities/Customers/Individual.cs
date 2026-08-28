using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Individual : AuditableEntity<int>
    {
        public string TaxId { get; private set; }
        public string FullName { get; private set; }
        public DateTime? BirthDate { get; private set; }
        private Individual() { }
        public static Individual Create(int customerId, string taxId, string fullName, DateTime? birthDate)
        {
            var individual = new Individual
            {
                TaxId = taxId,
                FullName = fullName,
                BirthDate = birthDate
            };
            individual.Id = customerId;
            return individual;
        }
    }
}
