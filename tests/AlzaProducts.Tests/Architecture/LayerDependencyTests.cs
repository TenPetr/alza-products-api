using AlzaProducts.Domain;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace AlzaProducts.Tests.Architecture;

/// <summary>
/// The layers are folders of a single project, so the compiler cannot stop a layer from using an outer one.
/// These rules enforce the Onion architecture instead: dependencies only point inward.
/// </summary>
public class LayerDependencyTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(typeof(Product).Assembly).Build();

    private static readonly IObjectProvider<IType> DomainLayer = Layer("Domain");
    private static readonly IObjectProvider<IType> ApplicationLayer = Layer("Application");
    private static readonly IObjectProvider<IType> InfrastructureLayer = Layer("Infrastructure");
    private static readonly IObjectProvider<IType> ApiLayer = Layer("Api");

    // ASP.NET Core and EF Core live in other assemblies, so they are matched by namespace of the referenced types
    private const string WebAndDataFrameworks = @"^Microsoft\.(AspNetCore|EntityFrameworkCore)(\..+)?$";

    [Fact]
    public void Domain_DependsOnNoOtherLayerNorFramework() =>
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(WebAndDataFrameworks)
            .Check(Architecture);

    [Fact]
    public void Application_DependsOnNeitherOuterLayersNorFrameworks() =>
        Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(WebAndDataFrameworks)
            .Check(Architecture);

    private static IObjectProvider<IType> Layer(string name) => Types()
        .That().ResideInNamespaceMatching($@"^AlzaProducts\.{name}(\..+)?$")
        .As($"{name} layer");
}
