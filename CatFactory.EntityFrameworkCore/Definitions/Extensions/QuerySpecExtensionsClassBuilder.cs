using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class QuerySpecExtensionsClassBuilder
{
    public static QuerySpecExtensionsClassDefinition GetQuerySpecExtensionsClassDef(this ApplicationProject project)
    {
        var definition = CSharpClassDefinition.Create<QuerySpecExtensionsClassDefinition>(AccessModifier.Public, project.GetQuerySpecExtensionsClassName())
            .Ns(project.GetApplicationQuerySpecsNs())
            .IsStatic()
            .UsingNs("Microsoft.EntityFrameworkCore");

        var method = new MethodDefinition(AccessModifier.Public, "IQueryable<TQueryModel>", "QuerySpec")
            .IsExtension(true)
            .AddParam("IQueryable<TQueryModel>", "query")
            .AddParam("IBaseQuerySpec<TQueryModel>", "querySpec")
            .Set(body =>
            {
                body.Comment(" Fetch a Queryable that includes all expression-based includes");
                body.Line("var queryableResultWithIncludes = querySpec.Includes.Aggregate(query, (current, include) => current.Include(include));");
                body.Empty();
                body.Comment(" Modify the IQueryable to include any string-based include statements");
                body.Line("var secondaryResult = querySpec.IncludeStrings.Aggregate(queryableResultWithIncludes, (current, include) => current.Include(include));");
                body.Empty();
                body.Comment(" Return the result of the query using the query spec's criteria expression");
                body.Line("return querySpec.Criteria == null ? secondaryResult : secondaryResult.Where(querySpec.Criteria);");
            });

        method.GenericTypes.Add(new GenericTypeDefinition("TQueryModel") { Constraints = ["TQuery<Model>", "class"] });

        definition.Add(method);

        return (QuerySpecExtensionsClassDefinition)definition;
    }
}
