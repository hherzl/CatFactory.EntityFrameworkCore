using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class DbObjectExtensions
    {
        public static bool HasDefaultSchema(this IDbObject table)
            => string.IsNullOrEmpty(table.Schema) || string.Compare(table.Schema, "dbo", true) == 0;

        public static string GetFullColumnName(this ITable table, Column column)
            => string.Join(".", new string[] { table.Schema, table.Name, column.Name });

        public static bool IsPrimaryKeyGuid(this Database database, ITable table)
            => table.PrimaryKey != null && table.PrimaryKey.Key.Count == 1 && database.ColumnIsGuid(table.Columns[0]) ? true : false;
    }
}
