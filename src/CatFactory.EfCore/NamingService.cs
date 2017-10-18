namespace CatFactory.EfCore
{
    public static class NamingService
    {
        public static string GetSingularName(string value)
        {
            if (value.EndsWith("ies"))
            {
                return string.Format("{0}y", value.Substring(0, value.Length - 3));
            }
            else if (value.EndsWith("tus"))
            {
                return value;
            }
            else if (value.EndsWith("s"))
            {
                return string.Format("{0}", value.Substring(0, value.Length - 1));
            }
            else
            {
                return value;
            }
        }

        public static string GetPluralName(string value)
        {
            // todo: improve the way to pluralize a name

            if (value.EndsWith("ss"))
            {
                return string.Format("{0}es", value);
            }
            else if (value.EndsWith("s"))
            {
                return value;
            }
            else if (value.EndsWith("y"))
            {
                return string.Format("{0}ies", value.Substring(0, value.Length - 1));
            }
            else
            {
                return string.Format("{0}s", value);
            }
        }
    }
}
