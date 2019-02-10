using System.Linq;
using CatFactory.NetCore;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class DataContractClassBuilder
    {
        public static DataContractClassDefinition GetDataContractClassDefinition(this EntityFrameworkCoreProject project, ITable table)
        {
            var definition = new DataContractClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetDataLayerDataContractsNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = project.GetDataContractName(table)
            };

            foreach (var column in table.Columns)
            {
                definition.Properties.Add(new PropertyDefinition(project.Database.ResolveDatabaseType(column), column.GetPropertyName()) { AccessModifier = AccessModifier.Public });
            }

            foreach (var foreignKey in table.ForeignKeys)
            {
                var foreignTable = project.Database.FindTable(foreignKey.References);

                if (foreignTable == null)
                    continue;

                var foreignKeyAlias = NamingConvention.GetCamelCase(project.GetEntityName(foreignTable));

                foreach (var column in foreignTable?.GetColumnsWithNoPrimaryKey())
                {
                    var target = string.Format("{0}{1}", project.GetEntityName(foreignTable), column.GetPropertyName());

                    if (definition.Properties.Count(item => item.Name == column.GetPropertyName()) == 0)
                        definition.Properties.Add(new PropertyDefinition(project.Database.ResolveDatabaseType(column), target) { AccessModifier = AccessModifier.Public });
                }
            }

            return definition;
        }
    }
}
