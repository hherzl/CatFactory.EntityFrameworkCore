using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.EntityFrameworkCore;

public class InfrastructureSettings : IProjectSettings
{
    public ValidationResult Validate()
    {
        return new();
    }
}
