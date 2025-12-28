using CatFactory.CodeFactory.Scaffolding;

namespace CatFactory.EntityFrameworkCore;

public static class CleanArchitectureProjectSettingsProjectSelectionExtensions
{
    public static ProjectSelection<DomainProjectSettings> GetByPattern(this IEnumerable<ProjectSelection<DomainProjectSettings>> sequence, string pattern)
    {
        return sequence.FirstOrDefault(item => item.Pattern == pattern);
    }

    public static ProjectSelection<DomainProjectSettings> GetGlobal(this IEnumerable<ProjectSelection<DomainProjectSettings>> sequence)
    {
        return sequence.FirstOrDefault(item => item.IsGlobal);
    }

    public static ProjectSelection<ApplicationProjectSettings> GetByPattern(this IEnumerable<ProjectSelection<ApplicationProjectSettings>> sequence, string pattern)
    {
        return sequence.FirstOrDefault(item => item.Pattern == pattern);
    }

    public static ProjectSelection<ApplicationProjectSettings> GetGlobal(this IEnumerable<ProjectSelection<ApplicationProjectSettings>> sequence)
    {
        return sequence.FirstOrDefault(item => item.IsGlobal);
    }
}
