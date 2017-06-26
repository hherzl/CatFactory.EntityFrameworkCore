using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class RepositoryBaseClassDefinition : CSharpClassDefinition
    {
        public RepositoryBaseClassDefinition(EfCoreProject project)
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Threading.Tasks");
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            if (project.Settings.AuditEntity != null)
            {
                Namespaces.Add(project.GetEntityLayerNamespace());
            }

            Name = "Repository";

            Fields.Add(new FieldDefinition(AccessModifier.Protected, "Boolean", "Disposed"));
            Fields.Add(new FieldDefinition(AccessModifier.Protected, project.Database.GetDbContextName(), "DbContext"));

            Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(project.Database.GetDbContextName(), "dbContext"))
            {
                Lines = new List<ILine>()
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

            Methods.Add(GetAddMethod(project));

            Methods.Add(GetUpdateMethod(project));

            Methods.Add(new MethodDefinition(AccessModifier.Protected, "void", "Remove", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
                GenericType = "TEntity",
                WhereConstraints = new List<String>()
                {
                    "TEntity : class"
                },
                Lines = new List<ILine>()
                {
                    new CodeLine("var dbSet = DbContext.Set<TEntity>();"),
                    new CodeLine(),
                    new CodeLine("var entry = DbContext.Entry(entity);"),
                    new CodeLine(),
                    new CodeLine("if (entry.State == EntityState.Deleted)"),
                    new CodeLine("{{"),
                    new CodeLine(1, "dbSet.Attach(entity);"),
                    new CodeLine(1, "dbSet.Remove(entity);"),
                    new CodeLine("}}"),
                    new CodeLine("else"),
                    new CodeLine("{{"),
                    new CodeLine(1, "entry.State = EntityState.Deleted;"),
                    new CodeLine("}}"),
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

        protected virtual MethodDefinition GetAddMethod(EfCoreProject project)
        {
            var lines = new List<ILine>();

            if (project.Settings.AuditEntity != null)
            {
                lines.AddRange(new List<ILine>
                {
                    new CodeLine("var cast = entity as IAuditEntity;"),
                    new CodeLine(),
                    new CodeLine("if (cast != null)"),
                    new CodeLine("{{"),
                    new CodeLine(1, "if (!cast.CreationDateTime.HasValue)"),
                    new CodeLine(1, "{{"),
                    new CodeLine(2, "cast.CreationDateTime = DateTime.Now;"),
                    new CodeLine(1, "}}"),
                    new CodeLine("}}"),
                    new CodeLine()
                });
            }

            lines.AddRange(new List<ILine>
            {
                new CodeLine("var entry = DbContext.Entry(entity);"),
                new CodeLine(),
                new CodeLine("if (entry.State != EntityState.Detached)"),
                new CodeLine("{{"),
                new CodeLine(1, "entry.State = EntityState.Added;"),
                new CodeLine("}}"),
                new CodeLine("else"),
                new CodeLine("{{"),
                new CodeLine(1, "var dbSet = DbContext.Set<TEntity>();"),
                new CodeLine(),
                new CodeLine(1, "dbSet.Add(entity);"),
                new CodeLine("}}")
            });

            return new MethodDefinition(AccessModifier.Protected, "void", "Add", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
                GenericType = "TEntity",
                WhereConstraints = new List<String>()
                {
                    "TEntity : class"
                },
                Lines = lines
            };
        }

        protected virtual MethodDefinition GetUpdateMethod(EfCoreProject project)
        {
            var lines = new List<ILine>();

            if (project.Settings.AuditEntity != null)
            {
                lines.AddRange(new List<ILine>
                {
                    new CodeLine("var cast = entity as IAuditEntity;"),
                    new CodeLine(),
                    new CodeLine("if (cast != null)"),
                    new CodeLine("{{"),
                    new CodeLine(1, "if (!cast.LastUpdateDateTime.HasValue)"),
                    new CodeLine(1, "{{"),
                    new CodeLine(2, "cast.LastUpdateDateTime = DateTime.Now;"),
                    new CodeLine(1, "}}"),
                    new CodeLine("}}"),
                    new CodeLine()
                });
            }

            lines.AddRange(new List<ILine>
            {
                new CodeLine("var entry = DbContext.Entry(entity);"),
                new CodeLine(),
                new CodeLine("if (entry.State == EntityState.Detached)"),
                new CodeLine("{{"),
                new CodeLine(1, "dbSet?.Attach(entity);"),
                new CodeLine("}}"),
                new CodeLine(),
                new CodeLine("entry.State = EntityState.Modified;")
            });

            return new MethodDefinition(AccessModifier.Protected, "void", "Update", new ParameterDefinition("TEntity", "entity"), new ParameterDefinition("DbSet<TEntity>", "dbSet", "null"))
            {
                IsVirtual = true,
                GenericType = "TEntity",
                WhereConstraints = new List<String>()
                {
                    "TEntity : class"
                },
                Lines = lines
            };
        }
    }
}
