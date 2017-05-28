using System;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class ConstraintExtensions
    {
        public static PropertyDefinition GetParentNavigationProperty(this ForeignKey foreignKey, EfCoreProject project, ITable table)
        {
            var propertyType = table.GetSingularName();
            var propertyName = String.Format("{0}Fk", propertyType);

            return new PropertyDefinition(propertyType, propertyName)
            {
                IsVirtual = project.Settings.DeclareNavigationPropertiesAsVirtual
            };
        }
    }
}
