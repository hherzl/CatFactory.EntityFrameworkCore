using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class DomainProjectSelectionExtensions
{
    public static ProjectSelection<DomainProjectSettings> GetSelection(this DomainProject project, IDbObject dbObject)
    {
        // Sales.OrderHeader
        var selectionForFullName = project.Selections.GetByPattern(dbObject.FullName);
        if (selectionForFullName != null)
            return selectionForFullName;

        // Sales.*
        var selectionForSchema = project.Selections.GetByPattern($"{dbObject.Schema}.*");
        if (selectionForSchema != null)
            return selectionForSchema;

        // *.OrderHeader
        var selectionForName = project.Selections.GetByPattern($"*.{dbObject.Name}");
        if (selectionForName != null)
            return selectionForName;

        return project.GlobalSelection();
    }

    public static DomainProject GlobalSelection(this DomainProject project, Action<DomainProjectSettings> action = null)
    {
        var settings = new DomainProjectSettings();

        var selection = project.Selections.GetGlobal();
        if (selection == null)
        {
            selection = new ProjectSelection<DomainProjectSettings>
            {
                Pattern = ProjectSelection<DomainProjectSettings>.GlobalPattern,
                Settings = settings
            };

            project.Selections.Add(selection);
        }
        else
        {
            settings = selection.Settings;
        }

        action?.Invoke(settings);

        return project;
    }

    public static ProjectSelection<DomainProjectSettings> GlobalSelection(this DomainProject project)
        => project.Selections.FirstOrDefault(item => item.IsGlobal);

    public static DomainProject Selection(this DomainProject project, string pattern, Action<DomainProjectSettings> action = null)
    {
        var selection = project.Selections.GetByPattern(pattern);
        if (selection == null)
        {
            var globalSettings = project.GlobalSelection().Settings;

            selection = new ProjectSelection<DomainProjectSettings>
            {
                Pattern = pattern,
                Settings = new DomainProjectSettings
                {
                    ForceOverwrite = globalSettings.ForceOverwrite
                }
            };

            project.Selections.Add(selection);
        }

        action?.Invoke(selection.Settings);

        return project;
    }
}
