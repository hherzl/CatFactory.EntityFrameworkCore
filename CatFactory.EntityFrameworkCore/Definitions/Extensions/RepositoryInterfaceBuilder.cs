using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class RepositoryInterfaceBuilder
    {
        public static RepositoryInterfaceDefinition GetRepositoryInterfaceDefinition(this EntityFrameworkCoreProject project)
            => new RepositoryInterfaceDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Threading.Tasks"
                },
                Namespace = project.GetDataLayerContractsNamespace(),
                Name = "IRepository",
                Implements =
                {
                    "IDisposable"
                },
                Methods =
                {
                    new MethodDefinition("void", "Add", new ParameterDefinition("TEntity", "entity"))
                    {
                        GenericTypes =
                        {
                            new GenericTypeDefinition
                            {
                                Name = "TEntity",
                                Constraint = "TEntity : class"
                            }
                        }
                    },
                    new MethodDefinition("void", "Update", new ParameterDefinition("TEntity", "entity"))
                    {
                        GenericTypes =
                        {
                            new GenericTypeDefinition
                            {
                                Name = "TEntity",
                                Constraint = "TEntity : class"
                            }
                        }
                    },
                    new MethodDefinition("void", "Remove", new ParameterDefinition("TEntity", "entity"))
                    {
                        GenericTypes =
                        {
                            new GenericTypeDefinition
                            {
                                Name = "TEntity",
                                Constraint = "TEntity : class"
                            }
                        }
                    },
                    new MethodDefinition("int", "CommitChanges"),
                    new MethodDefinition("Task<int>", "CommitChangesAsync")
                }
            };
    }
}
