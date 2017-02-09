using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class BaseRepositoryClassDefinition : CSharpClassDefinition
    {
        public BaseRepositoryClassDefinition(EfCoreProject project)
        {
            Project = project;

            Namespaces.Add("System");
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Threading.Tasks");

            Name = "Repository";

            Fields.Add(new FieldDefinition(AccessModifier.Protected, "Boolean", "Disposed"));
            Fields.Add(new FieldDefinition(AccessModifier.Protected, project.Database.GetDbContextName(), "DbContext"));

            Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(project.Database.GetDbContextName(), "dbContext"))
            {
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext = dbContext;")
                }
            });

            Methods.Add(new MethodDefinition("void", "Dispose")
            {
                Lines = new List<ILine>()
                {
                    new CodeLine("if (!Disposed)"),
                    new CodeLine("{{"),
                    new CodeLine(1, "if (DbContext != null)"),
                    new CodeLine(1, "{{"),
                    new CodeLine(2, "DbContext.Dispose();"),
                    new CodeLine(),
                    new CodeLine(2, "Disposed = true;"),
                    new CodeLine(1, "}}"),
                    new CodeLine("}}")
                }
            });

            Methods.Add(new MethodDefinition(AccessModifier.Protected, "IQueryable<TEntity>", "Paging", new ParameterDefinition("Int32", "pageSize"), new ParameterDefinition("Int32", "pageNumber"))
            {
                GenericType = "TEntity",
                WhereConstraints = new List<String>()
                {
                    "TEntity : class"
                },
                Lines = new List<ILine>()
                {
                    new CodeLine("var query = DbContext.Set<TEntity>();"),
                    new CodeLine(),
                    new CodeLine("return pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;")
                }
            });

            Methods.Add(new MethodDefinition("Int32", "CommitChanges")
            {
                Lines = new List<ILine>()
                {
                    new CodeLine("return DbContext.SaveChanges();")
                }
            });

            Methods.Add(new MethodDefinition("Task<Int32>", "CommitChangesAsync")
            {
                Lines = new List<ILine>()
                {
                    new CodeLine("return DbContext.SaveChangesAsync();")
                }
            });

        }

        public EfCoreProject Project { get; set; }

    }
}
