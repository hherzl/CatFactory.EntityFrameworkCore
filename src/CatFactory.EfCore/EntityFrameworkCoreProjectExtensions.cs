using System.Collections.Generic;
using System.IO;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class EntityFrameworkCoreProjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static EntityFrameworkCoreProjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetEntityLayerNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.EntityLayer);

        public static string GetEntityLayerNamespace(this EntityFrameworkCoreProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : string.Join(".", project.Name, project.Namespaces.EntityLayer, ns);

        public static string GetDataLayerNamespace(this EntityFrameworkCoreProject project)
            => string.Format("{0}.{1}", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer);

        public static string GetDataLayerConfigurationsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Configurations);

        public static string GetDataLayerContractsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this EntityFrameworkCoreProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Repositories);

        public static string GetEntityLayerDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.EntityLayer);

        public static string GetDataLayerDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer);

        public static string GetDataLayerConfigurationsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Configurations);

        public static string GetDataLayerContractsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesDirectory(this EntityFrameworkCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Repositories);

        public static PropertyDefinition GetChildNavigationProperty(this EntityFrameworkCoreProject project, ITable table, ForeignKey foreignKey)
        {
            var propertyType = string.Format("{0}<{1}>", project.Settings.NavigationPropertyEnumerableType, table.GetSingularName());

            return new PropertyDefinition(propertyType, table.GetPluralName())
            {
                IsVirtual = project.Settings.DeclareNavigationPropertiesAsVirtual,
                Attributes = project.Settings.UseDataAnnotations ? new List<MetadataAttribute>()
                {
                    new MetadataAttribute("ForeignKey", string.Format("\"{0}\"", string.Join(",", foreignKey.Key)))
                } : null
            };
        }
    }
}
