using CatFactory.CodeFactory;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class RepositoryExtensionsClassBuilder
    {
        public static RepositoryExtensionsClassDefinition GetRepositoryExtensionsClassDefinition(this EntityFrameworkCoreProject project)
        {
            var definition = new RepositoryExtensionsClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Linq",
                    project.GetDataLayerNamespace(),
                    project.GetEntityLayerNamespace()
                },
                Namespace = project.GetDataLayerRepositoriesNamespace(),
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                Name = "RepositoryExtensions"
            };

            definition.Methods.Add(new MethodDefinition("IQueryable<TEntity>", "Paging", new ParameterDefinition(project.GetDbContextName(project.Database), "dbContext"), new ParameterDefinition("int", "pageSize", "0"), new ParameterDefinition("int", "pageNumber", "0"))
            {
                AccessModifier = AccessModifier.Public,
                IsExtension = true,
                IsStatic = true,
                GenericTypes =
                {
                    new GenericTypeDefinition
                    {
                        Name = "TEntity",
                        Constraint = "TEntity : class"
                    }
                },
                Lines =
                {
                    new CodeLine("var query = dbContext.Set<TEntity>().AsQueryable();"),
                    new CodeLine(),
                    new CodeLine("return pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;")
                }
            });

            definition.Methods.Add(new MethodDefinition("IQueryable<TModel>", "Paging", new ParameterDefinition("IQueryable<TModel>", "query"), new ParameterDefinition("int", "pageSize", "0"), new ParameterDefinition("int", "pageNumber", "0"))
            {
                AccessModifier = AccessModifier.Public,
                IsExtension = true,
                IsStatic = true,
                GenericTypes =
                {
                    new GenericTypeDefinition
                    {
                        Name = "TModel",
                        Constraint = "TModel : class"
                    }
                },
                Lines =
                {
                    new CodeLine("return pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;")
                }
            });

            return definition;
        }
    }
}
