using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class QueryClassBuilder
{
    public static QueryClassDefinition GetSingleQueryClassDef(this ApplicationProject project, string feature, string query, ITable table)
    {
        var definition = CSharpClassDefinition.Create<QueryClassDefinition>(AccessModifier.Public, project.GetQueryClassName(query), ns: project.GetApplicationFeaturesNs(feature))
            .UsingNs("System")
            .AddCtor(new ClassConstructorDefinition(AccessModifier.Public));

        return (QueryClassDefinition)definition;
    }

    public static QueryClassDefinition GetComposedQueryClassDef(this ApplicationProject project, string feature, string query, ITable table)
    {
        var definition = CSharpClassDefinition.Create<QueryClassDefinition>(AccessModifier.Public, project.GetQueryClassName(query), ns: project.GetApplicationFeaturesNs(feature))
            .UsingNs("System")
            .AddCtor(new ClassConstructorDefinition(AccessModifier.Public));

        definition.AddAutomaticProp(AccessModifier.Public, "int", "PageSize");
        definition.AddAutomaticProp(AccessModifier.Public, "int", "PageNumber");

        return (QueryClassDefinition)definition;
    }
}
