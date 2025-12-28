namespace CatFactory.EntityFrameworkCore;

public static class CleanArchitectureSolutionExtensions
{
    public static CleanArchitectureSolution ScaffoldDomain(this CleanArchitectureSolution solution)
    {
        solution.Domain.Name = solution.Name;
        solution.Domain.OutputDirectory = solution.OutputDirectory;
        solution.Domain.Database = solution.Database;

        solution.Domain.GlobalSelection(settings => settings.ForceOverwrite = true);

        solution.Domain.BuildFeatures();
        solution.Domain.Scaffold();

        return solution;
    }

    public static CleanArchitectureSolution ScaffoldApplication(this CleanArchitectureSolution solution)
    {
        solution.Application.Name = solution.Name;
        solution.Application.OutputDirectory = solution.OutputDirectory;
        solution.Application.Database = solution.Database;

        solution.Application.GlobalSelection(settings => settings.ForceOverwrite = true);

        solution.Application.BuildFeatures();
        solution.Application.Scaffold();

        return solution;
    }
}
