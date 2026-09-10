using CloudShopping.Application;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Departments.Commands.CreateDepartment;
using CloudShopping.Application.Features.Departments.Commands.UpdateDepartment;
using CloudShopping.Application.Features.Departments.Commands.DeleteDepartment;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Moq;
using Xunit;

namespace CloudShopping.Tests.Units.Application;
public class DepartmentTests
{
    private readonly Mock<IDepartmentRepository> repository=new();
    private readonly Mock<IUnitOfWork> uow=new();
    private ServiceProvider Provider()
    {
        var tenant=new Mock<ITenantProvider>();tenant.Setup(x=>x.GetTenantId()).Returns(1);
        return new ServiceCollection().AddApplication().AddSingleton(repository.Object)
            .AddSingleton(uow.Object).AddSingleton(tenant.Object).BuildServiceProvider();
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Creation_enforces_unique_slug_within_tenant(bool duplicate)
    {
        repository.Setup(x=>x.SlugExistsAsync(1,"calcados",It.IsAny<CancellationToken>())).ReturnsAsync(duplicate);
        Department? saved=null;
        repository.Setup(x=>x.AddAsync(It.IsAny<Department>(),It.IsAny<CancellationToken>()))
            .Callback<Department,CancellationToken>((d,_)=>saved=d).Returns(Task.CompletedTask);
        using var provider=Provider();
        var handler=provider.GetRequiredService<IRequestHandler<CreateDepartmentCommand,Result<int>>>();
        var result=await handler.Handle(new("Calçados","calcados"),default);
        result.IsSuccess.Should().Be(!duplicate);
        if(duplicate){result.Error.Code.Should().Be("Department.SlugNotUnique");saved.Should().BeNull();}
        else {saved!.TenantId.Should().Be(1);saved.Name.Should().Be("Calçados");saved.Slug.Should().Be("calcados");}
        uow.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),duplicate?Times.Never():Times.Once());
    }

    [Theory]
    [InlineData("missing","Department.NotFound")]
    [InlineData("foreign","Department.NotFound")]
    [InlineData("system","Department.SystemDefault")]
    public async Task Update_and_delete_cannot_modify_unavailable_departments(string kind,string code)
    {
        var department=kind switch {"missing"=>null,"foreign"=>Department.CreateForTenant(2,"Nome","nome"),_=>Department.CreateSystemDefault("Nome","nome")};
        repository.Setup(x=>x.GetByIdAsync(1,It.IsAny<CancellationToken>())).ReturnsAsync(department);
        using var provider=Provider();
        var update=await provider.GetRequiredService<IRequestHandler<UpdateDepartmentCommand,Result>>().Handle(new(1,"Novo","novo"),default);
        var delete=await provider.GetRequiredService<IRequestHandler<DeleteDepartmentCommand,Result>>().Handle(new(1),default);
        update.Error.Code.Should().Be(code);delete.Error.Code.Should().Be(code);
        uow.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Never);
        repository.Verify(x=>x.Update(It.IsAny<Department>()),Times.Never);
        repository.Verify(x=>x.Remove(It.IsAny<Department>()),Times.Never);
    }

    [Fact]
    public async Task Update_and_delete_persist_the_tenant_department()
    {
        var department=Department.CreateForTenant(1,"Nome","nome");
        repository.Setup(x=>x.GetByIdAsync(1,It.IsAny<CancellationToken>())).ReturnsAsync(department);
        using var provider=Provider();
        var update=await provider.GetRequiredService<IRequestHandler<UpdateDepartmentCommand,Result>>().Handle(new(1,"Novo","novo"),default);
        update.IsSuccess.Should().BeTrue();department.Name.Should().Be("Novo");department.Slug.Should().Be("novo");
        var delete=await provider.GetRequiredService<IRequestHandler<DeleteDepartmentCommand,Result>>().Handle(new(1),default);
        delete.IsSuccess.Should().BeTrue();repository.Verify(x=>x.Update(department),Times.Once);
        repository.Verify(x=>x.Remove(department),Times.Once);uow.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Exactly(2));
    }
}
