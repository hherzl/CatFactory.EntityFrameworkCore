using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.EntityFrameworkCore;

public class InfrastructureProjectSettings : IProjectSettings
{
    public InfrastructureProjectSettings()
    {
        UseApplyConfigurationsFromAssemblyMethod = true;
        PluralizeDbSetPropertyNames = true;
    }

    public bool ForceOverwrite { get; set; }

    public bool SimplifyDataTypes { get; set; }

    public bool UseDataAnnotations { get; set; }

    public string ConcurrencyToken { get; set; }
    public bool HasConcurrencyToken
        => !string.IsNullOrEmpty(ConcurrencyToken);

    public string RowVersion { get; set; }
    public bool HasRowVersion
        => !string.IsNullOrEmpty(RowVersion);

    public bool UseApplyConfigurationsFromAssemblyMethod { get; set; }

    public bool PluralizeDbSetPropertyNames { get; set; }

    public bool AddConfigurationForUniquesInFluentAPI { get; set; }
    public bool AddConfigurationForForeignKeysInFluentAPI { get; set; }
    public bool DeclareNavigationProperties { get; set; }
    public bool AddConfigurationForDefaultsInFluentAPI { get; set; }
    public bool DeclareNavigationPropertiesAsVirtual { get; set; }

    public ValidationResult Validate()
    {
        throw new NotImplementedException();
    }
}
