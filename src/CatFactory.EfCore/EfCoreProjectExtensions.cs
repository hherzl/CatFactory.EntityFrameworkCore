using System;
using System.Collections.Generic;
using System.IO;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class EfCoreProjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static EfCoreProjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetEntityLayerNamespace(this Project project)
            => string.Join(".", namingConvention.GetClassName(project.Name), (project as EfCoreProject).Namespaces.EntityLayer);

        public static string GetEntityLayerNamespace(this Project project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : string.Join(".", project.Name, (project as EfCoreProject).Namespaces.EntityLayer, ns);

        public static string GetDataLayerNamespace(this Project project)
            => string.Format("{0}.{1}", namingConvention.GetClassName(project.Name), (project as EfCoreProject).Namespaces.DataLayer);

        public static string GetDataLayerMappingNamespace(this Project project)
        {
            var efCoreProject = project as EfCoreProject;

            return string.Join(".", namingConvention.GetClassName(project.Name), efCoreProject.Namespaces.DataLayer, efCoreProject.Namespaces.Mapping);
        }

        public static string GetDataLayerContractsNamespace(this EfCoreProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this EfCoreProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this EfCoreProject project)
            => string.Join(".", namingConvention.GetClassName(project.Name), project.Namespaces.DataLayer, project.Namespaces.Repositories);

        public static string GetEntityLayerDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.EntityLayer);

        public static string GetDataLayerDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer);

        public static string GetDataLayerMappingDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Mapping);

        public static string GetDataLayerContractsDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static string GetDataLayerDataContractsDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static string GetDataLayerRepositoriesDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Repositories);

        public static PropertyDefinition GetChildNavigationProperty(this EfCoreProject project, Table table, ForeignKey fk)
        {
            var propertyType = string.Format("{0}<{1}>", project.Settings.NavigationPropertyEnumerableType, table.GetSingularName());
            var propertyName = table.GetPluralName();

            return new PropertyDefinition(propertyType, propertyName)
            {
                IsVirtual = project.Settings.DeclareNavigationPropertiesAsVirtual,
                Attributes = project.Settings.UseDataAnnotations ? new List<MetadataAttribute>() { new MetadataAttribute("ForeignKey", string.Format("\"{0}\"", string.Join(",", fk.Key))) } : null
            };
        }
    }
}
