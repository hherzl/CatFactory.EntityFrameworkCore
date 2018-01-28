using System;
using System.Linq;
using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class DatabaseExtensions
    {
        public static string ResolveType(this Database database, Column column)
        {
            var map = database.Mappings.FirstOrDefault(item => item.DatabaseType == column.Type);

            if (map == null || map.ClrType == null)
                return "object";

            return map.AllowClrNullable ? string.Format("{0}?", map.ClrType.Name) : map.ClrType.Name;
        }

        public static bool ColumnIsDecimal(this Database database, Column column)
            => database.Mappings.Where(item => item.DatabaseType == column.Type && item.ClrFullNameType == typeof(decimal).FullName).Count() == 0 ? false : true;

        public static bool ColumnIsDouble(this Database database, Column column)
            => database.Mappings.Where(item => item.DatabaseType == column.Type && item.ClrFullNameType == typeof(double).FullName).Count() == 0 ? false : true;

        public static bool ColumnIsSingle(this Database database, Column column)
            => database.Mappings.Where(item => item.DatabaseType == column.Type && item.ClrFullNameType == typeof(float).FullName).Count() == 0 ? false : true;

        public static bool ColumnIsString(this Database database, Column column)
            => database.Mappings.Where(item => item.DatabaseType == column.Type && item.ClrFullNameType == typeof(string).FullName).Count() == 0 ? false : true;

        public static bool ColumnIsGuid(this Database database, Column column)
            => database.Mappings.Where(item => item.DatabaseType == column.Type && item.ClrFullNameType == typeof(Guid).FullName).Count() == 0 ? false : true;

        public static bool PrimaryKeyIsGuid(this Database database, ITable table)
        {
            if (table.PrimaryKey == null)
                return false;

            var columns = table.GetColumnsFromConstraint(table.PrimaryKey);

            if (columns.Count() == 0)
                return false;

            var column = columns.First();

            return database.Mappings.Where(item => item.DatabaseType == column.Type && item.ClrFullNameType == typeof(Guid).FullName).Count() == 0 ? false : true;
        }
    }
}
