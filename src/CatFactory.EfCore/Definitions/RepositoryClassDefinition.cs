using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore.Definitions
{
    public static class RepositoryClassDefinition
    {
        public static CSharpClassDefinition GetRepositoryClassDefinition(this ProjectFeature projectFeature)
        {
            var entityFrameworkCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var classDefinition = new CSharpClassDefinition();

            classDefinition.Namespaces.Add("System");
            classDefinition.Namespaces.Add("System.Linq");
            classDefinition.Namespaces.Add("System.Threading.Tasks");
            classDefinition.Namespaces.Add("Microsoft.EntityFrameworkCore");

            foreach (var table in entityFrameworkCoreProject.Database.Tables)
            {
                classDefinition.Namespaces.AddUnique(table.HasDefaultSchema() ? entityFrameworkCoreProject.GetEntityLayerNamespace() : entityFrameworkCoreProject.GetEntityLayerNamespace(table.Schema));

                classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetDataLayerContractsNamespace());
            }

            classDefinition.Namespace = entityFrameworkCoreProject.GetDataLayerRepositoriesNamespace();

            classDefinition.Name = projectFeature.GetClassRepositoryName();

            classDefinition.BaseClass = "Repository";

            classDefinition.Implements.Add(projectFeature.GetInterfaceRepositoryName());

            classDefinition.Constructors.Add(new ClassConstructorDefinition(new ParameterDefinition(projectFeature.Project.Database.GetDbContextName(), "dbContext"))
            {
                Invocation = "base(dbContext)"
            });

            var tables = projectFeature
                .Project
                .Database
                .Tables
                .Where(item => projectFeature.DbObjects.Select(dbo => dbo.FullName).Contains(item.FullName))
                .ToList();

            foreach (var table in tables)
            {
                if (entityFrameworkCoreProject.Settings.EntitiesWithDataContracts.Contains(table.FullName))
                {
                    classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetDataLayerDataContractsNamespace());
                }

                foreach (var foreignKey in table.ForeignKeys)
                {
                    if (string.IsNullOrEmpty(foreignKey.Child))
                    {
                        var child = projectFeature.Project.Database.FindTableBySchemaAndName(foreignKey.Child);

                        if (child != null)
                        {
                            classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetDataLayerDataContractsNamespace());
                        }
                    }
                }

                classDefinition.GetGetAllMethod(projectFeature, table);

                if (table.PrimaryKey != null)
                {
                    classDefinition.Methods.Add(GetGetMethod(projectFeature, table));
                }

                foreach (var unique in table.Uniques)
                {
                    classDefinition.Methods.Add(GetGetByUniqueMethods(projectFeature, table, unique));
                }

                classDefinition.Methods.Add(GetAddMethod(projectFeature, table));

                if (table.PrimaryKey != null)
                {
                    classDefinition.Methods.Add(GetUpdateMethod(projectFeature, table));
                    classDefinition.Methods.Add(GetRemoveMethod(projectFeature, table));
                }
            }




















            var views = projectFeature
                .Project
                .Database
                .Views
                .Where(item => projectFeature.DbObjects.Select(dbo => dbo.FullName).Contains(item.FullName))
                .ToList();

            foreach (var view in views)
            {
                if (entityFrameworkCoreProject.Settings.EntitiesWithDataContracts.Contains(view.FullName))
                {
                    classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetDataLayerDataContractsNamespace());
                }

                classDefinition.GetGetAllMethod(projectFeature, view);
            }

            return classDefinition;
        }

        private static void GetGetAllMethod(this CSharpClassDefinition classDefinition, ProjectFeature projectFeature, ITable table)
        {
            var entityFrameworkCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var returnType = string.Empty;

            var lines = new List<ILine>();

            if (entityFrameworkCoreProject.Settings.EntitiesWithDataContracts.Contains(table.FullName))
            {
                var entityAlias = NamingConvention.GetCamelCase(table.GetEntityName());

                returnType = table.GetDataContractName();

                var dataContractPropertiesSets = new[]
                {
                    new
                    {
                        IsForeign = false,
                        Type = string.Empty,
                        Nullable = false,
                        ObjectSource = string.Empty,
                        PropertySource = string.Empty,
                        Target = string.Empty
                    }
                }.ToList();

                foreach (var column in table.Columns)
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

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }

                    var foreignKeyAlias = NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                    foreach (var column in foreignTable?.GetColumnsWithOutPrimaryKey())
                    {
                        if (dataContractPropertiesSets.Where(item => string.Format("{0}.{1}", item.ObjectSource, item.PropertySource) == string.Format("{0}.{1}", entityAlias, column.GetPropertyName())).Count() == 0)
                        {
                            var target = string.Format("{0}{1}", foreignTable.GetEntityName(), column.GetPropertyName());

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
                lines.Add(new CodeLine("var query = from {0} in DbContext.Set<{1}>()", entityAlias, table.GetEntityName()));

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                    if (foreignTable == null)
                    {
                        continue;
                    }

                    var foreignKeyEntityName = foreignTable.GetEntityName();

                    var foreignKeyAlias = NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                    if (foreignTable.HasDefaultSchema())
                    {
                        classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetEntityLayerNamespace());
                    }
                    else
                    {
                        classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetEntityLayerNamespace(foreignTable.Schema));
                    }

                    if (foreignKey.Key.Count == 0)
                    {
                        lines.Add(new PreprocessorDirectiveLine(1, " There isn't definition for key in foreign key '{0}' in your current database", foreignKey.References));
                    }
                    else if (foreignKey.Key.Count == 1)
                    {
                        if (foreignTable == null)
                        {
                            lines.Add(LineHelper.GetWarning(" There isn't definition for '{0}' in your current database", foreignKey.References));
                        }
                        else
                        {
                            var column = table.Columns.FirstOrDefault(item => item.Name == foreignKey.Key[0]);

                            var x = NamingExtensions.namingConvention.GetPropertyName(foreignKey.Key[0]);
                            var y = NamingExtensions.namingConvention.GetPropertyName(foreignTable.PrimaryKey.Key[0]);

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
                        lines.Add(LineHelper.GetWarning(" Add logic for foreign key with multiple key"));
                    }
                }

                lines.Add(new CodeLine(1, "select new {0}", returnType));
                lines.Add(new CodeLine(1, "{"));

                for (var i = 0; i < dataContractPropertiesSets.Count; i++)
                {
                    var property = dataContractPropertiesSets[i];

                    if (string.IsNullOrEmpty(property.ObjectSource) && string.IsNullOrEmpty(property.Target))
                    {
                        continue;
                    }

                    if (property.IsForeign)
                    {
                        if (property.Type == "binary" || property.Type == "image" || property.Type == "varbinary")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Byte[]) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "bit")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Boolean?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type.Contains("char") || property.Type.Contains("text"))
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? string.Empty : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type.Contains("date"))
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(DateTime?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "tinyint")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Byte?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "smallint")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int16?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "bigint")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int64?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "int")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Int32?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "decimal" || property.Type == "money" || property.Type == "smallmoney")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Decimal?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "float")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Double?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "real")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Single?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                        else if (property.Type == "uniqueidentifier")
                        {
                            if (property.Nullable)
                            {
                                lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Guid?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                            else
                            {
                                lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                            }
                        }
                    }
                    else
                    {
                        lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                    }
                }

                lines.Add(new CodeLine(1, "};"));
                lines.Add(new CodeLine());
            }
            else
            {
                returnType = table.GetSingularName();

                lines.Add(new CommentLine(" Get query from DbSet"));

                if (entityFrameworkCoreProject.Settings.DeclareDbSetPropertiesInDbContext)
                {
                    lines.Add(new CodeLine("var query = DbContext.{0}.AsQueryable();", table.GetPluralName()));
                }
                else
                {
                    lines.Add(new CodeLine("var query = DbContext.Set<{0}>().AsQueryable();", table.GetSingularName()));
                }

                lines.Add(new CodeLine());
            }

            var parameters = new List<ParameterDefinition>
            {
            };

            if (table.ForeignKeys.Count == 0)
            {
                lines.Add(new CodeLine("return query;"));
            }
            else
            {
                for (var i = 0; i < table.ForeignKeys.Count; i++)
                {
                    var foreignKey = table.ForeignKeys[i];

                    if (foreignKey.Key.Count == 1)
                    {
                        var column = table.Columns.First(item => item.Name == foreignKey.Key[0]);

                        var parameterName = NamingExtensions.namingConvention.GetParameterName(column.Name);

                        parameters.Add(new ParameterDefinition(column.GetClrType(), parameterName, "null"));

                        if (column.IsString())
                        {
                            lines.Add(new CodeLine("if (!string.IsNullOrEmpty({0}))", NamingExtensions.namingConvention.GetParameterName(column.Name)));
                            lines.Add(new CodeLine("{"));
                            lines.Add(new CommentLine(1, " Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                            lines.Add(new CodeLine("}"));
                            lines.Add(new CodeLine());
                        }
                        else
                        {
                            lines.Add(new CodeLine("if ({0}.HasValue)", NamingExtensions.namingConvention.GetParameterName(column.Name)));
                            lines.Add(new CodeLine("{"));
                            lines.Add(new CommentLine(1, " Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", column.GetPropertyName(), parameterName));
                            lines.Add(new CodeLine("}"));
                            lines.Add(new CodeLine());
                        }
                    }
                    else
                    {
                        lines.Add(LineHelper.GetWarning("Add logic for foreign key with multiple key"));
                    }
                }

                lines.Add(new CodeLine("return query;"));
            }

            classDefinition.Methods.Add(new MethodDefinition(string.Format("IQueryable<{0}>", returnType), table.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                Lines = lines
            });
        }

        private static void GetGetAllMethod(this CSharpClassDefinition classDefinition, ProjectFeature projectFeature, IView view)
        {
            var lines = new List<ILine>
            {
                new CodeLine("DbContext.Set<{0}>();", view.GetSingularName())
            };

            var parameters = new List<ParameterDefinition>
            {
            };

            classDefinition.Methods.Add(new MethodDefinition(string.Format("IQueryable<{0}>", view.GetSingularName()), view.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                Lines = lines
            });
        }

        private static MethodDefinition GetGetByUniqueMethods(ProjectFeature projectFeature, ITable table, Unique unique)
        {
            var entityFrameworkCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var expression = string.Format("item => {0}", string.Join(" && ", unique.Key.Select(item => string.Format("item.{0} == entity.{0}", NamingExtensions.namingConvention.GetPropertyName(item)))));

            return new MethodDefinition(string.Format("Task<{0}>", table.GetSingularName()), table.GetGetByUniqueRepositoryMethodName(unique), new ParameterDefinition(table.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<ILine>
                {
                    new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", entityFrameworkCoreProject.Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetSingularName()), expression)
                }
            };
        }

        private static MethodDefinition GetGetMethod(ProjectFeature projectFeature, ITable table)
        {
            var entityFrameworkCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var expression = string.Empty;

            if (table.Identity == null)
            {
                expression = string.Format("item => {0}", string.Join(" && ", table.PrimaryKey.Key.Select(item => string.Format("item.{0} == entity.{0}", NamingExtensions.namingConvention.GetPropertyName(item)))));
            }
            else
            {
                expression = string.Format("item => item.{0} == entity.{0}", NamingExtensions.namingConvention.GetPropertyName(table.Identity.Name));
            }

            if (entityFrameworkCoreProject.Settings.EntitiesWithDataContracts.Contains(table.FullName))
            {
                var lines = new List<ILine>
                {
                    new CodeLine("return await DbContext.{0}", entityFrameworkCoreProject.Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetSingularName()))
                };

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                    if (foreignKey == null)
                    {
                        continue;
                    }

                    lines.Add(new CodeLine(1, ".Include(p => p.{0})", foreignKey.GetParentNavigationProperty(entityFrameworkCoreProject, foreignTable).Name));
                }

                lines.Add(new CodeLine(1, ".FirstOrDefaultAsync({0});", expression));

                return new MethodDefinition(string.Format("Task<{0}>", table.GetSingularName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
                {
                    IsAsync = true,
                    Lines = lines
                };
            }
            else
            {
                return new MethodDefinition(string.Format("Task<{0}>", table.GetSingularName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
                {
                    IsAsync = true,
                    Lines = new List<ILine>
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", entityFrameworkCoreProject.Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetSingularName()), expression)
                    }
                };
            }
        }

        private static MethodDefinition GetAddMethod(ProjectFeature projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            if (table.IsPrimaryKeyGuid())
            {
                lines.Add(new CommentLine(" Set value for GUID"));
                lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", NamingExtensions.namingConvention.GetPropertyName(table.PrimaryKey.Key[0])));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Add entity in DbSet"));
            lines.Add(new CodeLine("Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Save changes through DbContext"));
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", table.GetAddRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethod(ProjectFeature projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Update entity in DbSet"));
            lines.Add(new CodeLine("Update(changes);"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Save changes through DbContext"));
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", table.GetUpdateRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "changes"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetRemoveMethod(ProjectFeature projectFeature, ITable table)
        {
            return new MethodDefinition("Task<Int32>", table.GetRemoveRepositoryMethodName(), new ParameterDefinition(table.GetSingularName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<ILine>
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
