using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class RepositoryBaseClassBuilder
    {
        public static RepositoryBaseClassDefinition GetRepositoryBaseClassDefinition(this EntityFrameworkCoreProject project)
        {
            var definition = new RepositoryBaseClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Threading.Tasks",
                },
                Namespace = project.GetDataLayerContractsNamespace(),
                Name = "Repository",
                Fields =
                {
                    new FieldDefinition(AccessModifier.Protected, "bool", "Disposed"),
                    new FieldDefinition(AccessModifier.Protected, project.Database.GetDbContextName(), "DbContext")
                },
                Constructors =
                {
                    new ClassConstructorDefinition(new ParameterDefinition(project.Database.GetDbContextName(), "dbContext"))
                    {
                        Lines =
                        {
                            new CodeLine("DbContext = dbContext;")
                        }
                    }
                },
                Methods =
                {
                    new MethodDefinition("void", "Dispose")
                    {
                        Lines =
                        {
                            new CodeLine("if (!Disposed)"),
                            new CodeLine("{"),
                            new CodeLine(1, "DbContext?.Dispose();"),
                            new CodeLine(),
                            new CodeLine(1, "Disposed = true;"),
                            new CodeLine("}")
                        }
                    },
                    GetAddMethod(project),
                    GetUpdateMethod(project),
                    GetRemoveMethod(project),
                    new MethodDefinition("int", "CommitChanges")
                    {
                        Lines =
                        {
                            new CodeLine("return DbContext.SaveChanges();")
                        }
                    },
                    new MethodDefinition("Task<int>", "CommitChangesAsync")
                    {
                        Lines =
                        {
                            new CodeLine("return DbContext.SaveChangesAsync();")
                        }
                    }
                }
            };

            var selection = project.GlobalSelection();

            if (selection.Settings.AuditEntity != null)
                definition.Namespaces.Add(project.GetEntityLayerNamespace());

            return definition;
        }

        private static MethodDefinition GetAddMethod(EntityFrameworkCoreProject project)
        {
            var lines = new List<ILine>();

            var selection = project.GlobalSelection();

            if (selection.Settings.AuditEntity == null)
            {
                lines.AddRange(new List<ILine>
                {
                    new CodeLine("DbContext.Add(entity);")
                });
            }
            else
            {
                lines.AddRange(new List<ILine>
                {
                    new CommentLine(" Cast entity to IAuditEntity"),
                    new CodeLine("if(entity is IAuditEntity cast)"),
                    new CodeLine("{"),
                    new CommentLine(1, " Set creation datetime"),
                    new CodeLine(1, "cast.CreationDateTime = DateTime.Now;"),
                    new CodeLine("}"),
                    new CodeLine(),
                    new CodeLine("DbContext.Add(entity);"),
                });
            }

            return new MethodDefinition("void", "Add", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
                GenericTypes =
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

            if (selection.Settings.AuditEntity == null)
            {
                lines.AddRange(new List<ILine>
                {
                    new CodeLine("DbContext.Update(entity);")
                });
            }
            else
            {
                lines.AddRange(new List<ILine>
                {
                    new CommentLine(" Cast entity to IAuditEntity"),
                    new CodeLine("if (entity is IAuditEntity cast)"),
                    new CodeLine("{"),
                    new CommentLine(1, " Set update datetime"),
                    new CodeLine(1, "cast.LastUpdateDateTime = DateTime.Now;"),
                    new CodeLine("}"),
                    new CodeLine(),
                    new CodeLine("DbContext.Update(entity);")
                });
            }

            return new MethodDefinition("void", "Update", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
                GenericTypes =
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
            => new MethodDefinition("void", "Remove", new ParameterDefinition("TEntity", "entity"))
            {
                IsVirtual = true,
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
                    new CodeLine("DbContext.Remove(entity);")
                }
            };
    }
}
