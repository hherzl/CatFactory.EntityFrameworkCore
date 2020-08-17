namespace CatFactory.EntityFrameworkCore
{
    public class EntityFrameworkCoreProjectNamespaces
    {
        public EntityFrameworkCoreProjectNamespaces()
        {
            Models = "Models";
            QueryModels = "QueryModels";
            EntityLayer = "EntityLayer";
            DataLayer = "DataLayer";
            Configurations = "Configurations";
            Contracts = "Contracts";
            DataContracts = "DataContracts";
            Repositories = "Repositories";
            ValueConversion = "ValueConversion";
        }

        public string Models { get; set; }

        public string QueryModels { get; set; }

        public string EntityLayer { get; set; }

        public string DataLayer { get; set; }

        public string Configurations { get; set; }

        public string Contracts { get; set; }

        public string DataContracts { get; set; }

        public string Repositories { get; set; }

        /// <summary>
        /// Project Microsoft.EntityFrameworkCore.Storage.ValueConversion classes and types can be found here;
        /// </summary>
        public string ValueConversion { get; set; }
    }
}
