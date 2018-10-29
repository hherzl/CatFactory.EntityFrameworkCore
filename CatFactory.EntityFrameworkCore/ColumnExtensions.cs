using System.Text.RegularExpressions;
using CatFactory.CodeFactory;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.EntityFrameworkCore
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
                name = name.Replace(item, '_');

            return namingConvention.GetPropertyName(name);
        }

        public static string GetPropertyName(this Parameter parameter)
        {
            var name = parameter.Name;

            foreach (var item in DotNetNamingConvention.invalidChars)
                name = name.Replace(item, '_');

            return namingConvention.GetPropertyName(name);
        }

        public static string GetParameterName(this Column column)
            => namingConvention.GetParameterName(column.Name);

        public static string GetPropertyNameHack(this ITable table, Column column)
        {
            var propertyName = column.HasSameNameEnclosingType(table) ? column.GetNameForEnclosing() : column.GetPropertyName();

            var regex = new Regex(@"^[0-9]+$");

            if (regex.IsMatch(propertyName))
                propertyName = string.Format("V{0}", propertyName);

            return propertyName;
        }

        public static string GetPropertyNameHack(this IView view, Column column)
        {
            var propertyName = column.HasSameNameEnclosingType(view) ? column.GetNameForEnclosing() : column.GetPropertyName();

            var regex = new Regex(@"^[0-9]+$");

            if (regex.IsMatch(propertyName))
                propertyName = string.Format("V{0}", propertyName);

            return propertyName;
        }

        public static bool HasSameNameEnclosingType(this Column column, ITable table)
            => column.Name == table.Name;

        public static bool HasSameNameEnclosingType(this Column column, IView view)
            => column.Name == view.Name;

        public static string GetNameForEnclosing(this Column column)
            => string.Format("{0}1", column.Name);
    }
}
