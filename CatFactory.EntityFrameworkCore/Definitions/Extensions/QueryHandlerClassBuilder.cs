using CatFactory.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class QueryHandlerClassBuilder
{
    public static QueryHandlerClassDefinition GetSingleQueryHandlerClassDef(this ApplicationProject project, string feature, string query)
    {
        var definition = CSharpClassDefinition.Create<QueryHandlerClassDefinition>(AccessModifier.Public, project.GetQueryHandlerClassName(query), ns: project.GetApplicationFeaturesNs(feature), isSealed: true)
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
                .AddParam(project.GetQueryClassName(query), "request")
                .AddParam("CancellationToken", "ct", "default")
                .Set(body =>
                {
                    body.Line("await Task.FromResult(0);");
                })
            );

        return (QueryHandlerClassDefinition)definition;
    }

    public static QueryHandlerClassDefinition GetComposedQueryHandlerClassDef(this ApplicationProject project, string feature, string query)
    {
        var definition = CSharpClassDefinition.Create<QueryHandlerClassDefinition>(AccessModifier.Public, project.GetQueryHandlerClassName(query), ns: project.GetApplicationFeaturesNs(feature), isSealed: true)
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
                .AddParam(project.GetQueryClassName(query), "request")
                .AddParam("CancellationToken", "ct", "default")
                .Set(body =>
                {
                    body.Line("await Task.FromResult(0);");
                })
            );

        return (QueryHandlerClassDefinition)definition;
    }
}
