using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class RepositoryBaseClassBuilder
    {
        public static RepositoryBaseClassDefinition GetRepositoryBaseClassDefinition(this EntityFrameworkCoreProject project)
        {
            var classDefinition = new RepositoryBaseClassDefinition();

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("System.Linq");
            classDefinition.Namespaces.Add("System.Threading.Tasks");
            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            var selection = project.GlobalSelection();

            if (selection.Settings.AuditEntity != null)
            {
                classDefinition.Namespaces.Add(project.GetEntityLayerNamespace());
            }

            classDefinition.Namespace = project.GetDataLayerContractsNamespace();

            classDefinition.Name = "Repository";

            classDefinition.Fields.Add(new FieldDefinition(AccessModifier.Protected, "Boolean", "Disposed"));
            classDefinition.Fields.Add(new FieldDefinition(AccessModifier.Protected, project.Database.GetDbContextName(), "DbContext"));

            classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(project.Database.GetDbContextName(), "dbContext"))
            {
                Lines = new List<ILine>
                {
                    new CodeLine("DbContext = dbContext;")
                }
            });

            classDefinition.Methods.Add(new MethodDefinition("void", "Dispose")
            {
                Lines = new List<ILine>
                {
                    new CodeLine("if (!Disposed)"),
                    new CodeLine("{"),
                    new CodeLine(1, "DbContext?.Dispose();"),
                    new CodeLine(),
                    new CodeLine(1, "Disposed = true;"),
                    new CodeLine("}")
                }
            });

            classDefinition.Methods.Add(GetAddMethod(project));

            classDefinition.Methods.Add(GetUpdateMethod(project));

            classDefinition.Methods.Add(GetRemoveMethod(project));

            classDefinition.Methods.Add(new MethodDefinition("Int32", "CommitChanges")
            {
                Lines = new List<ILine>
                {
                    new CodeLine("return DbContext.SaveChanges();")
                }
            });

            classDefinition.Methods.Add(new MethodDefinition("Task<Int32>", "CommitChangesAsync")
            {
                Lines = new List<ILine>
                {
                    new CodeLine("return DbContext.SaveChangesAsync();")
                }
            });

            return classDefinition;
        }

        private static MethodDefinition GetAddMethod(EntityFrameworkCoreProject project)
        {
            var lines = new List<ILine>();

            var selection = project.GlobalSelection();

            if (selection.Settings.AuditEntity != null)
            {
                lines.AddRange(new List<ILine>
                {
                    new CommentLine(" Cast entity to IAuditEntity"),
                    new CodeLine("var cast = entity as IAuditEntity;"),
                    new CodeLine(),
                    new CodeLine("if (cast != null)"),
                    new CodeLine("{"),
                    new CodeLine(1, "if (!cast.CreationDateTime.HasValue)"),
                    new CodeLine(1, "{"),
                    new CommentLine(2, " Set creation date time"),
                    new CodeLine(2, "cast.CreationDateTime = DateTime.Now;"),
                    new CodeLine(1, "}"),
                    new CodeLine("}"),
                    new CodeLine()
                });
            }

            lines.AddRange(new List<ILine>
            {
                new CommentLine(" Get entry from Db context"),
                new CodeLine("var entry = DbContext.Entry(entity);"),
                new CodeLine(),
                new CodeLine("if (entry.State != EntityState.Detached)"),
                new CodeLine("{"),
                new CommentLine(1, " Set state for entity entry"),
                new CodeLine(1, "entry.State = EntityState.Added;"),
                new CodeLine("}"),
                new CodeLine("else"),
                new CodeLine("{"),
                new CommentLine(1, " Add entity to DbSet"),
                new CodeLine(1, "DbContext.Set<TEntity>().Add(entity);"),
                new CodeLine("}")
            });

            return new MethodDefinition(AccessModifier.Protected, "void", "Add", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
                GenericTypes = new List<GenericTypeDefinition>
                {
                    new GenericTypeDefinition
                    {
                        Name = "TEntity",
                        Constraint = "TEntity : class"
                    }
                },
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethod(EntityFrameworkCoreProject project)
        {
            var lines = new List<ILine>();

            var selection = project.GlobalSelection();

            if (selection.Settings.AuditEntity != null)
            {
                lines.AddRange(new List<ILine>
                {
                    new CodeLine("var cast = entity as IAuditEntity;"),
                    new CodeLine(),
                    new CodeLine("if (cast != null)"),
                    new CodeLine("{"),
                    new CodeLine(1, "if (!cast.LastUpdateDateTime.HasValue)"),
                    new CodeLine(1, "{"),
                    new CommentLine(2, " Set last update date time"),
                    new CodeLine(2, "cast.LastUpdateDateTime = DateTime.Now;"),
                    new CodeLine(1, "}"),
                    new CodeLine("}"),
                    new CodeLine()
                });
            }

            lines.AddRange(new List<ILine>
            {
                new CommentLine(" Get entity's entry"),
                new CodeLine("var entry = DbContext.Entry(entity);"),
                new CodeLine(),
                new CodeLine("if (entry.State == EntityState.Detached)"),
                new CodeLine("{"),
                new CommentLine(1, " Attach entity to DbSet"),
                new CodeLine(1, "DbContext.Set<TEntity>().Attach(entity);"),
                new CodeLine("}"),
                new CodeLine(),
                new CodeLine("entry.State = EntityState.Modified;")
            });

            return new MethodDefinition(AccessModifier.Protected, "void", "Update", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
                GenericTypes = new List<GenericTypeDefinition>
                {
                    new GenericTypeDefinition
                    {
                        Name = "TEntity",
                        Constraint = "TEntity : class"
                    }
                },
                Lines = lines
            };
        }

        private static MethodDefinition GetRemoveMethod(EntityFrameworkCoreProject project)
        {
            return new MethodDefinition(AccessModifier.Protected, "void", "Remove", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
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
                    new CommentLine(" Get entity's entry"),
                    new CodeLine("var entry = DbContext.Entry(entity);"),
                    new CodeLine(),
                    new CodeLine("if (entry.State == EntityState.Deleted)"),
                    new CodeLine("{"),
                    new CommentLine(1, " Create set for entity"),
                    new CodeLine(1, "var dbSet = DbContext.Set<TEntity>();"),
                    new CodeLine(),
                    new CommentLine(1, " Attach and remove entity from DbSet"),
                    new CodeLine(1, "dbSet.Attach(entity);"),
                    new CodeLine(1, "dbSet.Remove(entity);"),
                    new CodeLine("}"),
                    new CodeLine("else"),
                    new CodeLine("{"),
                    new CommentLine(1, " Set state for entity to 'Deleted'"),
                    new CodeLine(1, "entry.State = EntityState.Deleted;"),
                    new CodeLine("}"),
                }
            };
        }
    }
}
