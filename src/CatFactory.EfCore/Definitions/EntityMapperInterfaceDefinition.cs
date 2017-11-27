using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class EntityMapperInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetEntityMapperInterfaceDefinition(this EntityFrameworkCoreProject project)
        {
            var interfaceDefinition = new CSharpInterfaceDefinition();

            interfaceDefinition.Namespaces.Add("System.Collections.Generic");
            interfaceDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            interfaceDefinition.Namespace = project.GetDataLayerConfigurationsNamespace();

            interfaceDefinition.Name = "IEntityMapper";

            interfaceDefinition.Properties.Add(new PropertyDefinition("IEnumerable<IEntityTypeConfiguration>", "Configurations") { IsReadOnly = true });

            interfaceDefinition.Methods.Add(new MethodDefinition("void", "ConfigureEntities", new ParameterDefinition("ModelBuilder", "modelBuilder")));

            return interfaceDefinition;
        }
    }
}
