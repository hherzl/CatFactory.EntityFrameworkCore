using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.EntityFrameworkCore
{
    public static partial class EntityFrameworkCoreProjectExtensions
    {
        public static string GetParameterName(this EntityFrameworkCoreProject project, Column column)
            => project.CodeNamingConvention.GetParameterName(column.Name);

        public static string GetNavigationPropertyName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("{0}List", project.CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetEntityName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => project.CodeNamingConvention.GetClassName(dbObject.Name);

        public static string GetEntityResultName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("{0}Result", project.CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetPluralName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => project.NamingService.Pluralize(project.GetEntityName(dbObject));

        public static string GetDbContextName(this EntityFrameworkCoreProject project, Database database)
            => project.CodeNamingConvention.GetClassName(string.Format("{0}DbContext", database.Name));

        public static string GetDbSetPropertyName(this EntityFrameworkCoreProject project, IDbObject dbObject, bool pluralize)
            => pluralize ? project.NamingService.Pluralize(project.GetEntityName(dbObject)) : project.GetEntityName(dbObject);

        public static string GetFullDbSetPropertyName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => project.NamingService.Pluralize(string.Concat(project.CodeNamingConvention.GetNamespace(dbObject.Schema), project.GetEntityName(dbObject)));

        public static string GetEntityConfigurationName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("{0}Configuration", project.GetEntityName(dbObject));

        public static string GetFullEntityConfigurationName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => project.CodeNamingConvention.GetNamespace(project.ProjectNamespaces.Configurations, dbObject.Schema, string.Format("{0}Configuration", project.GetEntityName(dbObject)));

        public static string GetFullEntityName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.ProjectNamespaces.EntityLayer), project.CodeNamingConvention.GetClassName(dbObject.Schema), project.CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetQueryModelName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("{0}QueryModel", project.CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetDataContractName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("{0}Dto", project.CodeNamingConvention.GetClassName(dbObject.Name));

        public static string GetGetAllExtensionMethodName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("Get{0}", project.GetPluralName(dbObject));

        public static string GetGetAllRepositoryMethodName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("Get{0}", project.GetPluralName(dbObject));

        public static string GetGetRepositoryMethodName(this EntityFrameworkCoreProject project, IDbObject dbObject)
            => string.Format("Get{0}Async", project.GetEntityName(dbObject));

        public static string GetGetByUniqueRepositoryMethodName(this EntityFrameworkCoreProject project, ITable table, Unique unique)
            => string.Format("Get{0}By{1}Async", project.GetEntityName(table), string.Join("And", unique.Key.Select(item => project.CodeNamingConvention.GetPropertyName(item))));

        public static string GetAddRepositoryMethodName(this EntityFrameworkCoreProject project, ITable table)
            => string.Format("Add{0}Async", project.GetEntityName(table));

        public static string GetUpdateRepositoryMethodName(this EntityFrameworkCoreProject project, ITable table)
            => string.Format("Update{0}Async", project.GetEntityName(table));

        public static string GetRemoveRepositoryMethodName(this EntityFrameworkCoreProject project, ITable table)
            => string.Format("Remove{0}Async", project.GetEntityName(table));

        public static string GetScalarFunctionMethodName(this EntityFrameworkCoreProject project, ScalarFunction scalarFunction)
            => string.Format("{0}{1}", project.CodeNamingConvention.GetClassName(scalarFunction.Schema), project.CodeNamingConvention.GetClassName(scalarFunction.Name));

        public static bool HasSameEnclosingName(this ITable table)
            => table.Schema == table.Name;

        public static PropertyDefinition GetChildNavigationProperty(this EntityFrameworkCoreProject project, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table, ForeignKey foreignKey)
        {
            var propertyType = string.Format("{0}<{1}>", projectSelection.Settings.NavigationPropertyEnumerableType, project.GetEntityName(table));

            return new PropertyDefinition(propertyType, project.GetNavigationPropertyName(table))
            {
                AccessModifier = AccessModifier.Public,
                IsVirtual = projectSelection.Settings.DeclareNavigationPropertiesAsVirtual
            };
        }
    }
}
