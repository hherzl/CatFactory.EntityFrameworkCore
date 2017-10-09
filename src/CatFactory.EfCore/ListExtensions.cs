using System;
using System.Collections.Generic;
using System.Text;

namespace CatFactory.EfCore
{
    public static class ListExtensions
    {
        public static StringBuilder ToStringBuilder(this List<String> list)
        {
            var sb = new StringBuilder();

            foreach (var item in list)
            {
                sb.AppendLine(item);
            };

            return sb;
        }
    }
}
