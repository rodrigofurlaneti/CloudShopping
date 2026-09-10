using Microsoft.EntityFrameworkCore;
using CloudShopping.Application.Abstractions.Services;
using System.Reflection;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Primitives;
using CloudShopping.Infrastructure.Persistence;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;
using Xunit;

namespace CloudShopping.Tests.Arch;

public sealed class ArchitectureRules
{
    private static readonly Assembly Domain = typeof(Product).Assembly;
    private static readonly Assembly Application = typeof(IProductRepository).Assembly;
    private static readonly Assembly Infrastructure = typeof(AppDbContext).Assembly;
    private static readonly Assembly Api = typeof(CloudShopping.Api.Controllers.AccessController).Assembly;

    private static bool Inherits(Type type, Type generic)
    {
        for (var parent = type.BaseType; parent != null; parent = parent.BaseType)
            if (parent.IsGenericType && parent.GetGenericTypeDefinition() == generic) return true;
        return false;
    }
    private static Type[] Entities => Domain.GetTypes().Where(t => t.IsClass && !t.IsAbstract && Inherits(t, typeof(Entity<>))).ToArray();
    private static Type[] Requests => Application.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))).ToArray();

    [Theory]
    [InlineData("CloudShopping.Application")]
    [InlineData("CloudShopping.Infrastructure")]
    [InlineData("CloudShopping.Api")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData("MySqlConnector")]
    [InlineData("Dapper")]
    public void Domain_is_independent_of_outer_layers(string dependency) => AssertNoDependency(Domain, dependency);

    [Theory]
    [InlineData("CloudShopping.Infrastructure")]
    [InlineData("CloudShopping.Api")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("MySqlConnector")]
    [InlineData("Dapper")]
    [InlineData("Microsoft.AspNetCore.Http")]
    public void Application_does_not_use_database_or_API_implementations(string dependency) => AssertNoDependency(Application, dependency);

    [Fact]
    public void Infrastructure_does_not_depend_on_API() => AssertNoDependency(Infrastructure, "CloudShopping.Api");

    [Fact]
    public void Controllers_do_not_depend_on_infrastructure()
    {
        var result = Types.InAssembly(Api).That().Inherit(typeof(ControllerBase)).ShouldNot().HaveDependencyOn("CloudShopping.Infrastructure").GetResult();
        result.IsSuccessful.Should().BeTrue("controllers must delegate to application; violations: {0}", string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Entities_hide_setters_and_mutable_collections()
    {
        var violations = Entities.SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.SetMethod?.IsPublic == true || (p.PropertyType.IsGenericType && new[] { typeof(List<>), typeof(ICollection<>), typeof(IList<>) }.Contains(p.PropertyType.GetGenericTypeDefinition())))
            .Select(p => $"{t.FullName}.{p.Name}")).ToArray();
        violations.Should().BeEmpty("state changes must use domain methods");
    }

    [Fact]
    public void Entities_have_no_public_constructors() => Entities.Where(t => t.GetConstructors().Length > 0).Select(t => t.FullName).Should().BeEmpty("entities use factories or aggregate operations");

    [Fact]
    public void Repositories_are_implemented_in_infrastructure()
    {
        var contracts = Application.GetTypes().Where(t => t.IsInterface && t.Name.EndsWith("Repository") && !t.ContainsGenericParameters).ToArray();
        contracts.Should().NotBeEmpty();
        var missing = contracts.Where(c => !Infrastructure.GetTypes().Any(t => t.IsClass && !t.IsAbstract && c.IsAssignableFrom(t))).Select(t => t.FullName);
        missing.Should().BeEmpty("every concrete repository contract must have an adapter");
    }

    [Fact]
    public void Every_request_has_exactly_one_handler()
    {
        var violations = Requests.Where(r => Application.GetTypes().Count(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) && i.GenericTypeArguments[0] == r)) != 1);
        violations.Select(t => t.FullName).Should().BeEmpty();
    }

    [Fact]
    public void Commands_and_queries_have_separate_namespaces()
    {
        var violations = Requests.Where(t => t.Name.EndsWith("Command") ? !t.Namespace!.Contains(".Commands.") : t.Name.EndsWith("Query") ? !t.Namespace!.Contains(".Queries") : true);
        violations.Select(t => t.FullName).Should().BeEmpty("CQRS requests must be explicitly commands or queries");
    }

    [Fact]
    public void Every_command_has_a_validator()
    {
        var validators = Application.GetTypes().Where(t => t.IsClass && !t.IsAbstract).SelectMany(t => t.GetInterfaces()).Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)).Select(i => i.GenericTypeArguments[0]).ToHashSet();
        Requests.Where(t => t.Name.EndsWith("Command") && !validators.Contains(t)).Select(t => t.FullName).Should().BeEmpty("commands need validation before side effects");
    }

    [Fact]
    public void Query_handlers_do_not_request_unit_of_work()
    {
        var queries = Application.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("QueryHandler"));
        queries.Where(t => t.GetConstructors().SelectMany(c => c.GetParameters()).Any(p => p.ParameterType == typeof(IUnitOfWork)))
            .Select(t => t.FullName).Should().BeEmpty("queries must not commit changes");
    }

    [Fact]
    public void Persistence_models_belong_to_domain()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql("Server=127.0.0.1;Port=33077;User ID=root;Database=architecture_model_only", new MySqlServerVersion(new Version(8, 0, 43))).Options;
        using var db = new AppDbContext(options, new ModelTenant());
        var violations = db.Model.GetEntityTypes().Select(x => x.ClrType).Where(t => t.Assembly != Domain).Select(t => t.FullName).Order().ToArray();
        violations.Should().BeEmpty("EF business models must belong to Domain; violations: {0}", string.Join(", ", violations));
    }

    [Fact]
    public void Specs_reference_only_domain_and_application()
    {
        var file = Path.Combine(RepositoryRoot(), "3-BackEnd", "tests", "CloudShopping.Tests.Specs", "CloudShopping.Tests.Specs.csproj");
        var projects = System.Xml.Linq.XDocument.Load(file).Descendants("ProjectReference").Select(x => Path.GetFileName(((string)x.Attribute("Include")!).Replace('\\', '/'))).Order().ToArray();
        projects.Should().Equal("CloudShopping.Application.csproj", "CloudShopping.Domain.csproj");
    }

    private sealed class ModelTenant : ITenantProvider { public int GetTenantId() => 1; }

    private static void AssertNoDependency(Assembly assembly, string dependency)
    {
        var result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOn(dependency).GetResult();
        result.IsSuccessful.Should().BeTrue("{0} must not depend on {1}; violations: {2}", assembly.GetName().Name, dependency, string.Join(", ", result.FailingTypeNames ?? []));
    }
    private static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            if (Directory.Exists(Path.Combine(directory.FullName, "3-BackEnd", "src"))) return directory.FullName;
        throw new DirectoryNotFoundException("CloudShopping workspace not found.");
    }
}
