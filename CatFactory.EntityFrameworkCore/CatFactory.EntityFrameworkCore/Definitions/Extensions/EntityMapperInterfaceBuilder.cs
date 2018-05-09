using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class EntityMapperInterfaceBuilder
    {
        public static EntityMapperInterfaceDefinition GetEntityMapperInterfaceDefinition(this EntityFrameworkCoreProject project)
        {
            var interfaceDefinition = new EntityMapperInterfaceDefinition();

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
