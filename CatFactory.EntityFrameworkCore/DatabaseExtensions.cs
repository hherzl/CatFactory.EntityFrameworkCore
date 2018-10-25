using System;
using System.Linq;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class DatabaseExtensions
    {
        public static string ResolveType(this Database database, Column column)
        {
            var map = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == column.Type);

            if (map == null || map.GetClrType() == null)
                return "object";

            return map.AllowClrNullable ? string.Format("{0}?", map.GetClrType().Name) : map.GetClrType().Name;
        }

        public static bool PrimaryKeyIsGuid(this Database database, ITable table)
        {
            // todo: Review if this extension method has a definition in core package

            if (table.PrimaryKey == null)
                return false;

            var columns = table.GetColumnsFromConstraint(table.PrimaryKey);

            if (columns.Count() == 0)
                return false;

            var column = columns.First();

            return database.DatabaseTypeMaps.Where(item => item.DatabaseType == column.Type && item.ClrFullNameType == typeof(Guid).FullName).Count() == 0 ? false : true;
        }
    }
}
