using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Products.Commands.UpdateCatalogDetails;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Primitives.Results;
using FluentAssertions;
using MediatR;
using Moq;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class CatalogUseCaseSteps
{
    private readonly Mock<IProductRepository> repository=new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> work=new(MockBehavior.Strict);
    private Product product=null!;private UpdateCatalogDetailsCommand request=null!;private Result<Unit> result=null!;
    [Given(@"uma edição de catálogo na condição ""(.*)""")]
    public void GivenEdit(string condition)
    {
        product=Product.Create(1,1,"SKU","Nome",100,10);
        request=new(10,condition=="versão antiga"?2:1,"produto-editado","Descrição",null,1,2,3,4,null,null,[]);
        if(condition=="atributos excedidos")request=request with{Attributes=Enumerable.Range(1,13).ToDictionary(x=>x.ToString(),x=>"valor")};
        if(condition=="slug inválido")request=request with{Slug="URL INVALIDA"};
        repository.Setup(x=>x.GetByIdAsync(10,It.IsAny<CancellationToken>())).ReturnsAsync(condition=="ausente"?null:product);
        repository.Setup(x=>x.IsSlugOrVariantInUseAsync(10,request.Slug,null,null,It.IsAny<CancellationToken>())).ReturnsAsync(condition=="duplicado");
        work.Setup(x=>x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }
    [When("o caso de uso atualiza os detalhes do catálogo")]
    public async Task Update()=>result=await new UpdateCatalogDetailsCommandHandler(repository.Object,work.Object).Handle(request,default);
    [Then(@"a edição retorna ""(.*)"" e persiste (.*) vezes")]
    public void Verify(string code,int commits)
    {
        if(code=="sucesso"){result.IsSuccess.Should().BeTrue();product.Slug.Should().Be("produto-editado");}else{result.IsFailure.Should().BeTrue();result.Error.Code.Should().Be(code);product.Slug.Should().NotBe("produto-editado");}
        work.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Exactly(commits));
    }
}
