using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Collections;
using CatFactory.NetCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore.Definitions.Extensions
{
    public static class DbContextQueryExtensionsClassBuilder
    {
        public static DbContextQueryExtensionsClassDefinition GetDbContextQueryExtensionsClassDefinition(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var definition = new DbContextQueryExtensionsClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.Threading.Tasks",
                    "Microsoft.EntityFrameworkCore",
                    project.GetDomainModelsNamespace()
                },
                Namespace = project.Name,
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                Name = projectFeature.GetQueryExtensionsClassName()
            };

            foreach (var table in projectFeature.GetTables())
            {
                var selection = projectFeature.GetEntityFrameworkCoreProject().GetSelection(table);

                if (!project.Database.HasDefaultSchema(table))
                    definition.Namespaces.AddUnique(project.GetDomainModelsNamespace(table.Schema));

                if (selection.Settings.EntitiesWithDataContracts)
                {
                    definition.Namespaces.AddUnique(project.GetDomainQueryModelsNamespace());
                }

                definition.AddGetAllMethod(projectFeature, selection, table);

                if (table.PrimaryKey != null)
                    definition.Methods.Add(GetGetMethod(projectFeature, selection, table));

                foreach (var unique in table.Uniques)
                {
                    definition.Methods.Add(GetGetByUniqueMethods(projectFeature, table, unique));
                }

                if (selection.Settings.SimplifyDataTypes)
                    definition.SimplifyDataTypes();
            }

            return definition;
        }

        private static void AddGetAllMethod(this CSharpClassDefinition definition, ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var returnType = string.Empty;

            var lines = new List<ILine>();

            if (projectSelection.Settings.EntitiesWithDataContracts)
            {
                returnType = project.GetQueryModelName(table);

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

                var entityAlias = NamingConvention.GetCamelCase(project.GetEntityName(table));

                foreach (var column in table.Columns)
                {
                    var propertyName = project.GetPropertyName(table, column);

                    dataContractPropertiesSets.Add(new
                    {
                        IsForeign = false,
                        column.Type,
                        column.Nullable,
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

                    var foreignKeyAlias = NamingConvention.GetCamelCase(project.GetEntityName(foreignTable));

                    foreach (var column in foreignTable?.GetColumnsWithNoPrimaryKey())
                    {
                        var col = (Column)column;

                        var propertyName = project.GetPropertyName(foreignTable, col);

                        if (dataContractPropertiesSets.Where(item => string.Format("{0}.{1}", item.ObjectSource, item.PropertySource) == string.Format("{0}.{1}", entityAlias, propertyName)).Count() == 0)
                        {
                            var target = string.Format("{0}{1}", project.GetEntityName(foreignTable), propertyName);

                            dataContractPropertiesSets.Add(new
                            {
                                IsForeign = true,
                                column.Type,
                                column.Nullable,
                                ObjectSource = foreignKeyAlias,
                                PropertySource = propertyName,
                                Target = target
                            });
                        }
                    }
                }

                lines.Add(new CommentLine(" Get query from DbSet"));
                lines.Add(new CodeLine("var query = from {0} in dbContext.{1}", entityAlias, project.GetDbSetPropertyName(table, projectSelection.Settings.PluralizeDbSetPropertyNames)));

                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = projectFeature.Project.Database.FindTable(foreignKey.References);

                    if (foreignTable == null)
                        continue;

                    var foreignKeyEntityName = project.GetDbSetPropertyName(foreignTable, projectSelection.Settings.PluralizeDbSetPropertyNames);

                    var foreignKeyAlias = NamingConvention.GetCamelCase(project.GetEntityName(foreignTable));

                    if (projectFeature.Project.Database.HasDefaultSchema(foreignTable))
                        definition.Namespaces.AddUnique(project.GetDomainModelsNamespace());
                    else
                        definition.Namespaces.AddUnique(project.GetDomainModelsNamespace(foreignTable.Schema));

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

                            var x = project.CodeNamingConvention.GetPropertyName(foreignKey.Key.First());
                            var y = project.CodeNamingConvention.GetPropertyName(foreignTable.PrimaryKey.Key.First());

                            if (column.Nullable)
                            {
                                lines.Add(new CodeLine(1, "join {0}Join in dbContext.{1} on {2}.{3} equals {0}Join.{4} into {0}Temp", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
                                lines.Add(new CodeLine(2, "from {0} in {0}Temp.DefaultIfEmpty()", foreignKeyAlias, entityAlias, x, y));
                            }
                            else
                            {
                                lines.Add(new CodeLine(1, "join {0} in dbContext.{1} on {2}.{3} equals {0}.{4}", foreignKeyAlias, foreignKeyEntityName, entityAlias, x, y));
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
                        // todo: Add extension method to retrieve database type map by name

                        var dbTypeMap = projectFeature.Project.Database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == property.Type);

                        if (dbTypeMap == null)
                            throw new ObjectRelationMappingException(string.Format("There isn't mapping for '{0}' type", property.Type));

                        var clrType = dbTypeMap.GetClrType();

                        if (clrType.FullName == typeof(byte[]).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(byte[]) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(bool).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(bool?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(string).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? string.Empty : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(DateTime).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(DateTime?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(TimeSpan).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(TimeSpan?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(byte).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(byte?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(short).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(short?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(int).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(int?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(long).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(long?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(decimal).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(decimal?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(double).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(double?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(float).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(float?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else if (clrType.FullName == typeof(Guid).FullName)
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(Guid?) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                        else
                            lines.Add(new CodeLine(2, "{0} = {1} == null ? default(object) : {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                    }
                    else
                    {
                        lines.Add(new CodeLine(2, "{0} = {1}.{2},", property.Target, property.ObjectSource, property.PropertySource));
                    }
                }

                lines.Add(new CodeLine(1, "};"));
                lines.Add(new EmptyLine());
            }
            else
            {
                returnType = project.GetEntityName(table);

                var existingViews = project.Database.Views.Count(item => item.Name == table.Name);

                returnType = existingViews == 0 ? project.GetEntityName(table) : project.GetFullEntityName(table);

                var dbSetName = existingViews == 0 ? project.GetDbSetPropertyName(table, projectSelection.Settings.PluralizeDbSetPropertyNames) : project.GetFullDbSetPropertyName(table);

                lines.Add(new CommentLine(" Get query from DbSet"));
                lines.Add(new CodeLine("var query = dbContext.{0}.AsQueryable();", dbSetName));

                lines.Add(new EmptyLine());
            }

            var parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition(project.GetDbContextName(project.Database), "dbContext")
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
                        var column = table.Columns.First(item => item.Name == foreignKey.Key.First());

                        var parameterName = project.GetParameterName(column);

                        parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(column), parameterName, "null"));

                        if (projectFeature.Project.Database.ColumnIsDateTime(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0}.HasValue)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.GetPropertyName(table, column), parameterName));
                            lines.Add(new EmptyLine());
                        }
                        else if (projectFeature.Project.Database.ColumnIsNumber(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0}.HasValue)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.GetPropertyName(table, column), parameterName));
                            lines.Add(new EmptyLine());
                        }
                        else if (projectFeature.Project.Database.ColumnIsString(column))
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if (!string.IsNullOrEmpty({0}))", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.GetPropertyName(table, column), parameterName));
                            lines.Add(new EmptyLine());
                        }
                        else
                        {
                            lines.Add(new CommentLine(" Filter by: '{0}'", column.Name));
                            lines.Add(new CodeLine("if ({0} != null)", parameterName));
                            lines.Add(new CodeLine(1, "query = query.Where(item => item.{0} == {1});", project.GetPropertyName(table, column), parameterName));
                            lines.Add(new EmptyLine());
                        }
                    }
                    else
                    {
                        lines.Add(LineHelper.Warning("Add logic for foreign key with multiple key"));
                    }
                }

                lines.Add(new CodeLine("return query;"));
            }

            definition.Methods.Add(new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                Type = string.Format("IQueryable<{0}>", returnType),
                Name = project.GetGetAllExtensionMethodName(table),
                IsExtension = true,
                Parameters = parameters,
                Lines = lines
            });
        }

        private static MethodDefinition GetGetMethod(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ProjectSelection<EntityFrameworkCoreProjectSettings> projectSelection, ITable table)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var expression = string.Empty;

            if (table.Identity == null)
                expression = string.Format("item => {0}", string.Join(" && ", table.PrimaryKey.Key.Select(item => string.Format("item.{0} == entity.{0}", project.CodeNamingConvention.GetPropertyName(item)))));
            else
                expression = string.Format("item => item.{0} == entity.{0}", project.CodeNamingConvention.GetPropertyName(table.Identity.Name));

            var existingViews = project.Database.Views.Count(item => item.Name == table.Name);

            var genericTypeName = existingViews == 0 ? project.GetEntityName(table) : project.GetFullEntityName(table);
            var dbSetName = existingViews == 0 ? project.GetDbSetPropertyName(table, projectSelection.Settings.PluralizeDbSetPropertyNames) : project.GetFullDbSetPropertyName(table);

            var includeExpression = new List<string>();

            if (projectSelection.Settings.DeclareNavigationProperties)
            {
                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTable(foreignKey.References);

                    if (foreignKey == null)
                        continue;

                    includeExpression.Add(string.Format("Include(e => e.{0})", foreignKey.GetParentNavigationProperty(foreignTable, project).Name));
                }
            }

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                IsAsync = true,
                Type = string.Format("Task<{0}>", project.GetEntityName(table)),
                Name = project.GetGetRepositoryMethodName(table),
                IsExtension = true,
                Parameters =
                {
                    new ParameterDefinition(project.GetDbContextName(project.Database), "dbContext"),
                    new ParameterDefinition(project.GetEntityName(table), "entity"),
                    new ParameterDefinition("bool", "tracking", "true"),
                    new ParameterDefinition("bool", "include", "true")
                },
                Lines =
                {
                    new CodeLine("var query = dbContext.{0}.AsQueryable();", dbSetName),
                    new EmptyLine(),
                    new CodeLine("if (!tracking)"),
                    new CodeLine(1, "query = query.AsNoTracking();"),
                    new EmptyLine(),
                    new CodeLine("if (include)"),
                    new CodeLine(1, "query = query.{0};", string.Join(".", includeExpression)),
                    new EmptyLine(),
                    new CodeLine("return await query.FirstOrDefaultAsync({1});", dbSetName, expression)
                }
            };
        }

        private static MethodDefinition GetGetByUniqueMethods(ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature, ITable table, Unique unique)
        {
            var project = projectFeature.GetEntityFrameworkCoreProject();

            var selection = project.GetSelection(table);

            var expression = string.Format("item => {0}", string.Join(" && ", unique.Key.Select(item => string.Format("item.{0} == entity.{0}", project.CodeNamingConvention.GetPropertyName(item)))));

            var existingViews = project.Database.Views.Count(item => item.Name == table.Name);

            var genericTypeName = existingViews == 0 ? project.GetEntityName(table) : project.GetFullEntityName(table);
            var dbSetName = existingViews == 0 ? project.GetDbSetPropertyName(table, selection.Settings.PluralizeDbSetPropertyNames) : project.GetFullDbSetPropertyName(table);

            var includeExpression = new List<string>();

            if (selection.Settings.DeclareNavigationProperties)
            {
                foreach (var foreignKey in table.ForeignKeys)
                {
                    var foreignTable = project.Database.FindTable(foreignKey.References);

                    if (foreignKey == null)
                        continue;

                    includeExpression.Add(string.Format("Include(e => e.{0})", foreignKey.GetParentNavigationProperty(foreignTable, project).Name));
                }
            }

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                IsStatic = true,
                IsAsync = true,
                Type = string.Format("Task<{0}>", project.GetEntityName(table)),
                Name = project.GetGetByUniqueRepositoryMethodName(table, unique),
                IsExtension = true,
                Parameters =
                {
                    new ParameterDefinition(project.GetDbContextName(project.Database), "dbContext"),
                    new ParameterDefinition(project.GetEntityName(table), "entity"),
                    new ParameterDefinition("bool", "tracking", "true"),
                    new ParameterDefinition("bool", "include", "true")
                },
                Lines =
                {
                    new CodeLine("var query = dbContext.{0}.AsQueryable();", dbSetName),
                    new EmptyLine(),
                    new CodeLine("if (!tracking)"),
                    new CodeLine(1, "query = query.AsNoTracking();"),
                    new EmptyLine(),
                    new CodeLine("if (include)"),
                    new CodeLine(1, "query = query.{0};", string.Join(".", includeExpression)),
                    new EmptyLine(),
                    new CodeLine("return await query.FirstOrDefaultAsync({1});", dbSetName, expression)
                }
            };
        }
    }
}
