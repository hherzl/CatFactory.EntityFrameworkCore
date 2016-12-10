using System;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class NamingConventionExtensions
    {
        private static INamingConvention namingConvention;

        static NamingConventionExtensions()
        {
            namingConvention = new DotNetNamingConvention() as INamingConvention;
        }

        public static String GetSingularName(this IDbObject dbObject)
        {
            return NamingService.GetSingularName(dbObject.GetEntityName());
        }

        public static String GetSingularName(this DbObject dbObject)
        {
            return NamingService.GetSingularName(dbObject.GetEntityName());
        }

        public static String GetPluralName(this DbObject dbObject)
        {
            return NamingService.GetPluralName(dbObject.GetEntityName());
        }

        public static String GetEntityName(this IDbObject dbObject)
        {
            return String.Format("{0}", namingConvention.GetClassName(dbObject.Name));
        }

        public static String GetEntityName(this DbObject dbObject)
        {
            return String.Format("{0}", namingConvention.GetClassName(dbObject.Name));
        }

        public static String GetMapName(this IDbObject dbObject)
        {
            return namingConvention.GetClassName(String.Format("{0}Map", dbObject.Name));
        }

        public static String GetInterfaceRepositoryName(this ProjectFeature projectFeature)
        {
            return namingConvention.GetInterfaceName(String.Format("{0}Repository", projectFeature.Name));
        }

        public static String GetClassRepositoryName(this ProjectFeature projectFeature)
        {
            return namingConvention.GetClassName(String.Format("{0}Repository", projectFeature.Name));
        }

        public static String GetDbContextName(this Database db)
        {
            return namingConvention.GetClassName(String.Format("{0}DbContext", db.Name));
        }

        public static String GetDbEntityMapperName(this Database db)
        {
            return namingConvention.GetClassName(String.Format("{0}EntityMapper", db.Name));
        }

        public static String GetEntityLayerNamespace(this EfCoreProject project)
        {
            return namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, project.Namespaces.EntityLayer));
        }

        public static String GetDataLayerNamespace(this EfCoreProject project)
        {
            return namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, project.Namespaces.DataLayer));
        }

        public static String GetDataLayerMappingNamespace(this EfCoreProject project)
        {
            return namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.Mapping));
        }

        public static String GetDataLayerContractsNamespace(this EfCoreProject project)
        {
            return namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.Contracts));
        }

        public static String GetDataLayerRepositoriesNamespace(this EfCoreProject project)
        {
            return namingConvention.GetClassName(String.Join(".", project.Name, project.Namespaces.DataLayer, project.Namespaces.Repositories));
        }
    }
}
