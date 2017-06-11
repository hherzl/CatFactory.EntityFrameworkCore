using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class ViewModelClassDefinition : CSharpClassDefinition
    {
        public ViewModelClassDefinition(EfCoreProject project, IDbObject dbObject)
        {
            Namespaces.Add("System");
            Namespaces.Add("System.ComponentModel");

            Implements.Add("INotifyPropertyChanged");

            Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));

            Name = dbObject.GetViewModelName();

            Constructors.Add(new ClassConstructorDefinition());

            var resolver = new ClrTypeResolver() as ITypeResolver;

            var columns = default(IEnumerable<Column>);

            var tableCast = dbObject as ITable;

            if (tableCast != null)
            {
                columns = tableCast.Columns;
            }

            var viewCast = dbObject as IView;

            if (viewCast != null)
            {
                columns = viewCast.Columns;
            }

            if (tableCast != null || viewCast != null)
            {
                foreach (var column in columns)
                {
                    this.AddViewModelProperty(resolver.Resolve(column.Type), column.GetPropertyName());
                }
            }

            if (project.Settings.SimplifyDataTypes)
            {
                this.SimplifyDataTypes();
            }
        }
    }
}
