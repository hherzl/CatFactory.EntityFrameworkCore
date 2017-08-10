using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public class RepositoryClassDefinition : CSharpClassDefinition
    {
        public RepositoryClassDefinition(ProjectFeature projectFeature)
            : base()
        {
            ProjectFeature = projectFeature;

            Init();
        }

        public ProjectFeature ProjectFeature { get; }

        public override void Init()
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Threading.Tasks");
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            foreach (var dbObject in ProjectFeature.DbObjects)
            {
                var table = ProjectFeature.Project.Database.Tables.FirstOrDefault(item => item.FullName == dbObject.FullName);

                if (table == null)
                {
                    continue;
                }

                if (table.HasDefaultSchema())
                {
                    Namespaces.AddUnique(ProjectFeature.Project.GetEntityLayerNamespace());
                }
                else
                {
                    Namespaces.AddUnique(ProjectFeature.GetProject().GetEntityLayerNamespace(table.Schema));
                }

                Namespaces.AddUnique(ProjectFeature.GetProject().GetDataLayerContractsNamespace());
            }

            Namespace = ProjectFeature.GetProject().GetDataLayerRepositoriesNamespace();

            Name = ProjectFeature.GetClassRepositoryName();

            BaseClass = "Repository";

            Implements.Add(ProjectFeature.GetInterfaceRepositoryName());

            Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(ProjectFeature.Project.Database.GetDbContextName(), "dbContext"))
            {
                ParentInvoke = "base(dbContext)"
            });

            var dbos = ProjectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();
            var tables = ProjectFeature.Project.Database.Tables.Where(t => dbos.Contains(t.FullName)).ToList();

            foreach (var table in tables)
            {
                if (!Namespaces.Contains(ProjectFeature.GetProject().GetDataLayerDataContractsNamespace()))
                {
                    if (ProjectFeature.GetProject().Settings.EntitiesWithDataContracts.Contains(table.FullName) && !Namespaces.Contains(ProjectFeature.GetProject().GetDataLayerDataContractsNamespace()))
                    {
                        Namespaces.Add(ProjectFeature.GetProject().GetDataLayerDataContractsNamespace());
                    }
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    if (String.IsNullOrEmpty(foreignKey.Child))
                    {
                        var child = ProjectFeature.Project.Database.FindTableBySchemaAndName(foreignKey.Child);

                        if (child != null)
                        {
                            Namespaces.AddUnique(ProjectFeature.GetProject().GetDataLayerDataContractsNamespace());
                        }
                    }
                }

                Methods.Add(GetGetAllMethod(ProjectFeature, table));

                AddGetByUniqueMethods(ProjectFeature, table);

                Methods.Add(GetGetMethod(ProjectFeature, table));
                Methods.Add(GetAddMethod(ProjectFeature, table));
                Methods.Add(GetUpdateMethod(ProjectFeature, table));
                Methods.Add(GetRemoveMethod(ProjectFeature, table));
            }
        }

        public MethodDefinition GetGetAllMethod(ProjectFeature projectFeature, IDbObject dbObject)
        {
            var returnType = String.Empty;

            var lines = new List<ILine>();

            var tableCast = dbObject as Table;

            if (tableCast == null)
            {
                returnType = dbObject.GetSingularName();

                lines.Add(new CodeLine("return query.Paging(pageSize, pageNumber);"));
            }
            else
            {
                if (ProjectFeature.GetProject().Settings.EntitiesWithDataContracts.Contains(tableCast.FullName))
                {
                    var entityAlias = CatFactory.NamingConvention.GetCamelCase(tableCast.GetEntityName());

                    returnType = tableCast.GetDataContractName();

                    var dataContractPropertiesSets = new[]
                    {
                        new
                        {
                            IsForeign = false,
                            Type = String.Empty,
                            Nullable = false,
                            ObjectSource = String.Empty,
                            PropertySource = String.Empty,
                            Target = String.Empty
                        }
                    }.ToList();

                    foreach (var column in tableCast.Columns)
                    {
                        var propertyName = column.GetPropertyName();

                        dataContractPropertiesSets.Add(new
                        {
                            IsForeign = false,
                            Type = column.Type,
                            Nullable = column.Nullable,
                            ObjectSource = entityAlias,
                            PropertySource = propertyName,
                            Target = propertyName
                        });
                    }

                    foreach (var foreignKey in tableCast.ForeignKeys)
                    {
                        var foreignTable = ProjectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                        if (foreignTable == null)
                        {
                            continue;
                        }

                        var foreignKeyAlias = CatFactory.NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                        foreach (var column in foreignTable?.GetColumnsWithOutKey())
                        {
                            if (dataContractPropertiesSets.Where(item => String.Format("{0}.{1}", item.ObjectSource, item.PropertySource) == String.Format("{0}.{1}", entityAlias, column.GetPropertyName())).Count() == 0)
                            {
                                var target = String.Format("{0}{1}", foreignTable.GetEntityName(), column.GetPropertyName());

                                dataContractPropertiesSets.Add(new
                                {
                                    IsForeign = true,
                                    Type = column.Type,
                                    Nullable = column.Nullable,
                                    ObjectSource = foreignKeyAlias,
                                    PropertySource = column.GetPropertyName(),
                                    Target = target
                                });
                            }
                        }
                    }

                    lines.Add(new CommentLine(" Get query from DbSet"));
                    lines.Add(new CodeLine("var query = from {0} in DbContext.Set<{1}>()", entityAlias, tableCast.GetEntityName()));

                    foreach (var foreignKey in tableCast.ForeignKeys)
                    {
                        var foreignTable = ProjectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                        if (foreignTable == null)
                        {
                            continue;
                        }

                        var foreignKeyEntityName = foreignTable.GetEntityName();

                        var foreignKeyAlias = CatFactory.NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                        if (foreignTable.HasDefaultSchema())
                        {
                            Namespaces.AddUnique(projectFeature.GetProject().GetEntityLayerNamespace());
                        }
                        else
                        {
                            Namespaces.AddUnique(projectFeature.GetProject().GetEntityLayerNamespace(foreignTable.Schema));
                        }


                        if (foreignKey.Key.Count == 0)
                        {
                            lines.Add(new WarningLine(1, " There isn't definition for key in foreign key '{0}' in your current database", foreignKey.References));
                        }
                        else if (foreignKey.Key.Count == 1)
                        {
                            if (foreignTable == null)
                            {
                                lines.Add(new WarningLine(1, " There isn't definition for '{0}' in your current database", foreignKey.References));
                            }
                            else
                            {
                                var column = tableCast.Columns.FirstOrDefault(item => item.Name == foreignKey.Key[0]);

                                var x = NamingConvention.GetPropertyName(foreignKey.Key[0]);
                                var y = NamingConvention.GetPropertyName(foreignTable.PrimaryKey.Key[0]);

                                if (column.Nullable)
                                {
                                    lines.Add(new CodeLine(1, "join {0}Join in DbContext.Set<{1}>() on {2}.{3} equals {0}Join.{4} into {0}Temp", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                                    lines.Add(new CodeLine(2, "from {0} in {0}Temp.Where(relation => relation.{2} == {1}.{3}).DefaultIfEmpty()", foreignKeyAlias, entityAlias, x, y));
                                }
                                else
                                {
                                    lines.Add(new CodeLine(1, "join {0} in DbContext.Set<{1}>() on {2}.{3} equals {0}.{4}", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                                }
                            }
                        }
                        else
                        {
                            // todo: add logic for foreign key with multiple key
                            lines.Add(new WarningLine(1, "// todo: add logic for foreign key with multiple key"));
                        }
                    }

                    lines.Add(new CodeLine(1, "select new {0}", returnType));
                    lines.Add(new CodeLine(1, "{{"));

                    for (var i = 0; i < dataContractPropertiesSets.Count; i++)
                    {
                        var property = dataContractPropertiesSets[i];

                        if (String.IsNullOrEmpty(property.ObjectSource) && String.IsNullOrEmpty(property.Target))
                        {
                            continue;
                        }

                        if (property.IsForeign)
                        {
                            if (property.Nullable)
                            {
                                if (property.Type.Contains("char"))
                                {
                                    lines.Add(new CodeLine(2, "{0} = {1} == null ? String.Empty : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                                }
                                else if (property.Type.Contains("date"))
                                {
                                    lines.Add(new CodeLine(2, "{0} = {1} == null ? default(DateTime?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                                }
                                else if (property.Type.Contains("smallint"))
                                {
                                    lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int16?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                                }
                                else if (property.Type.Contains("bigint"))
                                {
                                    lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int64?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                                }
                                else if (property.Type.Contains("int"))
                                {
                                    lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int32?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                                }
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else
                        {
                            lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                    }

                    lines.Add(new CodeLine(1, "}};"));
                    lines.Add(new CodeLine());
                }
                else
                {
                    returnType = dbObject.GetSingularName();

                    lines.Add(new CommentLine(" Get query from DbSet"));
                    lines.Add(new CodeLine("var query = DbContext.Set<{0}>().AsQueryable();", dbObject.GetSingularName()));
                    lines.Add(new CodeLine());

                }
            }

            var parameters = new List<ParameterDefinition>()
            {
                new ParameterDefinition("Int32", "pageSize", "10"),
                new ParameterDefinition("Int32", "pageNumber", "1")
            };

            if (tableCast != null)
            {
                if (tableCast.ForeignKeys.Count == 0)
                {
                    lines.Add(new CodeLine("return query.Paging(pageSize, pageNumber);"));
                }
                else
                {
                    var typeResolver = new ClrTypeResolver();

                    for (var i = 0; i < tableCast.ForeignKeys.Count; i++)
                    {
                        var foreignKey = tableCast.ForeignKeys[i];

                        if (foreignKey.Key.Count == 1)
                        {
                            var column = tableCast.Columns.First(item => item.Name == foreignKey.Key[0]);

                            var parameterName = NamingConvention.GetParameterName(column.Name);

                            parameters.Add(new ParameterDefinition(typeResolver.Resolve(column.Type), parameterName, "null"));

                            if (column.IsString())
                            {
                                lines.Add(new CodeLine("if (!String.IsNullOrEmpty({0}))", NamingConvention.GetParameterName(column.Name)));
                                lines.Add(new CodeLine("{{"));
                                lines.Add(new CommentLine(1, " Filter by: '{0}'", column.Name));
                                lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                                lines.Add(new CodeLine("}}"));
                                lines.Add(new CodeLine());
                            }
                            else
                            {
                                lines.Add(new CodeLine("if ({0}.HasValue)", NamingConvention.GetParameterName(column.Name)));
                                lines.Add(new CodeLine("{{"));
                                lines.Add(new CommentLine(1, " Filter by: '{0}'", column.Name));
                                lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                                lines.Add(new CodeLine("}}"));
                                lines.Add(new CodeLine());
                            }
                        }
                        else
                        {
                            // todo: add logic for composed foreign key
                            lines.Add(new WarningLine(1, "// todo: add logic for foreign key with multiple key"));
                        }
                    }

                    lines.Add(new CodeLine("return query.Paging(pageSize, pageNumber);"));
                }
            }

            return new MethodDefinition(String.Format("IQueryable<{0}>", returnType), dbObject.GetGetAllMethodName(), parameters.ToArray())
            {
                Lines = lines
            };
        }

        public void AddGetByUniqueMethods(ProjectFeature projectFeature, IDbObject dbObject)
        {
            var table = dbObject as ITable;

            if (table == null)
            {
                table = projectFeature.Project.Database.FindTableBySchemaAndName(dbObject.FullName);
            }

            if (table == null)
            {
                return;
            }

            foreach (var unique in table.Uniques)
            {
                var expression = String.Format("item => {0}", String.Join(" && ", unique.Key.Select(item => String.Format("item.{0} == entity.{0}", NamingConvention.GetPropertyName(item)))));

                Methods.Add(new MethodDefinition(String.Format("Task<{0}>", dbObject.GetSingularName()), dbObject.GetGetByUniqueMethodName(unique), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
                {
                    IsAsync = true,
                    Lines = new List<ILine>()
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", ProjectFeature.GetProject().Settings.DeclareDbSetPropertiesInDbContext ? dbObject.GetPluralName() : String.Format("Set<{0}>()", dbObject.GetSingularName()), expression)
                    }
                });
            }
        }

        public MethodDefinition GetGetMethod(ProjectFeature projectFeature, IDbObject dbObject)
        {
            var expression = String.Empty;
            var table = projectFeature.Project.Database.FindTableBySchemaAndName(dbObject.FullName);

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

            return new MethodDefinition(String.Format("Task<{0}>", dbObject.GetSingularName()), dbObject.GetGetMethodName(), new ParameterDefinition(dbObject.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<ILine>()
                {
                    new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", ProjectFeature.GetProject().Settings.DeclareDbSetPropertiesInDbContext ? dbObject.GetPluralName() : String.Format("Set<{0}>()", dbObject.GetSingularName()), expression)
                }
            };
        }

        public MethodDefinition GetAddMethod(ProjectFeature projectFeature, Table table)
        {
            var lines = new List<ILine>();

            if (table.IsPrimaryKeyGuid())
            {
                lines.Add(new CommentLine(" Set value for GUID"));
                lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", NamingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Add entity in DbSet"));
            lines.Add(new CodeLine("Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Save changes through DbContext"));
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", table.GetAddMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetUpdateMethod(ProjectFeature projectFeature, Table table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Update entity in DbSet"));
            lines.Add(new CodeLine("Update(changes);"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Save changes through DbContext"));
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", table.GetUpdateMethodName(), new ParameterDefinition(table.GetSingularName(), "changes"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetRemoveMethod(ProjectFeature projectFeature, Table table)
        {
            return new MethodDefinition("Task<Int32>", table.GetRemoveMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<ILine>()
                {
                    new CommentLine(" Remove entity from DbSet"),
                    new CodeLine("Remove(entity);"),
                    new CodeLine(),
                    new CommentLine(" Save changes through DbContext"),
                    new CodeLine("return await CommitChangesAsync();")
                }
            };
        }
    }
}
