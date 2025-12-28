using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class QuerySpecClassBuilder
{
    public static QuerySpecClassDefinition GetQuerySpecClassDef(this ApplicationProject project, string query, string entity)
    {
        var definition = CSharpClassDefinition.Create<QuerySpecClassDefinition>(AccessModifier.Public, project.GetQuerySpecClassName(query), ns: project.GetApplicationQuerySpecsNs())
            .UsingNs(project.Solution.Domain.GetDomainEntitiesNs())
            .AddCtor(new(AccessModifier.Public));

        definition.BaseClass = $"BaseQuerySpec<{entity}>";

        return (QuerySpecClassDefinition)definition;
    }
}
