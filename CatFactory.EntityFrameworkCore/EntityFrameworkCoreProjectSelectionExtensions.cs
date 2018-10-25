using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class EntityFrameworkCoreProjectSelectionExtensions
    {
        public static ProjectSelection<EntityFrameworkCoreProjectSettings> GetSelection(this EntityFrameworkCoreProject project, ITable table)
        {
            // Sales.Order
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == table.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", table.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // *.Order
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", table.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
        }

        public static ProjectSelection<EntityFrameworkCoreProjectSettings> GetSelection(this EntityFrameworkCoreProject project, IView view)
        {
            // Sales.Order
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == view.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", view.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // *.Order
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", view.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
        }
    }
}
