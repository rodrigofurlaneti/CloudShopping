using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Reports.Queries.ExportReport;
using CloudShopping.Application.Features.Reports.ViewModels;
using FluentAssertions;
using Moq;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class ReportsSteps
{
    private readonly Mock<IReportRepository> repository=new(MockBehavior.Strict);private CloudShopping.Domain.Primitives.Results.Result<ReportExport>? result;
    [When(@"o relatório exporta (.*) pedidos em um período de (.*) dias")]
    public async Task Export(int count,int days)
    {
        var rows=Enumerable.Range(1,count).Select(i=>new ReportOrder(i,new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),100,"Paid","Shipped",false)).ToArray();
        repository.Setup(x=>x.GetExportOrdersAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),5001,It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        result=await new ExportReportQueryHandler(repository.Object).Handle(new(new DateOnly(2026,1,1),new DateOnly(2026,1,1).AddDays(days-1)),default);
    }
    [Then(@"a exportação é ""(.*)"" e consulta o repositório (.*) vezes")]
    public void Verify(string status,int calls)
    {if(status=="aceita"){result!.IsSuccess.Should().BeTrue();result.Value.FileName.Should().StartWith("pedidos-");result.Value.Content.Should().StartWith("Pedido,Data UTC");}else { result!.IsFailure.Should().BeTrue(); result.Error.Code.Should().Be("UseCase.Invalid"); }repository.Verify(x=>x.GetExportOrdersAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),5001,It.IsAny<CancellationToken>()),Times.Exactly(calls));}
    [Then(@"a célula CSV ""(.*)"" é protegida contra fórmula")]
    public void Csv(string input)=>CloudShopping.Application.Features.Reports.ReportCsv.Cell(input).Should().StartWith("\"'");
}
