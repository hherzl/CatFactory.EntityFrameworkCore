using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class QuerySpecClassBuilder
{
    public static QuerySpecClassDefinition GetQuerySpecClassDef(this ApplicationProject project, string query, string entity, ITable table)
    {
        var definition = CSharpClassDefinition.Create<QuerySpecClassDefinition>(AccessModifier.Public, project.GetQuerySpecClassName(query))
            .Ns(project.GetApplicationQuerySpecsNs())
            .UsingNs(project.Solution.Domain.GetDomainEntitiesNs())
            .AddCtor(new(AccessModifier.Public));

        var defaultCtor = definition.Constructors.First();
        if (table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1)
        {
            var column = (Column)table.GetColumnsFromConstraint(table.PrimaryKey).First();
            var type = project.Database.ResolveDatabaseType(column);
            var names = new
            {
                Param = project.CodeNamingConvention.GetParameterName(column.Name),
                Prop = project.CodeNamingConvention.GetPropertyName(column.Name)
            };
            defaultCtor.AddParam(type, names.Param);
            defaultCtor.AddLine($"Criteria = entity => entity.{names.Prop} == {names.Param};");
        }

        definition.BaseClass = $"BaseQuerySpec<{entity}>";

        definition.SimplifyDataTypes();

        return (QuerySpecClassDefinition)definition;
    }
}
