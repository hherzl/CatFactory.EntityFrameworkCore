using System.Collections.Generic;
using System.Linq;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class ConstraintExtensions
    {
        public static PropertyDefinition GetParentNavigationProperty(this ForeignKey foreignKey, ITable table, EntityFrameworkCoreProject project)
        {
            var propertyType = "";

            if (table.HasSameEnclosingName())
            {
                propertyType = string.Join(".", (new string[]
                {
                    project.ProjectNamespaces.EntityLayer,
                    project.Database.HasDefaultSchema(table) ? string.Empty : table.Schema, project.GetEntityName(table)
                }).Where(item => !string.IsNullOrEmpty(item)));
            }
            else
            {
                propertyType = project.GetEntityName(table);
            }

            var selection = project.GetSelection(table);

            return new PropertyDefinition(propertyType, $"{project.GetEntityName(table)}Fk")
            {
                AccessModifier = AccessModifier.Public,
                IsVirtual = selection.Settings.DeclareNavigationPropertiesAsVirtual,
                Attributes = selection.Settings.UseDataAnnotations ? new List<MetadataAttribute>
                {
                    new MetadataAttribute("ForeignKey", $"\"{string.Join(",", foreignKey.Key)}\"")
                } : null
            };
        }
    }
}
