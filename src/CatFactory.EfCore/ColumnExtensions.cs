using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class ColumnExtensions
    {
        public static bool HasSameNameEnclosingType(this Column column, ITable table)
            => column.Name == table.Name;

        public static bool HasSameNameEnclosingType(this Column column, IView view)
            => column.Name == view.Name;

        public static string GetNameForEnclosing(this Column column)
            => string.Format("{0}1", column.Name);
    }
}
