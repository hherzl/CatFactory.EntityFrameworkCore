using System.Collections.ObjectModel;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.EntityFrameworkCore;

public class DomainProjectSettings : IProjectSettings
{
    public DomainProjectSettings()
    {
        SimplifyDataTypes = true;
        EntityInterfaceName = "IEntity";
        UseAutomaticPropertiesForEntities = true;
        BackingFields = [];
        ScaffoldExceptions = true;
    }

    public bool ForceOverwrite { get; set; }

    public bool SimplifyDataTypes { get; set; }
    public bool UseDataAnnotations { get; set; }

    public string EntityInterfaceName { get; set; }
    public bool HasEntityInterfaceName
        => !string.IsNullOrEmpty(EntityInterfaceName);

    public bool UseAutomaticPropertiesForEntities { get; set; }
    public Collection<string> BackingFields { get; set; }

    public AuditEntity AuditEntity { get; set; }

    public bool DeclareNavigationProperties { get; set; }
    public bool DeclareNavigationPropertiesAsVirtual { get; set; }
    public string NavigationPropertyEnumerableNamespace { get; set; }
    public string NavigationPropertyEnumerableType { get; set; }

    public string ConcurrencyToken { get; set; }

    public bool ScaffoldExceptions { get; set; }
    public bool ScaffoldNotificationsForEntities { get; set; }

    public ValidationResult Validate()
    {
        return new();
    }
}
