using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class CommandClassBuilder
{
    public static CommandClassDefinition GetCreateCommandClassDef(this ApplicationProject project, string feature, string command, ITable table)
    {
        var definition = CSharpClassDefinition.Create<CommandClassDefinition>(AccessModifier.Public, project.GetCommandClassName(command), ns: project.GetApplicationFeaturesNs(feature))
            .Implement("IValidatableObject")
            .UsingNs("System.ComponentModel.DataAnnotations")
            .UsingNs("System.Linq")
            .AddCtor(new ClassConstructorDefinition(AccessModifier.Public))
            .Add(new MethodDefinition(AccessModifier.Public, "IEnumerable<ValidationResult>", "Validate")
                .AddParam("ValidationContext", "validationContext")
                .Set(body => body.Line("return Enumerable.Empty<ValidationResult>();"))
            );

        foreach (var column in table.Columns)
        {
            var propType = project.Database.ResolveDatabaseType(column);

            definition.AddAutomaticProperty(propType, project.GetPropName(table, column));
        }

        definition.SimplifyDataTypes();

        return (CommandClassDefinition)definition;
    }
}
