using CatFactory.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class BaseQuerySpecClassBuilder
{
    public static QuerySpecBaseClassDefinition GetBaseQuerySpecClassDef(this ApplicationProject project)
    {
        var definition = CSharpClassDefinition.Create<QuerySpecBaseClassDefinition>(AccessModifier.Public, project.GetBaseQuerySpecClassName(), ns: project.GetApplicationQuerySpecsNs())
            .UsingNs("System.Linq.Expressions")
            .AddCtor(new(AccessModifier.Public)
            {
                Lines =
                {
                    new CodeLine("Includes = [];"),
                    new CodeLine("IncludeStrings = [];")
                }
            })
            .Add(new MethodDefinition(AccessModifier.Protected, "void", "AddInclude")
                .IsVirtual(true)
                .AddParam("Expression<Func<TQuerySpec, object>>", "includeExpression")
                .Set(body => body.Line("Includes.Add(includeExpression);"))
            )
            .Add(new MethodDefinition(AccessModifier.Protected, "void", "AddInclude")
                .IsVirtual(true)
                .AddParam("string ", "includeString")
                .Set(body => body.Line("IncludeStrings.Add(includeString);"))
            );

        definition.GenericTypes.Add(new GenericTypeDefinition { Name = "TQuerySpec" });

        definition.AddAutomaticProperty("Expression<Func<TQuerySpec, bool>>", "Criteria");
        definition.AddAutomaticProperty("List<Expression<Func<TQuerySpec, object>>>", "Includes");
        definition.AddAutomaticProperty("List<string>", "IncludeStrings");

        return (QuerySpecBaseClassDefinition)definition;
    }

    public static CSharpInterfaceDefinition GetBaseQuerySpecInterfaceDef(this ApplicationProject project)
    {
        var classDefinition = CSharpClassDefinition.Create<QuerySpecBaseClassDefinition>(AccessModifier.Public, project.GetBaseQuerySpecClassName(), ns: project.GetApplicationQuerySpecsNs())
            .UsingNs("System.Linq.Expressions")
            .AddCtor(new(AccessModifier.Public))
            .Add(new MethodDefinition(AccessModifier.Protected, "void", "AddInclude")
                .IsVirtual(true)
                .AddParam("Expression<Func<TQuerySpec, object>>", "includeExpression")
                .Set(body => body.Line("Includes.Add(includeExpression);"))
            )
            .Add(new MethodDefinition(AccessModifier.Protected, "void", "AddInclude")
                .IsVirtual(true)
                .AddParam("string ", "includeString")
                .Set(body => body.Line("IncludeStrings.Add(includeString);"))
            );

        classDefinition.GenericTypes.Add(new GenericTypeDefinition { Name = "TQuerySpec" });

        classDefinition.AddAutomaticProperty("Expression<Func<TQuerySpec, bool>>", "Criteria");
        classDefinition.AddAutomaticProperty("List<Expression<Func<TQuerySpec, object>>>", "Includes");
        classDefinition.AddAutomaticProperty("List<string>", "IncludeStrings");

        var interfaceDefinition = classDefinition.RefactInterface();

        interfaceDefinition.GenericTypes.Add(new GenericTypeDefinition { Name = "TQuerySpec" });

        return interfaceDefinition;
    }
}
