using System.Collections.Generic;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Programmability;

namespace CatFactory.EntityFrameworkCore
{
    public static class DatabaseExtensions
    {
        public static List<ScalarFunction> GetScalarFunctions(this Database database)
        {
            var importBag = database.ImportBag as IDictionary<string, object>;

            if (importBag.ContainsKey("ScalarFunctions"))
            {
                if (importBag["ScalarFunctions"] is List<ScalarFunction> scalarFunctions)
                    return scalarFunctions;
            }

            return new List<ScalarFunction>();
        }

        public static List<TableFunction> GetTableFunctions(this Database database)
        {
            var importBag = database.ImportBag as IDictionary<string, object>;

            if (importBag.ContainsKey("TableFunctions"))
            {
                if (importBag["TableFunctions"] is List<TableFunction> tableFunctions)
                    return tableFunctions;
            }

            return new List<TableFunction>();
        }

        public static List<StoredProcedure> GetStoredProcedures(this Database database)
        {
            var importBag = database.ImportBag as IDictionary<string, object>;

            if (importBag.ContainsKey("StoredProcedures"))
            {
                if (importBag["StoredProcedures"] is List<StoredProcedure> storedProcedures)
                    return storedProcedures;
            }

            return new List<StoredProcedure>();
        }
    }
}
