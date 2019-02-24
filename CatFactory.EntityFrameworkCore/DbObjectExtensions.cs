using System.Linq;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.EntityFrameworkCore
{
    public static class DbObjectExtensions
    {
        public static string GetFullColumnName(this ITable table, Column column)
            => string.Join(".", new string[] { table.Schema, table.Name, column.Name });

        public static bool HasSameNameEnclosingType(this Column column, ITable table)
            => column.Name == table.Name;

        public static bool HasSameNameEnclosingType(this Column column, IView view)
            => column.Name == view.Name;

        public static bool HasSameNameEnclosingType(this Column column, TableFunction tableFunction)
            => column.Name == tableFunction.Name;

        public static string ResolveDatabaseTypeHack(this Database database, string type)
        {
            if (type.Contains("("))
                type = type.Substring(0, type.IndexOf("("));

            var databaseTypeMap = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == type);

            if (databaseTypeMap == null || databaseTypeMap.GetClrType() == null)
                return "object";

            return databaseTypeMap.AllowClrNullable ? string.Format("{0}?", databaseTypeMap.GetClrType().Name) : databaseTypeMap.GetClrType().Name;
        }

        public static string ResolveDatabaseType(this Database database, Parameter parameter)
            => database.ResolveDatabaseTypeHack(parameter.Type);

        public static string GetFullName(this Database database, IDbObject dbObject)
            => database.NamingConvention.GetObjectName(dbObject.Schema, dbObject.Name);

        public static bool HasTypeMappedToClr(this Database database, string name)
        {
            var type = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType.Contains(name));

            if (type == null)
                return false;

            if (!string.IsNullOrEmpty(type.ParentDatabaseType))
                return type.GetParentType(database.DatabaseTypeMaps) == null ? false : true;

            if (type.GetClrType() != null)
                return true;

            return false;
        }

        public static DatabaseTypeMap GetClrMapForType(this Database database, string name)
        {
            var type = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType.Contains(name));

            if (type == null)
                return null;

            if (!string.IsNullOrEmpty(type.ParentDatabaseType))
                return type.GetParentType(database.DatabaseTypeMaps);

            return type.GetClrType() == null ? null : type;
        }
    }
}
