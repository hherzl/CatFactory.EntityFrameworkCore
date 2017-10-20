using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class DbObjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static DbObjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetSingularName(this IDbObject dbObject)
            => NamingService.GetSingularName(dbObject.GetEntityName());

        public static string GetPluralName(this IDbObject dbObject)
            => NamingService.GetPluralName(dbObject.GetEntityName());

        public static string GetEntityName(this IDbObject dbObject)
            => string.Format("{0}", namingConvention.GetClassName(dbObject.Name));

        public static string GetDataContractName(this IDbObject dbObject)
            => string.Format("{0}Dto", namingConvention.GetClassName(dbObject.Name));

        public static string GetMapName(this IDbObject dbObject)
            => namingConvention.GetClassName(string.Format("{0}Map", dbObject.GetSingularName()));

        public static string GetDbContextName(this Database db)
            => namingConvention.GetClassName(string.Format("{0}DbContext", db.Name));

        public static string GetDbEntityMapperName(this Database db)
            => namingConvention.GetClassName(string.Format("{0}EntityMapper", db.Name));

        public static string GetGetAllRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}", dbObject.GetPluralName());

        public static string GetGetByUniqueRepositoryMethodName(this IDbObject dbObject, Unique unique)
            => string.Format("Get{0}By{1}Async", dbObject.GetSingularName(), string.Join("And", unique.Key.Select(item => namingConvention.GetPropertyName(item))));

        public static string GetGetRepositoryMethodName(this IDbObject dbObject)
            => string.Format("Get{0}Async", dbObject.GetSingularName());

        public static string GetAddRepositoryMethodName(this ITable dbObject)
            => string.Format("Add{0}Async", dbObject.GetSingularName());

        public static string GetUpdateRepositoryMethodName(this ITable dbObject)
            => string.Format("Update{0}Async", dbObject.GetSingularName());

        public static string GetRemoveRepositoryMethodName(this ITable dbObject)
            => string.Format("Remove{0}Async", dbObject.GetSingularName());

        public static bool HasDefaultSchema(this IDbObject table)
            => string.IsNullOrEmpty(table.Schema) || string.Compare(table.Schema, "dbo", true) == 0;

        public static string GetFullColumnName(this ITable table, Column column)
            => string.Join(".", new string[] { table.Schema, table.Name, column.Name });

        public static bool IsPrimaryKeyGuid(this ITable table)
            => table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1 && table.Columns[0].Type == "uniqueidentifier" ? true : false;
    }
}
