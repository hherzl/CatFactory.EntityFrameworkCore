using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatFactory.EntityFrameworkCore;

public static class CleanArchitectureSpecExtensions
{
    //public static string GetDomainDirectory2(this DomainProject project)
    //=> Path.Combine(project.OutputDirectory, $"{project.Name}.{project.Namespaces.Domain}");

    //public static string GetDomainEntitiesDirectory(this DomainProject project)
    //    => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.Namespaces.Domain}", project.Namespaces.Entities);

    //public static string GetDomainExceptionsDirectory(this DomainProject project)
    //    => Path.Combine(project.OutputDirectory, $"{project.Name}.{project.Namespaces.Domain}", project.Namespaces.Exceptions);

    //public static string GetDomainExceptionNs(this DomainProject project)
    //    => project.CodeNamingConvention.GetNamespace(project.Name, project.Namespaces.Domain, project.Namespaces.Exceptions);

    //public static string GetDomainDirectory(this DomainProject project)
    //    => Path.Combine(project.OutputDirectory);

    //public static string GetDomainModelsDirectory(this DomainProject project)
    //    => Path.Combine(project.OutputDirectory, project.Namespaces.Models);

    //public static string GetDomainQueryModelsDirectory(this ICleanArchitectureSpec project)
    //    => Path.Combine(project.OutputDirectory, project.Namespaces.QueryModels);

    //public static string GetDomainConfigurationsDirectory(this ICleanArchitectureSpec project)
    //    => Path.Combine(project.OutputDirectory, project.Namespaces.Configurations);

    //public static string GetDomainConfigurationsDirectory(this ICleanArchitectureSpec project, string schema)
    //    => Path.Combine(project.OutputDirectory, project.Namespaces.Configurations, schema);

    //public static string GetDomainModelsNamespace(this ICleanArchitectureSpec project)
    //    => project.CodeNamingConvention.GetNamespace(project.Name, project.Namespaces.Domain, project.Namespaces.Entities);

    //public static string GetDomainModelsNamespace(this ICleanArchitectureSpec project, string ns)
    //    => string.IsNullOrEmpty(ns) ? GetDomainModelsNamespace(project) : project.CodeNamingConvention.GetNamespace(project.Name, project.Namespaces.Domain, project.Namespaces.Entities, ns);

    //public static string GetDomainQueryModelsNamespace(this ICleanArchitectureSpec project)
    //    => project.CodeNamingConvention.GetNamespace(project.Name, project.Namespaces.QueryModels);

    //public static string GetDomainConfigurationsNamespace(this ICleanArchitectureSpec project)
    //    => project.CodeNamingConvention.GetNamespace(project.Name, project.Namespaces.Infrastructure, project.Namespaces.Persistence, project.Namespaces.Configurations);

    //public static string GetDomainConfigurationsNamespace(this ICleanArchitectureSpec project, string ns)
    //    => string.IsNullOrEmpty(ns) ? GetDomainConfigurationsNamespace(project) : project.CodeNamingConvention.GetNamespace(project.Name, project.Namespaces.Configurations, ns);
}
