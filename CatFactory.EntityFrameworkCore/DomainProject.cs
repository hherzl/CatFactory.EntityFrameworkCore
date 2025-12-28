using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore;

public class DomainProject : CSharpProject<DomainProjectSettings>
{
    public DomainProject(CleanArchitectureSolution solution)
    {
        Solution = solution;
    }

    public CleanArchitectureSolution Solution { get; set; }

    public override void Scaffold(IObjectDefinition objectDefinition, string outputDirectory, string subdirectory = "")
    {
        var codeBuilder = default(ICodeBuilder);

        var selection = objectDefinition.DbObject == null ? this.GlobalSelection() : this.GetSelection(objectDefinition.DbObject);

        if (objectDefinition is CSharpClassDefinition)
        {
            codeBuilder = new CSharpClassBuilder
            {
                OutputDirectory = outputDirectory,
                ForceOverwrite = selection.Settings.ForceOverwrite,
                ObjectDefinition = objectDefinition
            };
        }
        else if (objectDefinition is CSharpInterfaceDefinition)
        {
            codeBuilder = new CSharpInterfaceBuilder
            {
                OutputDirectory = outputDirectory,
                ForceOverwrite = selection.Settings.ForceOverwrite,
                ObjectDefinition = objectDefinition
            };
        }

        OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

        codeBuilder.CreateFile(subdirectory: subdirectory);

        OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
    }
}
