using CatFactory.EntityFrameworkCore.Definitions.Extensions;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class DomainProjectExtensions
{
    #region [ Directory naming ]

    public static string GetDomainDirectory(this DomainProject project)
        => Path.Combine(project.OutputDirectory);

    public static string GetDomainEntitiesDirectory(this DomainProject project)
        => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.Solution.Namespaces.Domain}", project.Solution.Namespaces.Entities);

    public static string GetDomainExceptionsDirectory(this DomainProject project)
        => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.Solution.Namespaces.Domain}", project.Solution.Namespaces.Exceptions);

    #endregion

    #region [ Namespaces ]

    public static string GetDomainEntitiesNs(this DomainProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.Solution.Namespaces.Domain, project.Solution.Namespaces.Entities);

    public static string GetDomainExceptionNs(this DomainProject project)
        => project.CodeNamingConvention.GetNamespace(project.Name, project.Solution.Namespaces.Domain, project.Solution.Namespaces.Exceptions);

    #endregion

    #region [ Naming convention ]

    public static string GetEntityName(this DomainProject project, IDbObject dbObject)
        => project.CodeNamingConvention.GetClassName(dbObject.Name);

    public static string GetParamName(this DomainProject project, Column column)
        => project.CodeNamingConvention.GetParameterName(column.Name);

    public static string GetNavigationPropName(this DomainProject project, IDbObject dbObject)
        => string.Format("{0}List", project.CodeNamingConvention.GetClassName(dbObject.Name));

    #endregion

    public static DomainProject Scaffold(this DomainProject project, Action<DomainProjectSettings> action = null)
    {
        ScaffoldEntityInterface(project);
        ScaffoldEntities(project);
        ScaffoldExceptions(project);

        return project;
    }

    internal static void ScaffoldEntityInterface(DomainProject project)
    {
        project.Scaffold(project.GetEntityInterfaceDef(), project.GetDomainEntitiesDirectory());

        if (project.GlobalSelection().Settings.AuditEntity != null)
            project.Scaffold(project.GetAuditEntityInterfaceDefinition(), project.GetDomainEntitiesDirectory());
    }

    internal static DomainProject ScaffoldEntities(this DomainProject project)
    {
        foreach (var table in project.Database.Tables)
        {
            var definition = project.GetEntityClassDef(table);

            var selection = project.GetSelection(table);
            if (selection.Settings.UseDataAnnotations)
                definition.AddDataAnnotations(table, project);

            project.Scaffold(definition, project.GetDomainEntitiesDirectory());
        }

        foreach (var view in project.Database.Views)
        {
            var definition = project.GetEntityClassDef(view);

            var selection = project.GetSelection(view);
            if (selection.Settings.UseDataAnnotations)
                definition.AddDataAnnotations(view, project);

            project.Scaffold(definition, project.GetDomainEntitiesDirectory());
        }

        return project;
    }

    internal static void ScaffoldExceptions(this DomainProject project)
    {
        project.Scaffold(project.GetExceptionClassDef(), project.GetDomainExceptionsDirectory());
    }
}
