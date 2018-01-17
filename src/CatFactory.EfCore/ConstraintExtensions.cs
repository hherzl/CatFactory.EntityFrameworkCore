using System.Collections.Generic;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class ConstraintExtensions
    {
        public static PropertyDefinition GetParentNavigationProperty(this ForeignKey foreignKey, ITable table, EntityFrameworkCoreProject project)
        {
            var propertyType = table.GetSingularName();

            var selection = project.GetSelection(table);

            return new PropertyDefinition(propertyType, string.Format("{0}Fk", propertyType))
            {
                IsVirtual = selection.Settings.DeclareNavigationPropertiesAsVirtual,
                Attributes = selection.Settings.UseDataAnnotations ? new List<MetadataAttribute> { new MetadataAttribute("ForeignKey", string.Format("\"{0}\"", string.Join(",", foreignKey.Key))) } : null
            };
        }
    }
}
