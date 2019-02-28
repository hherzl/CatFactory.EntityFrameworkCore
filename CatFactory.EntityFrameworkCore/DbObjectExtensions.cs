using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class DbObjectExtensions
    {
        public static string GetFullName(this Database db, IDbObject dbObject)
            => db.NamingConvention.GetObjectName(dbObject.Schema, dbObject.Name);

        public static string GetFullColumnName(this ITable table, Column column)
            => string.Join(".", new string[] { table.Schema, table.Name, column.Name });
    }
}
