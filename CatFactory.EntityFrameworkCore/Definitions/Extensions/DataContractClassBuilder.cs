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
                Name = project.GetDataContractName(table),
                DbObject = table
            };

            foreach (var column in table.Columns)
            {
                definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropertyName(table, column))
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
                    var propertyName = project.GetPropertyName(foreignTable, column);

                    var target = string.Format("{0}{1}", project.GetEntityName(foreignTable), propertyName);

                    if (definition.Properties.Count(item => item.Name == propertyName) == 0)
                        definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), target)
                        {
                            IsAutomatic = true
                        });
                }
            }

            return definition;
        }

        public static DataContractClassDefinition GetDataContractClassDefinition(this EntityFrameworkCoreProject project, IView view)
        {
            var definition = new DataContractClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetDataLayerDataContractsNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = project.GetDataContractName(view),
                DbObject = view
            };

            foreach (var column in view.Columns)
            {
                definition.Properties.Add(new PropertyDefinition(AccessModifier.Public, project.Database.ResolveDatabaseType(column), project.GetPropertyName(view, column))
                {
                    IsAutomatic = true
                });
            }

            return definition;
        }
    }
}
