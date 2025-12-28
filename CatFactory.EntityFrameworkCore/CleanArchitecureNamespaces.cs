namespace CatFactory.EntityFrameworkCore;

public class CleanArchitecureNamespaces
{
    public CleanArchitecureNamespaces()
    {
        Common = "Common";
        Models = "Models";

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
        Configurations = "Configurations";
    }

    public string Common { get; set; }
    public string Models { get; set; }

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
    public string Configurations { get; set; }
}
