using CatFactory.Mapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class DbObjectExtensions
    {
        public static string GetFullColumnName(this ITable table, Column column)
            => string.Join(".", new string[] { table.Schema, table.Name, column.Name });
    }
}
