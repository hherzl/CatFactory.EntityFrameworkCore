using System;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class NamingConventionExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static NamingConventionExtensions()
        {
            namingConvention = new DotNetNamingConvention() as ICodeNamingConvention;
        }

        public static String GetSingularName(this IDbObject dbObject)
            => NamingService.GetSingularName(dbObject.GetEntityName());

        public static String GetSingularName(this DbObject dbObject)
            => NamingService.GetSingularName(dbObject.GetEntityName());

        public static String GetPluralName(this DbObject dbObject)
            => NamingService.GetPluralName(dbObject.GetEntityName());

        public static String GetPluralName(this IDbObject dbObject)
            => NamingService.GetPluralName(dbObject.GetEntityName());

        public static String GetEntityName(this IDbObject dbObject)
            => String.Format("{0}", namingConvention.GetClassName(dbObject.Name));

        public static String GetEntityName(this DbObject dbObject)
            => String.Format("{0}", namingConvention.GetClassName(dbObject.Name));

        public static String GetViewModelName(this IDbObject dbObject)
            => String.Format("{0}ViewModel", namingConvention.GetClassName(dbObject.GetSingularName()));

        public static String GetDataContractName(this IDbObject dbObject)
            => String.Format("{0}Dto", namingConvention.GetClassName(dbObject.Name));

        public static String GetMapName(this IDbObject dbObject)
            => namingConvention.GetClassName(String.Format("{0}Map", dbObject.GetSingularName()));

        public static String GetInterfaceRepositoryName(this ProjectFeature projectFeature)
            => namingConvention.GetInterfaceName(String.Format("{0}Repository", projectFeature.Name));

        public static String GetClassRepositoryName(this ProjectFeature projectFeature)
            => namingConvention.GetClassName(String.Format("{0}Repository", projectFeature.Name));

        public static String GetBusinessInterfaceName(this ProjectFeature projectFeature)
            => namingConvention.GetInterfaceName(String.Format("{0}BusinessObject", projectFeature.Name));

        public static String GetBusinessClassName(this ProjectFeature projectFeature)
            => namingConvention.GetClassName(String.Format("{0}BusinessObject", projectFeature.Name));

        public static String GetDbContextName(this Database db)
            => namingConvention.GetClassName(String.Format("{0}DbContext", db.Name));

        public static String GetDbEntityMapperName(this Database db)
            => namingConvention.GetClassName(String.Format("{0}EntityMapper", db.Name));

        public static String GetEntityLayerNamespace(this Project project)
            => namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, (project as EfCoreProject).Namespaces.EntityLayer));

        public static String GetEntityLayerNamespace(this EfCoreProject project, String ns)
            => String.Join(".", project.Name, project.Namespaces.EntityLayer, ns);

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

        public static String GetGetAllMethodName(this IDbObject dbObject)
            => String.Format("Get{0}", dbObject.GetPluralName());

        public static String GetGetByUniqueMethodName(this IDbObject dbObject, Unique unique, ICodeNamingConvention namingConvention)
            => String.Format("Get{0}By{1}Async", dbObject.GetSingularName(), String.Join("And", unique.Key.Select(item => namingConvention.GetPropertyName(item))));

        public static String GetGetMethodName(this IDbObject dbObject)
            => String.Format("Get{0}Async", dbObject.GetSingularName());

        public static String GetAddMethodName(this IDbObject dbObject)
            => String.Format("Add{0}Async", dbObject.GetSingularName());

        public static String GetUpdateMethodName(this IDbObject dbObject)
            => String.Format("Update{0}Async", dbObject.GetSingularName());

        public static String GetRemoveMethodName(this IDbObject dbObject)
            => String.Format("Remove{0}Async", dbObject.GetSingularName());
    }
}
