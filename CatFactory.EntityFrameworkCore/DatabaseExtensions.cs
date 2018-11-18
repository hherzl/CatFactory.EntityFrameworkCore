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

        public static DatabaseTypeMap ResolveType(this Database database, string type)
            => database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == type);
    }
}
