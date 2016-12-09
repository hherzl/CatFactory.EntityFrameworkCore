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

        public static String GetPluralName(this DbObject dbObject)
        {
            // todo: improve the way to pluralize a name

            var entityName = dbObject.GetEntityName();

            if (dbObject.Name.EndsWith("y"))
            {
                return String.Format("{0}ies", entityName.Substring(0, entityName.Length - 2));
            }
            else
            {
                return String.Format("{0}s", entityName);
            }
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

        public static String GetEntityLayerNamespace(this Project project)
        {
            return namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, "EntityLayer"));
        }

        public static String GetDataLayerNamespace(this Project project)
        {
            return namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, "DataLayer"));
        }

        public static String GetDataLayerMappingNamespace(this Project project)
        {
            return namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, "DataLayer.Mapping"));
        }

        public static String GetDataLayerContractsNamespace(this Project project)
        {
            return namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, "DataLayer.Contracts"));
        }

        public static String GetDataLayerRepositoriesNamespace(this Project project)
        {
            return namingConvention.GetClassName(String.Format("{0}.{1}", project.Name, "DataLayer.Repositories"));
        }

        public static String GetDbEntityMapperName(this Database db)
        {
            return namingConvention.GetClassName(String.Format("{0}EntityMapper", db.Name));
        }
    }
}
