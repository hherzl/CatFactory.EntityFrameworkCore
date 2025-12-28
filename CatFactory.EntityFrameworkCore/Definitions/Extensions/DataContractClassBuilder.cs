using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class DataContractClassBuilder
{
    public static DataContractClassDefinition GetDataContractClassDefinition(this EntityFrameworkCoreProject project, ITable table, bool isDomainDrivenDesign)
    {
        var definition = new DataContractClassDefinition
        {
            Namespaces =
            {
                "System"
            },
            Namespace = isDomainDrivenDesign ? project.GetDomainQueryModelsNamespace() : project.GetDataLayerDataContractsNamespace(),
            AccessModifier = AccessModifier.Public,
            Name = project.GetDataContractName(table),
            DbObject = table
        };

        foreach (var column in table.Columns)
        {
            definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropName(table, column))
            {
                IsAutomatic = true
            });
        }

        foreach (var foreignKey in table.ForeignKeys)
        {
            var foreignTable = project.Database.FindTable(foreignKey.References);

            if (foreignTable == null)
                continue;

            var foreignKeyAlias = NamingConvention.GetCamelCase(project.GetEntityName(foreignTable));

            foreach (var column in foreignTable?.GetColumnsWithNoPrimaryKey())
            {
                var col = (Column)column;

                var propName = project.GetPropName(foreignTable, col);

                var target = string.Format("{0}{1}", project.GetEntityName(foreignTable), propName);

                if (definition.Properties.Count(item => item.Name == propName) == 0)
                {
                    definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(col), target)
                    {
                        IsAutomatic = true
                    });
                }
            }
        }

        definition.SimplifyDataTypes();

        return definition;
    }
}
