using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore
{
    public static class ValueConversion
    {
        public static EntityFrameworkCoreProject ScaffoldValueConversion(this EntityFrameworkCoreProject project)
        {
            var boolToStringConverters = new CSharpClassDefinition
            {
                Namespaces =
                {
                    "Microsoft.EntityFrameworkCore.Storage.ValueConversion"
                },
                Namespace = "ValueConversion",
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                Name = "BoolToStringConverters",
                Fields =
                {
                    new FieldDefinition
                    {
                        AccessModifier = AccessModifier.Private,
                        IsStatic = true,
                        IsReadOnly = true,
                        Type = "BoolToStringConverter",
                        Name = "bYN",
                        Value = "new BoolToStringConverter(\"N\", \"Y\")"
                    }
                }
            };

            CSharpCodeBuilder.CreateFiles(project.OutputDirectory, project.GetDataLayerDirectory("ValueConversion"), true, boolToStringConverters);

            return project;
        }
    }
}
