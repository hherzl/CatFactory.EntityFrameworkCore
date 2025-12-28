using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.EntityFrameworkCore;

public class ApplicationSettings : IProjectSettings
{
    public bool UseDataAnnotationsForRequestValidations { get; set; }
    public bool UseRecordsForModels { get; set; }
    public bool UseCQRS { get; set; }
    public bool UseQuerySpecifications { get; set; }

    public ValidationResult Validate()
    {
        return new();
    }
}
