using System.Collections.Generic;
using System.Linq;
using CatFactory.DotNetCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public static class IDotNetClassDefinitionExtensions
    {
        public static void AddDataAnnotations(this IDotNetClassDefinition classDefinition, ITable table, EntityFrameworkCoreProject project)
        {
            classDefinition.Attributes.Add(new MetadataAttribute("Table", string.Format("\"{0}\"", table.Name))
            {
                Sets = new List<MetadataAttributeSet>
                {
                    new MetadataAttributeSet("Schema", string.Format("\"{0}\"", table.Schema))
                }
            });

            var selection = project.GetSelection(table);

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                foreach (var property in classDefinition.Properties)
                {
                    if (column.GetPropertyName() != property.Name)
                        continue;

                    if (table.Identity?.Name == column.Name)
                        property.Attributes.Add(new MetadataAttribute("DatabaseGenerated", "DatabaseGeneratedOption.Identity"));

                    if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                        property.Attributes.Add(new MetadataAttribute("Key"));

                    property.Attributes.Add(new MetadataAttribute("Column", string.Format("\"{0}\"", column.Name)));

                    if (!column.Nullable && table.Identity != null && table.Identity.Name != column.Name)
                        property.Attributes.Add(new MetadataAttribute("Required"));

                    if (project.Database.ColumnIsString(column) && column.Length > 0)
                        property.Attributes.Add(new MetadataAttribute("StringLength", column.Length.ToString()));

                    if (!string.IsNullOrEmpty(selection.Settings.ConcurrencyToken) && selection.Settings.ConcurrencyToken == column.Name)
                        property.Attributes.Add(new MetadataAttribute("Timestamp"));
                }
            }
        }

        public static void AddDataAnnotations(this IDotNetClassDefinition classDefinition, IView view, EntityFrameworkCoreProject project)
        {
            classDefinition.Attributes.Add(new MetadataAttribute("Table", string.Format("\"{0}\"", view.Name))
            {
                Sets = new List<MetadataAttributeSet>
                {
                    new MetadataAttributeSet("Schema", string.Format("\"{0}\"", view.Schema))
                }
            });

            var primaryKeys = project
                .Database
                .Tables
                .Where(item => item.PrimaryKey != null)
                .Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(c => c.Name).First())
                .ToList();

            var result = view.Columns.Where(item => primaryKeys.Contains(item.Name)).ToList();

            for (var i = 0; i < view.Columns.Count; i++)
            {
                var column = view.Columns[i];

                foreach (var property in classDefinition.Properties)
                {
                    if (column.GetPropertyName() != property.Name)
                        continue;

                    property.Attributes.Add(new MetadataAttribute("Column", string.Format("\"{0}\"", column.Name))
                    {
                        Sets = new List<MetadataAttributeSet>
                        {
                            new MetadataAttributeSet("Order", (i + 1).ToString())
                        }
                    });

                    if (!column.Nullable && primaryKeys.Contains(column.Name))
                        property.Attributes.Add(new MetadataAttribute("Key"));

                    if (!column.Nullable)
                        property.Attributes.Add(new MetadataAttribute("Required"));
                }
            }
        }
    }
}
