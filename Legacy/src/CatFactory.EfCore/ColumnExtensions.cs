using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class ColumnExtensions
    {
        public static ICodeNamingConvention namingConvention;

        static ColumnExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetPropertyName(this Column column)
        {
            var name = column.Name;

            foreach (var item in DotNetNamingConvention.invalidChars)
            {
                name = name.Replace(item, '_');
            }

            return namingConvention.GetPropertyName(name);
        }

        public static string GetParameterName(this Column column)
            => namingConvention.GetParameterName(column.Name);

        public static bool HasSameNameEnclosingType(this Column column, ITable table)
            => column.Name == table.Name;

        public static bool HasSameNameEnclosingType(this Column column, IView view)
            => column.Name == view.Name;

        public static string GetNameForEnclosing(this Column column)
            => string.Format("{0}1", column.Name);
    }
}
