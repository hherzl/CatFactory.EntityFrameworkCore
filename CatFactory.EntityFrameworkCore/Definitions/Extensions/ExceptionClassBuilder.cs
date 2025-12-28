using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions;

public static class ExceptionClassBuilder
{
    public static ExceptionClassDefinition GetExceptionClassDef(this DomainProject project)
    {
        var definition = CSharpClassDefinition.Create<ExceptionClassDefinition>(AccessModifier.Public, $"{project.Name}Exception", baseClass: "Exception", ns: project.GetDomainExceptionNs())
            .UsingNs("System")
            .AddCtor(new(AccessModifier.Public) { Invocation = "base()" })
            .AddCtor(new ClassConstructorDefinition(AccessModifier.Public) { Invocation = "base(message)" }.AddParam("string", "message"))
            ;

        return (ExceptionClassDefinition)definition;
    }
}
