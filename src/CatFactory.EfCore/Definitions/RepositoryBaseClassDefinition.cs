using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class RepositoryBaseClassDefinition : CSharpClassDefinition
    {
        public RepositoryBaseClassDefinition(EfCoreProject project)
            : base()
        {
            Project = project;

            Init();
        }

        public EfCoreProject Project { get; }

        public void Init()
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Threading.Tasks");
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            if (Project.Settings.AuditEntity != null)
            {
                Namespaces.Add(Project.GetEntityLayerNamespace());
            }

            Namespace = Project.GetDataLayerContractsNamespace();

            Name = "Repository";

            Fields.Add(new FieldDefinition(AccessModifier.Protected, "Boolean", "Disposed"));
            Fields.Add(new FieldDefinition(AccessModifier.Protected, Project.Database.GetDbContextName(), "DbContext"));

            Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(Project.Database.GetDbContextName(), "dbContext"))
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
                    new CodeLine("{"),
                    new CodeLine(1, "DbContext?.Dispose();"),
                    new CodeLine(),
                    new CodeLine(1, "Disposed = true;"),
                    new CodeLine("}")
                }
            });

            Methods.Add(GetAddMethod(Project));

            Methods.Add(GetUpdateMethod(Project));

            Methods.Add(GetRemoveMethod(Project));

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
                GenericType = "TEntity",
                WhereConstraints = new List<String>()
                {
                    "TEntity : class"
                },
                Lines = lines
            };
        }

        protected virtual MethodDefinition GetRemoveMethod(EfCoreProject project)
        {
            return new MethodDefinition(AccessModifier.Protected, "void", "Remove", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
                GenericType = "TEntity",
                WhereConstraints = new List<String>()
                {
                    "TEntity : class"
                },
                Lines = new List<ILine>()
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
