using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class ApplicationProjectSelectionExtensions
{
    public static ProjectSelection<ApplicationProjectSettings> GetSelection(this ApplicationProject project, IDbObject dbObject)
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

    public static ApplicationProject GlobalSelection(this ApplicationProject project, Action<ApplicationProjectSettings> action = null)
    {
        var settings = new ApplicationProjectSettings();

        var selection = project.Selections.GetGlobal();
        if (selection == null)
        {
            selection = new ProjectSelection<ApplicationProjectSettings>
            {
                Pattern = ProjectSelection<ApplicationProjectSettings>.GlobalPattern,
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

    public static ProjectSelection<ApplicationProjectSettings> GlobalSelection(this ApplicationProject project)
        => project.Selections.FirstOrDefault(item => item.IsGlobal);

    public static ApplicationProject Selection(this ApplicationProject project, string pattern, Action<ApplicationProjectSettings> action = null)
    {
        var selection = project.Selections.GetByPattern(pattern);
        if (selection == null)
        {
            var globalSettings = project.GlobalSelection().Settings;

            selection = new ProjectSelection<ApplicationProjectSettings>
            {
                Pattern = pattern,
                Settings = new ApplicationProjectSettings
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
