using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public static class IDotNetClassDefinitionExtensions
{
    public static void AddDataAnnotations(this IDotNetClassDefinition classDefinition, ITable table, DomainProject project)
    {
        classDefinition.Attributes.Add(new MetadataAttribute("Table", $"\"{table.Name}\"")
        {
            Sets =
            {
                new MetadataAttributeSet("Schema", $"\"{table.Schema}\"")
            }
        });

        var selection = project.GetSelection(table);

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];

            foreach (var prop in classDefinition.Properties)
            {
                if (project.GetPropName(table, column) != prop.Name)
                    continue;

                if (table.Identity?.Name == column.Name)
                    prop.Attributes.Add(new MetadataAttribute("DatabaseGenerated", "DatabaseGeneratedOption.Identity"));

                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                    prop.Attributes.Add(new MetadataAttribute("Key"));

                if (prop.Name == column.Name)
                    prop.Attributes.Add(new MetadataAttribute("Column"));
                else
                    prop.Attributes.Add(new MetadataAttribute("Column", $"\"{column.Name}\""));

                if (!column.Nullable && table.Identity != null && table.Identity.Name != column.Name)
                    prop.Attributes.Add(new MetadataAttribute("Required"));

                if (project.Database.ColumnIsString(column) && column.Length > 0)
                    prop.Attributes.Add(new MetadataAttribute("StringLength", column.Length.ToString()));

                if (!string.IsNullOrEmpty(selection.Settings.ConcurrencyToken) && selection.Settings.ConcurrencyToken == column.Name)
                    prop.Attributes.Add(new MetadataAttribute("Timestamp"));
            }
        }
    }

    public static void AddDataAnnotations(this IDotNetClassDefinition classDefinition, IView view, DomainProject project)
    {
        classDefinition.Attributes.Add(new MetadataAttribute("Table", $"\"{view.Name}\"")
        {
            Sets =
            {
                new MetadataAttributeSet("Schema", $"\"{view.Schema}\"")
            }
        });

        var primaryKeys = project
            .Database
            .Tables
            .Where(item => item.PrimaryKey != null)
            .Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(column => column.Name).First())
            .ToList();

        var result = view.Columns
            .Where(item => primaryKeys.Contains(item.Name))
            .ToList();

        for (var i = 0; i < view.Columns.Count; i++)
        {
            var column = view.Columns[i];

            foreach (var prop in classDefinition.Properties)
            {
                if (project.GetPropName(view, column) != prop.Name)
                    continue;

                if (prop.Name == column.Name)
                {
                    prop.Attributes.Add(new MetadataAttribute("Column")
                    {
                        Sets = { new MetadataAttributeSet("Order", (i + 1).ToString()) }
                    });
                }
                else
                {
                    prop.Attributes.Add(new MetadataAttribute("Column", $"\"{column.Name}\"")
                    {
                        Sets =
                        {
                            new MetadataAttributeSet("Order", (i + 1).ToString())
                        }
                    });
                }

                if (!column.Nullable && primaryKeys.Contains(column.Name))
                    prop.Attributes.Add(new MetadataAttribute("Key"));

                if (!column.Nullable)
                    prop.Attributes.Add(new MetadataAttribute("Required"));
            }
        }
    }

    public static void AddDataAnnotations(this IDotNetClassDefinition classDefinition, ITable table, EntityFrameworkCoreProject project)
    {
        classDefinition.Attributes.Add(new MetadataAttribute("Table", string.Format("\"{0}\"", table.Name))
        {
            Sets =
            {
                new MetadataAttributeSet("Schema", string.Format("\"{0}\"", table.Schema))
            }
        });

        var selection = project.GetSelection(table);

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];

            foreach (var prop in classDefinition.Properties)
            {
                if (project.GetPropName(table, column) != prop.Name)
                    continue;

                if (table.Identity?.Name == column.Name)
                    prop.Attributes.Add(new MetadataAttribute("DatabaseGenerated", "DatabaseGeneratedOption.Identity"));

                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                    prop.Attributes.Add(new MetadataAttribute("Key"));

                if (prop.Name == column.Name)
                    prop.Attributes.Add(new MetadataAttribute("Column"));
                else
                    prop.Attributes.Add(new MetadataAttribute("Column", string.Format("\"{0}\"", column.Name)));

                if (!column.Nullable && table.Identity != null && table.Identity.Name != column.Name)
                    prop.Attributes.Add(new MetadataAttribute("Required"));

                if (project.Database.ColumnIsString(column) && column.Length > 0)
                    prop.Attributes.Add(new MetadataAttribute("StringLength", column.Length.ToString()));

                if (!string.IsNullOrEmpty(selection.Settings.ConcurrencyToken) && selection.Settings.ConcurrencyToken == column.Name)
                    prop.Attributes.Add(new MetadataAttribute("Timestamp"));
            }
        }
    }

    public static void AddDataAnnotations(this IDotNetClassDefinition classDefinition, IView view, EntityFrameworkCoreProject project)
    {
        classDefinition.Attributes.Add(new MetadataAttribute("Table", string.Format("\"{0}\"", view.Name))
        {
            Sets =
            {
                new MetadataAttributeSet("Schema", string.Format("\"{0}\"", view.Schema))
            }
        });

        var primaryKeys = project
            .Database
            .Tables
            .Where(item => item.PrimaryKey != null)
            .Select(item => item.GetColumnsFromConstraint(item.PrimaryKey).Select(column => column.Name).First())
            .ToList();

        var result = view.Columns
            .Where(item => primaryKeys.Contains(item.Name))
            .ToList();

        for (var i = 0; i < view.Columns.Count; i++)
        {
            var column = view.Columns[i];

            foreach (var prop in classDefinition.Properties)
            {
                if (project.GetPropName(view, column) != prop.Name)
                    continue;

                if (prop.Name == column.Name)
                {
                    prop.Attributes.Add(new MetadataAttribute("Column")
                    {
                        Sets = { new MetadataAttributeSet("Order", (i + 1).ToString()) }
                    });
                }
                else
                {
                    prop.Attributes.Add(new MetadataAttribute("Column", string.Format("\"{0}\"", column.Name))
                    {
                        Sets = { new MetadataAttributeSet("Order", (i + 1).ToString()) }
                    });
                }

                if (!column.Nullable && primaryKeys.Contains(column.Name))
                    prop.Attributes.Add(new MetadataAttribute("Key"));

                if (!column.Nullable)
                    prop.Attributes.Add(new MetadataAttribute("Required"));
            }
        }
    }
}
