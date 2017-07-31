using System;
using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class TableExtensions
    {
        public static String GetFullColumnName(this ITable table, Column column)
            => String.Join(".", new String[] { table.Schema, table.Name, column.Name });

        public static Boolean HasDefaultSchema(this IDbObject table)
            => String.IsNullOrEmpty(table.Schema) || String.Compare(table.Schema, "dbo", true) == 0;
    }
}
