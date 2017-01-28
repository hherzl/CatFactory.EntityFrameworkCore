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
        public RepositoryClassDefinition(EfCoreProject project, ProjectFeature projectFeature)
        {
            Project = project;

            Namespaces.Add("System");
            Namespaces.Add("System.Collections.Generic");
            Namespaces.Add("System.Linq");

            Name = projectFeature.GetClassRepositoryName();

            BaseClass = "Repository";

            Implements.Add(projectFeature.GetInterfaceRepositoryName());

            Constructors.Add(new ClassConstructorDefinition()
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(projectFeature.Database.GetDbContextName(), "dbContext")
                },
                ParentInvoke = "base(dbContext)"
            });

            foreach (var dbObject in projectFeature.DbObjects)
            {
                if (dbObject.Type == "procedure")
                {
                    // todo: add logic to invoke stored procedures
                    continue;
                }

                Methods.Add(GetGetAllMethod(projectFeature, dbObject));

                if (!projectFeature.IsView(dbObject))
                {
                    Methods.Add(GetGetMethod(projectFeature, dbObject));
                    Methods.Add(GetAddMethod(projectFeature, dbObject));
                    Methods.Add(GetUpdateMethod(projectFeature, dbObject));
                    Methods.Add(GetDeleteMethod(projectFeature, dbObject));
                }
            }
        }

        public EfCoreProject Project { get; set; }

        public MethodDefinition GetGetAllMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            return new MethodDefinition(String.Format("IQueryable<{0}>", dbObject.GetSingularName()), String.Format("Get{0}", dbObject.GetPluralName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition("Int32", "pageSize") { DefaultValue = "0" },
                    new ParameterDefinition("Int32", "pageNumber")  { DefaultValue = "0" }
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("var query = DbContext.{0};", Project.DeclareDbSetPropertiesInDbContext ?  dbObject.GetEntityName() : String.Format("Set<{0}>()", dbObject.GetSingularName())),
                    new CodeLine(),
                    new CodeLine("return pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;")
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

            return new MethodDefinition(dbObject.GetSingularName(), String.Format("Get{0}", dbObject.GetSingularName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(dbObject.GetSingularName(), "entity")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("return DbContext.{0}.FirstOrDefault({1});", Project.DeclareDbSetPropertiesInDbContext ? dbObject.GetSingularName() : String.Format("Set<{0}>()", dbObject.GetSingularName()), expression)
                }
            };
        }

        public MethodDefinition GetAddMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            return new MethodDefinition("void", String.Format("Add{0}", dbObject.GetSingularName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(dbObject.GetSingularName(), "entity")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext.{0}.Add(entity);", Project.DeclareDbSetPropertiesInDbContext ? dbObject.GetSingularName() : String.Format("Set<{0}>()", dbObject.GetSingularName())),
                    new CodeLine(),
                    new CodeLine("DbContext.SaveChanges();")
                }
            };
        }

        public MethodDefinition GetUpdateMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            var lines = new List<CodeLine>();

            lines.Add(new CodeLine("var entity = Get{0}(changes);", dbObject.GetSingularName()));
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

            return new MethodDefinition("void", String.Format("Update{0}", dbObject.GetSingularName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(dbObject.GetSingularName(), "changes")
                },
                Lines = lines
            };
        }

        public MethodDefinition GetDeleteMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            return new MethodDefinition("void", String.Format("Delete{0}", dbObject.GetSingularName()))
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition(dbObject.GetSingularName(), "entity")
                },
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext.{0}.Remove(entity);", Project.DeclareDbSetPropertiesInDbContext ? dbObject.GetSingularName() : String.Format("Set<{0}>()", dbObject.GetSingularName())),
                    new CodeLine(),
                    new CodeLine("DbContext.SaveChanges();")
                }
            };
        }
    }
}
