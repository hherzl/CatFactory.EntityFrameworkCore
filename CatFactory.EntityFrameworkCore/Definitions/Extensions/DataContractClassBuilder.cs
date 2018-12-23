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
            var classDefinition = new DataContractClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetDataLayerDataContractsNamespace(),
                Name = table.GetDataContractName()
            };

            foreach (var column in table.Columns)
            {
                classDefinition.Properties.Add(new PropertyDefinition(project.Database.ResolveDatabaseType(column), column.GetPropertyName()));
            }

            foreach (var foreignKey in table.ForeignKeys)
            {
                var foreignTable = project.Database.FindTable(foreignKey.References);

                if (foreignTable == null)
                    continue;

                var foreignKeyAlias = NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                foreach (var column in foreignTable?.GetColumnsWithNoPrimaryKey())
                {
                    var target = string.Format("{0}{1}", foreignTable.GetEntityName(), column.GetPropertyName());

                    if (classDefinition.Properties.Count(item => item.Name == column.GetPropertyName()) == 0)
                        classDefinition.Properties.Add(new PropertyDefinition(project.Database.ResolveDatabaseType(column), target));
                }
            }

            return classDefinition;
        }
    }
}
