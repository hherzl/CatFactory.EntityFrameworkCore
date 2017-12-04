using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class ColumnExtensions
    {
        public static string GetClrType(this Column column)
            => new ClrTypeResolver().Resolve(column.Type);

        public static bool IsDecimal(this Column column)
        {
            switch (column.Type)
            {
                case "decimal":
                case "money":
                case "smallmoney":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsDouble(this Column column)
        {
            switch (column.Type)
            {
                case "float":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsSingle(this Column column)
        {
            switch (column.Type)
            {
                case "real":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsString(this Column column)
        {
            switch (column.Type)
            {
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    return true;

                default:
                    return false;
            }
        }
    }
}
