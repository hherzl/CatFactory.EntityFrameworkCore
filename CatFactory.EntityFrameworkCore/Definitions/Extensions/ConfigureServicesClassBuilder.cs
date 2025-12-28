using CatFactory.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class ConfigureServicesClassBuilder
{
    public static ConfigureServicesClassDefinition GetApplicationConfigureServicesClassDef(this ApplicationProject project)
    {
        var definition = CSharpClassDefinition.Create<ConfigureServicesClassDefinition>(AccessModifier.Public, "ConfigureServices", ns: project.GetApplicationNs(), isStatic: true)
            .UsingNs("Microsoft.Extensions.Configuration")
            .UsingNs("Microsoft.Extensions.DependencyInjection");

        definition.Methods.Add(new(AccessModifier.Public, "IServiceCollection", "AddApplicationServices", new("IServiceCollection", "services"), new("IConfiguration", "configuration"))
        {
            IsStatic = true,
            IsExtension = true,
            Lines =
            {
                new CodeLine("return services;")
            }
        });

        return (ConfigureServicesClassDefinition)definition;
    }

    public static ConfigureServicesClassDefinition GetInfrastructureConfigureServicesClassDefinition(this EntityFrameworkCoreProject project)
    {
        var definition = CSharpClassDefinition.Create<ConfigureServicesClassDefinition>(AccessModifier.Public, "ConfigureServices", ns: project.GetInfrastructureNs(), isStatic: true)
            .UsingNs("Microsoft.Extensions.Configuration")
            .UsingNs("Microsoft.Extensions.DependencyInjection");

        definition.Methods.Add(new(AccessModifier.Public, "IServiceCollection", "AddApplicationServices", new("IServiceCollection", "services"), new("IConfiguration", "configuration"))
        {
            IsStatic = true,
            IsExtension = true,
            Lines =
            {
                new CodeLine("return services;")
            }
        });

        return (ConfigureServicesClassDefinition)definition;
    }
}
