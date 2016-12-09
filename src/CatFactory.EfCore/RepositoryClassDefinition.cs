using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class RepositoryClassDefinition : CSharpClassDefinition
    {
        public RepositoryClassDefinition(ProjectFeature projectFeature)
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Collections.Generic");
            Namespaces.Add("System.Linq");

            Name = projectFeature.GetClassRepositoryName();

            Implements.Add(nameof(IDisposable));
            Implements.Add(projectFeature.GetInterfaceRepositoryName());

            Fields.Add(new FieldDefinition("Boolean", "Disposed") { ModifierAccess = ModifierAccess.Protected });
            Fields.Add(new FieldDefinition(projectFeature.Database.GetDbContextName(), "DbContext") { ModifierAccess = ModifierAccess.Protected });

            Constructors.Add(new ClassConstructorDefinition()
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(projectFeature.Database.GetDbContextName(), "dbContext")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext = dbContext;")
                }
            });

            Methods.Add(new MethodDefinition("void", "Dispose")
            {
                Lines = new List<CodeLine>()
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

            foreach (var dbObject in projectFeature.DbObjects)
            {
                // todo: add primary predicate for get method

                Methods.Add(GetGetAllMethod(projectFeature, dbObject));
                Methods.Add(GetGetMethod(projectFeature, dbObject));

                if (!projectFeature.IsView(dbObject))
                {
                    Methods.Add(GetAddMethod(projectFeature, dbObject));
                    Methods.Add(GetUpdateMethod(projectFeature, dbObject));
                    Methods.Add(GetDeleteMethod(projectFeature, dbObject));
                }
            }
        }

        public MethodDefinition GetGetAllMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            return new MethodDefinition(String.Format("IEnumerable<{0}>", dbObject.GetEntityName()), String.Format("Get{0}", dbObject.GetPluralName()))
            {
                Lines = new List<CodeLine>()
                {
                    new CodeLine("return DbContext.Set<{0}>();", dbObject.GetEntityName())
                }
            };
        }

        public MethodDefinition GetGetMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            var table = projectFeature.Database.Tables.FirstOrDefault(item => item.FullName == dbObject.FullName);

            var expression = String.Empty;

            if (table != null)
            {
                if (table.PrimaryKey == null)
                {
                    if (table.Identity != null)
                    {
                        expression = String.Format("item => item.{0} == entity.{0}", table.Identity.Name);
                    }
                }
                else
                {
                    expression = String.Format("item => {0}", String.Join(" && ", table.PrimaryKey.Key.Select(item => String.Format("item.{0} == entity.{0}", item))));
                }
            }

            return new MethodDefinition(dbObject.GetEntityName(), String.Format("Get{0}", dbObject.GetEntityName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(dbObject.GetEntityName(), "entity")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("return DbContext.Set<{0}>().FirstOrDefault({1});", dbObject.GetEntityName(), expression)
                }
            };
        }

        public MethodDefinition GetAddMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            return new MethodDefinition("void", String.Format("Add{0}", dbObject.GetEntityName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(dbObject.GetEntityName(), "entity")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext.Set<{0}>().Add(entity);", dbObject.GetEntityName()),
                    new CodeLine(),
                    new CodeLine("DbContext.SaveChanges();")
                }
            };
        }

        public MethodDefinition GetUpdateMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            var lines = new List<CodeLine>();

            lines.Add(new CodeLine("var entity = Get{0}(changes);", dbObject.GetEntityName()));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("if (entity != null)"));
            lines.Add(new CodeLine("{{"));

            var table = projectFeature.Database.Tables.FirstOrDefault(x => x.FullName == dbObject.FullName);

            if (table != null)
            {
                foreach (var column in table.GetColumnsWithOutKey())
                {
                    lines.Add(new CodeLine(1, "entity.{0} = changes.{0};", column.GetPropertyName()));
                }
            }

            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "DbContext.SaveChanges();"));
            lines.Add(new CodeLine("}}"));

            return new MethodDefinition("void", String.Format("Update{0}", dbObject.GetEntityName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(dbObject.GetEntityName(), "changes")
                },
                Lines = lines
            };
        }

        public MethodDefinition GetDeleteMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            return new MethodDefinition("void", String.Format("Delete{0}", dbObject.GetEntityName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(dbObject.GetEntityName(), "entity")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext.Set<{0}>().Remove(entity);", dbObject.GetEntityName()),
                    new CodeLine(),
                    new CodeLine("DbContext.SaveChanges();")
                }
            };
        }
    }
}
