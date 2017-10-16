using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class RepositoryExtensionsClassDefinition
    {
        public static CSharpClassDefinition GetRepositoryExtensionsClassDefinition(this EfCoreProject project)
        {
            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("System.Linq");
            classDefinition.Namespaces.Add(project.GetDataLayerNamespace());
            classDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

            classDefinition.Name = "RepositoryExtensions";
            classDefinition.Namespace = project.GetDataLayerRepositoriesNamespace();
            classDefinition.IsStatic = true;

            classDefinition.Methods.Add(new MethodDefinition("IQueryable<TEntity>", "Paging", new ParameterDefinition(project.Database.GetDbContextName(), "dbContext"), new ParameterDefinition("Int32", "pageSize", "0"), new ParameterDefinition("Int32", "pageNumber", "0"))
            {
                GenericType = "TEntity",
                IsExtension = true,
                IsStatic = true,
                WhereConstraints = new List<String>()
                {
                    "TEntity : class, IEntity",
                },
                Lines = new List<ILine>()
                {
                    new CodeLine("var query = dbContext.Set<TEntity>().AsQueryable();"),
                    new CodeLine(),
                    new CodeLine("return pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;")
                }
            });

            classDefinition.Methods.Add(new MethodDefinition("IQueryable<T>", "Paging", new ParameterDefinition("IQueryable<T>", "query"), new ParameterDefinition("Int32", "pageSize", "0"), new ParameterDefinition("Int32", "pageNumber", "0"))
            {
                GenericType = "T",
                IsExtension = true,
                IsStatic = true,
                WhereConstraints = new List<String>()
                {
                    "T : class",
                },
                Lines = new List<ILine>()
                {
                    new CodeLine("return pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;")
                }
            });

            return classDefinition;
        }
    }
}
