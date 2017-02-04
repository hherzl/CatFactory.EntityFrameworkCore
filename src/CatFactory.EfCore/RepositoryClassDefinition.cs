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
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Threading.Tasks");
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Name = projectFeature.GetClassRepositoryName();

            BaseClass = "Repository";

            Implements.Add(projectFeature.GetInterfaceRepositoryName());

            Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(projectFeature.Database.GetDbContextName(), "dbContext"))
            {
                ParentInvoke = "base(dbContext)"
            });

            foreach (var dbObject in projectFeature.DbObjects)
            {
                if (dbObject.Type == "STORED_PROCEDURE")
                {
                    // todo: add logic to invoke stored procedures
                    continue;
                }
                
                Methods.Add(GetGetAllMethod(projectFeature, dbObject));
                
                AddGetByUniqueMethods(projectFeature, dbObject);

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
            return new MethodDefinition(String.Format("IQueryable<{0}>", dbObject.GetSingularName()), String.Format("Get{0}", dbObject.GetPluralName()), new ParameterDefinition("Int32", "pageSize", "0"), new ParameterDefinition("Int32", "pageNumber", "0"))
            {
                Lines = new List<CodeLine>()
                {
                    new CodeLine("return Paging<{0}>(pageSize, pageNumber);", dbObject.GetSingularName())
                }
            };
        }

        public void AddGetByUniqueMethods(ProjectFeature projectFeature, DbObject dbObject)
        {
            var table = projectFeature.Database.Tables.FirstOrDefault(item => item.FullName == dbObject.FullName);

            if (table == null)
            {
                return;
            }

            foreach (var unique in table.Uniques)
            {
                var expression = String.Format("item => {0}", String.Join(" && ", unique.Key.Select(item => String.Format("item.{0} == entity.{0}", NamingConvention.GetPropertyName(item)))));

                Methods.Add(new MethodDefinition(String.Format("Task<{0}>", dbObject.GetSingularName()), String.Format("Get{0}By{1}Async", dbObject.GetSingularName(), String.Join("And", unique.Key.Select(item => NamingConvention.GetPropertyName(item)))), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
                {
                    IsAsync = true,
                    Lines = new List<CodeLine>()
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", Project.DeclareDbSetPropertiesInDbContext ? dbObject.GetPluralName() : String.Format("Set<{0}>()", dbObject.GetSingularName()), expression)
                    }
                });
            }
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
                        expression = String.Format("item => item.{0} == entity.{0}", NamingConvention.GetPropertyName(table.Identity.Name));
                    }
                }
                else
                {
                    expression = String.Format("item => {0}", String.Join(" && ", table.PrimaryKey.Key.Select(item => String.Format("item.{0} == entity.{0}", NamingConvention.GetPropertyName(item)))));
                }
            }

            return new MethodDefinition(String.Format("Task<{0}>", dbObject.GetSingularName()), String.Format("Get{0}Async", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<CodeLine>()
                {
                    new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", Project.DeclareDbSetPropertiesInDbContext ? dbObject.GetPluralName() : String.Format("Set<{0}>()", dbObject.GetSingularName()), expression)
                }
            };
        }

        public MethodDefinition GetAddMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            return new MethodDefinition("Task<Int32>", String.Format("Add{0}Async", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext.{0}.Add(entity);", Project.DeclareDbSetPropertiesInDbContext ? dbObject.GetPluralName() : String.Format("Set<{0}>()", dbObject.GetSingularName())),
                    new CodeLine(),
                    new CodeLine("return await CommitChangesAsync();")
                }
            };
        }

        public MethodDefinition GetUpdateMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            var lines = new List<CodeLine>();

            lines.Add(new CodeLine("var entity = await Get{0}Async(changes);", dbObject.GetSingularName()));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("if (entity != null)"));
            lines.Add(new CodeLine("{{"));

            var table = projectFeature.Database.Tables.FirstOrDefault(x => x.FullName == dbObject.FullName);

            if (table != null)
            {
                foreach (var column in table.GetUpdateColumns(Project))
                {
                    lines.Add(new CodeLine(1, "entity.{0} = changes.{0};", column.GetPropertyName()));
                }
            }

            lines.Add(new CodeLine("}}"));

            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", String.Format("Update{0}Async", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "changes"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetDeleteMethod(ProjectFeature projectFeature, DbObject dbObject)
        {
            return new MethodDefinition("Task<Int32>", String.Format("Delete{0}Async", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<CodeLine>()
                {
                    new CodeLine("DbContext.{0}.Remove(entity);", Project.DeclareDbSetPropertiesInDbContext ? dbObject.GetPluralName() : String.Format("Set<{0}>()", dbObject.GetSingularName())),
                    new CodeLine(),
                    new CodeLine("return await CommitChangesAsync();")
                }
            };
        }
    }
}
