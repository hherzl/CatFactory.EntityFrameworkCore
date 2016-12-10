using System;

namespace CatFactory.EfCore
{
    public class Namespaces
    {
        public Namespaces()
        {
            EntityLayer = "EntityLayer";
            DataLayer = "DataLayer";
            DataLayerMapping = "DataLayer.Mapping";
            DataLayerContracts = "DataLayer.Contracts";
            DataLayerRepositories = "DataLayer.Repositories";
        }

        public String EntityLayer { get; set; }

        public String DataLayer { get; set; }

        public String DataLayerMapping { get; set; }

        public String DataLayerContracts { get; set; }

        public String DataLayerRepositories { get; set; }
    }
}
