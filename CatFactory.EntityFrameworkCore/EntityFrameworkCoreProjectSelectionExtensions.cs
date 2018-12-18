using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore
{
    public static class EntityFrameworkCoreProjectSelectionExtensions
    {
        public static ProjectSelection<EntityFrameworkCoreProjectSettings> GetSelection(this EntityFrameworkCoreProject project, IDbObject dbObj)
        {
            // Sales.OrderHeader
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == dbObj.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", dbObj.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // *.OrderHeader
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", dbObj.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
        }
    }
}
