using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CloudShopping.Tests.Units.Domain;
public class CustomerTests
{
    [Fact]
    public void Guest_becomes_lead_and_registered_consumer()
    {
        var c=Customer.CreateGuest(3);c.TenantId.Should().Be(3);c.CustomerTypeId.Should().Be(CustomerType.Guest);
        c.SessionToken.Should().NotBeEmpty();c.ConvertToLead(" CLIENTE@EXAMPLE.COM ");
        c.Email.Should().Be("cliente@example.com");c.CustomerTypeId.Should().Be(CustomerType.Lead);
        c.RegisterAsB2C("52998224725"," Cliente ",null);c.CustomerTypeId.Should().Be(CustomerType.B2C);
        c.Individual!.FullName.Should().Be("Cliente");
        var birth=new DateTime(1990,1,1);c.UpdateB2CProfile(" Novo Nome ",birth);
        c.Individual.FullName.Should().Be("Novo Nome");c.Individual.BirthDate.Should().Be(birth);
        c.SetPassword("hash1");c.PasswordHash.Should().Be("hash1");
        c.ChangePassword("hash2");c.PasswordHash.Should().Be("hash2");
        c.ChangeEmail(" NOVO@EXAMPLE.COM ");c.Email.Should().Be("novo@example.com");
    }
    [Fact]
    public void Company_profile_can_be_updated_without_changing_document()
    {
        var c=Customer.CreateGuest(1);c.RegisterAsB2B("11222333000181"," Empresa ",null);
        c.CustomerTypeId.Should().Be(CustomerType.B2B);c.Company!.CompanyName.Should().Be("Empresa");
        c.UpdateB2BProfile(" Nova Empresa "," IE ");c.Company.CompanyName.Should().Be("Nova Empresa");
        c.Company.StateTaxId.Should().Be("IE");c.Company.BusinessTaxId.Should().Be("11222333000181");
    }
    [Theory]
    [InlineData("b2c-to-b2b")] [InlineData("b2b-to-b2c")]
    [InlineData("guest-update-person")] [InlineData("guest-update-company")]
    [InlineData("lead-twice")]
    public void Incompatible_customer_transitions_are_rejected(string operation)
    {
        var c=Customer.CreateGuest(1);Action act=()=>{switch(operation){
            case "b2c-to-b2b":c.RegisterAsB2C("52998224725","Nome",null);c.RegisterAsB2B("11222333000181","Empresa",null);break;
            case "b2b-to-b2c":c.RegisterAsB2B("11222333000181","Empresa",null);c.RegisterAsB2C("52998224725","Nome",null);break;
            case "guest-update-person":c.UpdateB2CProfile("Nome",null);break;
            case "guest-update-company":c.UpdateB2BProfile("Empresa",null);break;
            case "lead-twice":c.ConvertToLead("a@example.com");c.ConvertToLead("b@example.com");break;
        }};act.Should().Throw<InvalidOperationException>();
    }
    [Theory]
    [InlineData("email")] [InlineData("password")] [InlineData("change-password")]
    public void Empty_credentials_are_rejected(string field)
    {
        var c=Customer.CreateGuest(1);Action act=()=>{switch(field){
            case "email":c.ChangeEmail(" ");break;case "password":c.SetPassword(" ");break;default:c.ChangePassword(" ");break;}};
        act.Should().Throw<ArgumentException>();c.Email.Should().BeNull();c.PasswordHash.Should().BeNull();
    }
    [Theory]
    [InlineData("52998224725",true)] [InlineData("11222333000181",true)]
    [InlineData("52998224724",false)] [InlineData("11222333000180",false)]
    [InlineData("11111111111",false)] [InlineData("",false)] [InlineData("5299822472A",false)]
    public void Tax_documents_require_valid_check_digits(string value,bool valid)=>TaxDocument.IsValid(value).Should().Be(valid);
}
