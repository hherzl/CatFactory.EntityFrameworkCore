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
            namingConvention = new DotNetNamingConvention() as ICodeNamingConvention;
        }

        public static String GetEntityLayerNamespace(this Project project)
            => namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, (project as EfCoreProject).Namespaces.EntityLayer));

        public static String GetEntityLayerNamespace(this EfCoreProject project, String ns)
            => String.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : String.Join(".", project.Name, project.Namespaces.EntityLayer, ns);

        public static String GetDataLayerNamespace(this EfCoreProject project)
            => namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, project.Namespaces.DataLayer));

        public static String GetDataLayerMappingNamespace(this EfCoreProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.Mapping));

        public static String GetDataLayerContractsNamespace(this EfCoreProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.Contracts));

        public static String GetDataLayerDataContractsNamespace(this EfCoreProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.DataContracts));

        public static String GetDataLayerRepositoriesNamespace(this EfCoreProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.Repositories));

        public static String GetBusinessLayerNamespace(this EfCoreProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.BusinessLayer));

        public static String GetBusinessLayerContractsNamespace(this EfCoreProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.BusinessLayer, project.Namespaces.Contracts));

        public static String GetBusinessLayerResponsesNamespace(this EfCoreProject project)
            => namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.BusinessLayer, project.Namespaces.Responses));

        public static String GetEntityLayerDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.EntityLayer);

        public static String GetDataLayerDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer);

        public static String GetDataLayerMappingDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Mapping);

        public static String GetDataLayerContractsDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Contracts);

        public static String GetDataLayerDataContractsDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.DataContracts);

        public static String GetDataLayerRepositoriesDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.DataLayer, project.Namespaces.Repositories);

        public static String GetBusinessLayerDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.BusinessLayer);

        public static String GetBusinessLayerContractsDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.BusinessLayer, project.Namespaces.Contracts);

        public static String GetBusinessLayerResponsesDirectory(this EfCoreProject project)
            => Path.Combine(project.OutputDirectory, project.Namespaces.BusinessLayer, project.Namespaces.Responses);

        public static PropertyDefinition GetChildNavigationProperty(this EfCoreProject project, Table table, ForeignKey fk)
        {
            var propertyType = String.Format("{0}<{1}>", project.Settings.NavigationPropertyEnumerableType, table.GetSingularName());
            var propertyName = table.GetPluralName();

            return new PropertyDefinition(propertyType, propertyName)
            {
                IsVirtual = project.Settings.DeclareNavigationPropertiesAsVirtual,
                Attributes = project.Settings.UseDataAnnotations ? new List<MetadataAttribute>() { new MetadataAttribute("ForeignKey", String.Format("\"{0}\"", String.Join(",", fk.Key))) } : null
            };
        }
    }
}
