using System.Collections.Generic;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class ConstraintExtensions
    {
        public static PropertyDefinition GetParentNavigationProperty(this ForeignKey foreignKey, EntityFrameworkCoreProject project, ITable table)
        {
            var propertyType = table.GetSingularName();

            return new PropertyDefinition(propertyType, string.Format("{0}Fk", propertyType))
            {
                IsVirtual = project.Settings.DeclareNavigationPropertiesAsVirtual,
                Attributes = project.Settings.UseDataAnnotations ? new List<MetadataAttribute> { new MetadataAttribute("ForeignKey", string.Format("\"{0}\"", string.Join(",", foreignKey.Key))) } : null
            };
        }
    }
}
