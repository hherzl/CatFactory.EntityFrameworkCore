using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.EntityFrameworkCore
{
    public static class NamingExtensions
    {
        public static ICodeNamingConvention NamingConvention;
        public static INamingService NamingService;

        static NamingExtensions()
        {
            NamingConvention = new DotNetNamingConvention();
            NamingService = new NamingService();
        }

        public static string GetParameterName(this Column column)
            => NamingConvention.GetParameterName(column.Name);

        public static string GetNavigationPropertyName(this IDbObject dbObject)
            => string.Format("{0}List", NamingConvention.GetClassName(dbObject.Name));

        public static string GetPluralName(this IDbObject dbObject)
            => NamingService.Pluralize(dbObject.GetEntityName());

        public static string GetEntityName(this IDbObject dbObject)
            => NamingConvention.GetClassName(dbObject.Name);

        public static string GetFullEntityName(this IDbObject dbObject)
            => NamingConvention.GetClassName(dbObject.Name);

        public static string GetDataContractName(this IDbObject dbObject)
            => string.Format("{0}Dto", NamingConvention.GetClassName(dbObject.Name));

        public static string GetDbContextName(this Database database)
            => NamingConvention.GetClassName(string.Format("{0}DbContext", database.Name));

        public static string GetDbSetPropertyName(this IDbObject dbObject)
            => NamingService.Pluralize(dbObject.GetEntityName());

        public static string GetEntityConfigurationName(this IDbObject dbObject)
            => NamingConvention.GetClassName(string.Format("{0}Configuration", dbObject.GetEntityName()));

        public static string GetGetAllRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}", dbObject.GetPluralName());

        public static string GetGetByUniqueRepositoryMethodName(this IDbObject dbObject, Unique unique)
            => string.Format("Get{0}By{1}Async", dbObject.GetEntityName(), string.Join("And", unique.Key.Select(item => NamingConvention.GetPropertyName(item))));

        public static string GetGetRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetEntityName());

        public static string GetAddRepositoryMethodName(this ITable table)
            => string.Format("Add{0}Async", table.GetEntityName());

        public static string GetUpdateRepositoryMethodName(this ITable table)
            => string.Format("Update{0}Async", table.GetEntityName());

        public static string GetRemoveRepositoryMethodName(this ITable table)
            => string.Format("Remove{0}Async", table.GetEntityName());

        public static string GetScalarFunctionMethodName(this ScalarFunction scalarFunction)
            => string.Format("{0}{1}", NamingConvention.GetClassName(scalarFunction.Schema), NamingConvention.GetClassName(scalarFunction.Name));
    }
}
