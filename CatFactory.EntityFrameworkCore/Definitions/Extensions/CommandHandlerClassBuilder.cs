using CatFactory.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class CommandHandlerClassBuilder
{
    public static CommandHandlerClassDefinition GetCreateCommandHandlerClassDef(this ApplicationProject project, string feature, string command)
    {
        var definition = CSharpClassDefinition.Create<CommandHandlerClassDefinition>(AccessModifier.Public, project.GetCommandHandlerClassName(command), ns: project.GetApplicationFeaturesNs(feature), isSealed: true)
            .UsingNs("Microsoft.EntityFrameworkCore")
            .UsingNs(project.GetApplicationCommonNs())
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
                    body.Line("await Task.FromResult(0);");
                })
            );

        return (CommandHandlerClassDefinition)definition;
    }
}
