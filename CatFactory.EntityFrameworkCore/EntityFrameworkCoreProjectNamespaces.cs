namespace CatFactory.EntityFrameworkCore;

public class EntityFrameworkCoreProjectNamespaces
{
    public EntityFrameworkCoreProjectNamespaces()
    {
        Common = "Common";

        Domain = "Domain";
        Entities = "Entities";
        Exceptions = "Exceptions";

        Application = "Application";
        Features = "Features";
        Queries = "Queries";
        QuerySpecs = "QuerySpecs";
        Commands = "Commands";
        Services = "Services";

        Infrastructure = "Infrastructure";
        Persistence = "Persistence";

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

    public string Common { get; set; }

    public string Domain { get; set; }
    public string Entities { get; set; }
    public string Exceptions { get; set; }

    public string Application { get; set; }
    public string Features { get; set; }
    public string Queries { get; set; }
    public string QuerySpecs { get; set; }
    public string Commands { get; set; }
    public string Services { get; set; }

    public string Infrastructure { get; set; }
    public string Persistence { get; set; }

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
