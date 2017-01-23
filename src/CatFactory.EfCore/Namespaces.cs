using System;

namespace CatFactory.EfCore
{
    public class Namespaces
    {
        public Namespaces()
        {
            EntityLayer = "EntityLayer";
            DataLayer = "DataLayer";
            Mapping = "Mapping";
            Contracts = "Contracts";
            DataContracts = "DataContracts";
            Repositories = "Repositories";
            BusinessLayer = "BusinessLayer";
            Responses = "Responses";
        }

        public String EntityLayer { get; set; }

        public String DataLayer { get; set; }

        public String Mapping { get; set; }

        public String Contracts { get; set; }

        public String DataContracts { get; set; }

        public String Repositories { get; set; }

        public String BusinessLayer { get; set; }

        public String Responses { get; set; }
    }
}
