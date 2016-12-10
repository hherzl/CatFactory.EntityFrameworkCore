using System;

namespace CatFactory.EfCore
{
    public static class NamingService
    {
        public static String GetSingularName(String value)
        {
            if (value.EndsWith("ies"))
            {
                return String.Format("{0}y", value.Substring(0, value.Length - 3));
            }
            else if (value.EndsWith("s"))
            {
                return String.Format("{0}", value.Substring(0, value.Length - 1));
            }
            else
            {
                return value;
            }
        }

        public static String GetPluralName(String value)
        {
            // todo: improve the way to pluralize a name

            if (value.EndsWith("s"))
            {
                return value;
            }
            else if (value.EndsWith("y"))
            {
                return String.Format("{0}ies", value.Substring(0, value.Length - 1));
            }
            else
            {
                return String.Format("{0}s", value);
            }
        }
    }
}
