using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class DbContextInterfaceBuilder
{
    public static DbContextInterfaceDefinition GetDbContextInterfaceDef(this ApplicationProject project)
    {
        var definition = CSharpInterfaceDefinition.Create<DbContextInterfaceDefinition>(AccessModifier.Public, project.GetDbContextInterfaceName(project.Database), ns: project.GetApplicationCommonNs())
            .UsingNs("Microsoft.EntityFrameworkCore")
            .UsingNs(project.Solution.Domain.GetDomainEntitiesNs())
            ;

        definition.Methods.Add(new MethodDefinition("Task<int>", "SaveChangesAsync").AddParam("CancellationToken", "ct", "default"));

        foreach (var table in project.Database.Tables)
        {
            definition.Properties.Add(new($"DbSet<{project.Solution.Domain.GetEntityName(table)}>", project.GetPropName(table.Name))
            {
                IsAutomatic = true
            });
        }

        return (DbContextInterfaceDefinition)definition;
    }
}
