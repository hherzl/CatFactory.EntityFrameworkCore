using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.EntityFrameworkCore;

public class ApplicationProjectSettings : IProjectSettings
{
    public ApplicationProjectSettings()
    {
    }

    public bool ForceOverwrite { get; set; }

    public ValidationResult Validate()
    {
        return new();
    }
}
