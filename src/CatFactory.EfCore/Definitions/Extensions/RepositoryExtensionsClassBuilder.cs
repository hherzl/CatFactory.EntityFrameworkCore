using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions.Extensions
{
    public static class RepositoryExtensionsClassBuilder
    {
        public static RepositoryExtensionsClassDefinition GetRepositoryExtensionsClassDefinition(this EntityFrameworkCoreProject project)
        {
            var classDefinition = new RepositoryExtensionsClassDefinition();

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("System.Linq");
            classDefinition.Namespaces.Add(project.GetDataLayerNamespace());
            classDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

            classDefinition.Namespace = project.GetDataLayerRepositoriesNamespace();
            classDefinition.IsStatic = true;
            classDefinition.Name = "RepositoryExtensions";

            classDefinition.Methods.Add(new MethodDefinition("IQueryable<TEntity>", "Paging", new ParameterDefinition(project.Database.GetDbContextName(), "dbContext"), new ParameterDefinition("Int32", "pageSize", "0"), new ParameterDefinition("Int32", "pageNumber", "0"))
            {
                IsExtension = true,
                IsStatic = true,
                GenericTypes = new List<GenericTypeDefinition>
                {
                    new GenericTypeDefinition
                    {
                        Name = "TEntity",
                        Constraint = "TEntity : class"
                    }
                },
                Lines = new List<ILine>
                {
                    new CodeLine("var query = dbContext.Set<TEntity>().AsQueryable();"),
                    new CodeLine(),
                    new CodeLine("return pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;")
                }
            });

            classDefinition.Methods.Add(new MethodDefinition("IQueryable<TModel>", "Paging", new ParameterDefinition("IQueryable<TModel>", "query"), new ParameterDefinition("Int32", "pageSize", "0"), new ParameterDefinition("Int32", "pageNumber", "0"))
            {
                IsExtension = true,
                IsStatic = true,
                GenericTypes = new List<GenericTypeDefinition>
                {
                    new GenericTypeDefinition
                    {
                        Name = "TModel",
                        Constraint = "TModel : class"
                    }
                },
                Lines = new List<ILine>
                {
                    new CodeLine("return pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;")
                }
            });

            return classDefinition;
        }
    }
}
