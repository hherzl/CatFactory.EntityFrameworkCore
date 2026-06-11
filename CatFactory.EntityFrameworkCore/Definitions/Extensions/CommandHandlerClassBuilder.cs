using CatFactory.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class CommandHandlerClassBuilder
{
    public static CommandHandlerClassDefinition GetCreateCommandHandlerClassDef(this ApplicationProject project, string feature, string command, ITable table)
    {
        var definition = CSharpClassDefinition.Create<CommandHandlerClassDefinition>(AccessModifier.Public, project.GetCommandHandlerClassName(command), ns: project.GetApplicationFeaturesNs(feature), isSealed: true)
            .UsingNs("Microsoft.EntityFrameworkCore")
            .UsingNs(project.GetApplicationCommonNs())
            .UsingNs(project.Solution.Domain.GetDomainEntitiesNs())
            .AddCtor(new(AccessModifier.Public, new ParameterDefinition(project.GetDbContextInterfaceName(project.Database), "dbContext"))
            {
                Lines =
                {
                    new CodeLine($"{project.CodeNamingConvention.GetFieldName("dbContext")} = dbContext;")
                }
            })
            .AddField(AccessModifier.Private, project.GetDbContextInterfaceName(project.Database), "dbContext", isReadonly: true)
            .Add(new MethodDefinition(AccessModifier.Public, "Task", "HandleAsync")
                .IsAsync()
                .AddParam(project.GetCommandClassName(command), "request")
                .AddParam("CancellationToken", "ct", "default")
                .Set(body =>
                {
                    body.Line($"var entity = new {project.Solution.Domain.GetEntityName(table)}();");
                    body.Empty();
                    body.Line($"_dbContext.{project.Solution.Infrastructure.GetDbSetPropName(table, false)}.Add(entity);");
                    body.Empty();
                    body.Line("await _dbContext.SaveChangesAsync(ct);");
                })
            );

        return (CommandHandlerClassDefinition)definition;
    }
}
