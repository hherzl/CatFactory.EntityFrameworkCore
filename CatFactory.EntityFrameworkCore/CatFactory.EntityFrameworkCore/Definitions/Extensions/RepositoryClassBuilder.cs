using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.NetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class RepositoryClassBuilder
    {
        public static RepositoryClassDefinition GetRepositoryClassDefinition(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
        {
            var entityFrameworkCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var classDefinition = new RepositoryClassDefinition();

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
                var selection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(table);

                if (selection.Settings.EntitiesWithDataContracts)
                    classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetDataLayerDataContractsNamespace());

                foreach (var foreignKey in table.ForeignKeys)
                {
                    if (string.IsNullOrEmpty(foreignKey.Child))
                    {
                        var child = projectFeature.Project.Database.FindTable(foreignKey.Child);

                        if (child != null)
                            classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetDataLayerDataContractsNamespace());
                    }
                }

                classDefinition.GetGetAllMethod(projectFeature, selection, table);

                if (table.PrimaryKey != null)
                    classDefinition.Methods.Add(GetGetMethod(projectFeature, selection, table));

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
                var selection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(view);

                if (selection.Settings.EntitiesWithDataContracts)
                    classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetDataLayerDataContractsNamespace());

                classDefinition.GetGetAllMethod(projectFeature, view);
            }

            return classDefinition;
        }

        private static void GetGetAllMethod(this CSharpClassDefinition classDefinition, ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var entityFrameworkCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var returnType = string.Empty;

            var lines = new List<ILine>();

            if (projectSelection.Settings.EntitiesWithDataContracts)
            {
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

                var entityAlias = NamingConvention.GetCamelCase(table.GetEntityName());

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
                    var foreignTable = projectFeature.Project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                        continue;

                    var foreignKeyAlias = NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                    foreach (var column in foreignTable?.GetColumnsWithNoPrimaryKey())
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
                    var foreignTable = projectFeature.Project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                        continue;

                    var foreignKeyEntityName = foreignTable.GetEntityName();

                    var foreignKeyAlias = NamingConvention.GetCamelCase(foreignTable.GetEntityName());

                    if (foreignTable.HasDefaultSchema())
                        classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetEntityLayerNamespace());
                    else
                        classDefinition.Namespaces.AddUnique(entityFrameworkCoreProject.GetEntityLayerNamespace(foreignTable.Schema));

                    if (foreignKey.Key.Count == 0)
                    {
                        lines.Add(new PreprocessorDirectiveLine(1, " There isn't definition for key in foreign key '{0}' in your current database", foreignKey.References));
                    }
                    else if (foreignKey.Key.Count == 1)
                    {
                        if (foreignTable == null)
                        {
                            lines.Add(LineHelper.Warning(" There isn't definition for '{0}' in your current database", foreignKey.References));
                        }
                        else
                        {
                            var column = table.Columns.FirstOrDefault(item => item.Name == foreignKey.Key.First());

                            var x = NamingExtensions.namingConvention.GetPropertyName(foreignKey.Key.First());
                            var y = NamingExtensions.namingConvention.GetPropertyName(foreignTable.PrimaryKey.Key.First());

                            if (column.Nullable)
                            {
                                lines.Add(new CodeLine(1, "join {0}Join in DbContext.Set<{1}>() on {2}.{3} equals {0}Join.{4} into {0}Temp", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                                lines.Add(new CodeLine(2, "from {0} in {0}Temp.DefaultIfEmpty()", foreignKeyAlias, entityAlias, x, y));
                            }
                            else
                            {
                                lines.Add(new CodeLine(1, "join {0} in DbContext.Set<{1}>() on {2}.{3} equals {0}.{4}", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                            }
                        }
                    }
                    else
                    {
                        lines.Add(LineHelper.Warning(" Add logic for foreign key with multiple key"));
                    }
                }

                lines.Add(new CodeLine(1, "select new {0}", returnType));
                lines.Add(new CodeLine(1, "{"));

                for (var i = 0; i < dataContractPropertiesSets.Count; i++)
                {
                    var property = dataContractPropertiesSets[i];

                    if (string.IsNullOrEmpty(property.ObjectSource) && string.IsNullOrEmpty(property.Target))
                        continue;

                    if (property.IsForeign)
                    {
                        if (property.Type == "binary" || property.Type == "image" || property.Type == "varbinary")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Byte[]) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "bit")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(bool?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type.Contains("char") || property.Type.Contains("text"))
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? string.Empty : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type.Contains("date"))
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(DateTime?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "tinyint")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Byte?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "smallint")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(short?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "bigint")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(short?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "int")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(int?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "decimal" || property.Type == "money" || property.Type == "smallmoney")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(decimal?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "float")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(double?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "real")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(float?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        }
                        else if (property.Type == "uniqueidentifier")
                        {
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Guid?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
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
                returnType = table.GetEntityName();

                lines.Add(new CommentLine(" Get query from DbSet"));

                if (projectSelection.Settings.DeclareDbSetPropertiesInDbContext)
                    lines.Add(new CodeLine("var query = DbContext.{0}.AsQueryable();", table.GetPluralName()));
                else
                    lines.Add(new CodeLine("var query = DbContext.Set<{0}>().AsQueryable();", table.GetEntityName()));

                lines.Add(new CodeLine());
            }

            var parameters = new List<ParameterDefinition> { };

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
                        var column = table.Columns.First(item => item.Name == foreignKey.Key.First());

                        var parameterName = NamingExtensions.namingConvention.GetParameterName(column.Name);

                        parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveType(column), parameterName, "null"));

                        if (projectFeature.Project.Database.ColumnIsString(column))
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
                        lines.Add(LineHelper.Warning("Add logic for foreign key with multiple key"));
                    }
                }

                lines.Add(new CodeLine("return query;"));
            }

            classDefinition.Methods.Add(new MethodDefinition(string.Format("IQueryable<{0}>", returnType), table.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                Lines = lines
            });
        }

        private static void GetGetAllMethod(this CSharpClassDefinition classDefinition, ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, IView view)
        {
            var lines = new List<ILine>
            {
                new CodeLine("DbContext.Set<{0}>();", view.GetEntityName())
            };

            var parameters = new List<ParameterDefinition> { };

            classDefinition.Methods.Add(new MethodDefinition(string.Format("IQueryable<{0}>", view.GetEntityName()), view.GetGetAllRepositoryMethodName(), parameters.ToArray())
            {
                Lines = lines
            });
        }

        private static MethodDefinition GetGetByUniqueMethods(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ITable table, Unique unique)
        {
            var entityFrameworkCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var selection = entityFrameworkCoreProject.GetSelection(table);

            var expression = string.Format("item => {0}", string.Join(" && ", unique.Key.Select(item => string.Format("item.{0} == entity.{0}", NamingExtensions.namingConvention.GetPropertyName(item)))));

            return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetByUniqueRepositoryMethodName(unique), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = new List<ILine>
                {
                    new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", selection.Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetEntityName()), expression)
                }
            };
        }

        private static MethodDefinition GetGetMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var entityFrameworkCoreProject = projectFeature.GetEntityFrameworkCoreProject();

            var expression = string.Empty;

            if (table.Identity == null)
                expression = string.Format("item => {0}", string.Join(" && ", table.PrimaryKey.Key.Select(item => string.Format("item.{0} == entity.{0}", NamingExtensions.namingConvention.GetPropertyName(item)))));
            else
                expression = string.Format("item => item.{0} == entity.{0}", NamingExtensions.namingConvention.GetPropertyName(table.Identity.Name));

            if (projectSelection.Settings.EntitiesWithDataContracts)
            {
                var lines = new List<ILine>
                {
                    new CodeLine("return await DbContext.{0}", projectSelection.Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetEntityName()))
                };

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTable(foreignKey.References);

                    if (foreignKey == null)
                        continue;

                    lines.Add(new CodeLine(1, ".Include(p => p.{0})", foreignKey.GetParentNavigationProperty(foreignTable, entityFrameworkCoreProject).Name));
                }

                lines.Add(new CodeLine(1, ".FirstOrDefaultAsync({0});", expression));

                return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
                {
                    IsAsync = true,
                    Lines = lines
                };
            }
            else
            {
                return new MethodDefinition(string.Format("Task<{0}>", table.GetEntityName()), table.GetGetRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
                {
                    IsAsync = true,
                    Lines = new List<ILine>
                    {
                        new CodeLine("return await DbContext.{0}.FirstOrDefaultAsync({1});", projectSelection .Settings.DeclareDbSetPropertiesInDbContext ? table.GetPluralName() : string.Format("Set<{0}>()", table.GetEntityName()), expression)
                    }
                };
            }
        }

        private static MethodDefinition GetAddMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            if (projectFeature.Project.Database.PrimaryKeyIsGuid(table))
            {
                lines.Add(new CommentLine(" Set value for GUID"));
                lines.Add(new CodeLine("entity.{0} = Guid.NewGuid();", NamingExtensions.namingConvention.GetPropertyName(table.PrimaryKey.Key.First())));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Add entity in DbSet"));
            lines.Add(new CodeLine("Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Save changes through DbContext"));
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", table.GetAddRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetUpdateMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            lines.Add(new CommentLine(" Update entity in DbSet"));
            lines.Add(new CodeLine("Update(changes);"));
            lines.Add(new CodeLine());
            lines.Add(new CommentLine(" Save changes through DbContext"));
            lines.Add(new CodeLine("return await CommitChangesAsync();"));

            return new MethodDefinition("Task<Int32>", table.GetUpdateRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "changes"))
            {
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetRemoveMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ITable table)
        {
            return new MethodDefinition("Task<Int32>", table.GetRemoveRepositoryMethodName(), new ParameterDefinition(table.GetEntityName(), "entity"))
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
