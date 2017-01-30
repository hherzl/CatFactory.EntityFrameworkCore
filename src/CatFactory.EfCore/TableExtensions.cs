using System.Collections.Generic;
using CatFactory.Mapping;

namespace CatFactory.EfCore
{
    public static class TableExtensions
    {
        public static IEnumerable<Column> GetUpdateColumns(this Table table, Project project)
        {
            foreach (var column in table.GetColumnsWithOutKey())
            {
                if (project.UpdateExclusions.Contains(column.Name))
                {
                    continue;
                }

                yield return column;
            }
        }
    }
}
