using CatFactory.CodeFactory.Scaffolding;

namespace CatFactory.EntityFrameworkCore;

public static class CleanArchitectureProjectSettingsProjectSelectionExtensions
{
    public static ProjectSelection<DomainProjectSettings> GetByPattern(this IEnumerable<ProjectSelection<DomainProjectSettings>> sequence, string pattern)
        => sequence.FirstOrDefault(item => item.Pattern == pattern);

    public static ProjectSelection<DomainProjectSettings> GetGlobal(this IEnumerable<ProjectSelection<DomainProjectSettings>> sequence)
        => sequence.FirstOrDefault(item => item.IsGlobal);

    public static ProjectSelection<ApplicationProjectSettings> GetByPattern(this IEnumerable<ProjectSelection<ApplicationProjectSettings>> sequence, string pattern)
        => sequence.FirstOrDefault(item => item.Pattern == pattern);

    public static ProjectSelection<ApplicationProjectSettings> GetGlobal(this IEnumerable<ProjectSelection<ApplicationProjectSettings>> sequence)
        => sequence.FirstOrDefault(item => item.IsGlobal);

    public static ProjectSelection<InfrastructureProjectSettings> GetByPattern(this IEnumerable<ProjectSelection<InfrastructureProjectSettings>> sequence, string pattern)
        => sequence.FirstOrDefault(item => item.Pattern == pattern);

    public static ProjectSelection<InfrastructureProjectSettings> GetGlobal(this IEnumerable<ProjectSelection<InfrastructureProjectSettings>> sequence)
        => sequence.FirstOrDefault(item => item.IsGlobal);
}
