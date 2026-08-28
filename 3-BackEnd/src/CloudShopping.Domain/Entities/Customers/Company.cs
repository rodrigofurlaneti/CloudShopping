using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Company : AuditableEntity<int>
    {
        public string BusinessTaxId { get; private set; }
        public string CompanyName { get; private set; }
        public string? StateTaxId { get; private set; }
        private Company() { }
        public static Company Create(int customerId, string businessTaxId, string companyName, string? stateTaxId)
        {
            var company = new Company
            {
                BusinessTaxId = businessTaxId,
                CompanyName = companyName,
                StateTaxId = stateTaxId
            };
            company.Id = customerId;
            return company;
        }
    }
}
