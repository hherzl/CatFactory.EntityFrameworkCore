using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.EntityFrameworkCore;

public static class SharedKernelExtensions
{
    public static EntityFrameworkCoreProject ScaffoldSharedKernel(this EntityFrameworkCoreProject project, Action<SharedKernelSettings> action = null)
    {
        return project;
    }
}

public class SharedKernelSettings : IProjectSettings
{
    public ValidationResult Validate()
    {
        return new();
    }
}
