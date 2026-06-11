using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class InfrastructureProjectSelectionExtensions
{
    public static ProjectSelection<InfrastructureProjectSettings> GetSelection(this InfrastructureProject project, IDbObject dbObject)
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

    public static InfrastructureProject GlobalSelection(this InfrastructureProject project, Action<InfrastructureProjectSettings> action = null)
    {
        var settings = new InfrastructureProjectSettings();

        var selection = project.Selections.GetGlobal();
        if (selection == null)
        {
            selection = new ProjectSelection<InfrastructureProjectSettings>
            {
                Pattern = ProjectSelection<InfrastructureProjectSettings>.GlobalPattern,
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

    public static ProjectSelection<InfrastructureProjectSettings> GlobalSelection(this InfrastructureProject project)
        => project.Selections.FirstOrDefault(item => item.IsGlobal);

    public static InfrastructureProject Selection(this InfrastructureProject project, string pattern, Action<InfrastructureProjectSettings> action = null)
    {
        var selection = project.Selections.GetByPattern(pattern);
        if (selection == null)
        {
            var globalSettings = project.GlobalSelection().Settings;

            selection = new ProjectSelection<InfrastructureProjectSettings>
            {
                Pattern = pattern,
                Settings = new InfrastructureProjectSettings
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
