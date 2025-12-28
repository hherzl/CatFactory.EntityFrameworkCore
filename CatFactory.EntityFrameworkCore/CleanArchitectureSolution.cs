using CatFactory.ObjectRelationalMapping;

namespace CatFactory.EntityFrameworkCore;

public class CleanArchitectureSolution
{
    public CleanArchitectureSolution(string name, string outputDirectory, Database database)
    {
        Name = name;
        OutputDirectory = outputDirectory;
        Database = database;

        Namespaces = new();
        Domain = new(this);
        Application = new(this);
    }

    public string Name { get; set; }
    public string OutputDirectory { get; set; }
    public Database Database { get; set; }

    public CleanArchitecureNamespaces Namespaces { get; set; }
    public DomainProject Domain { get; set; }
    public ApplicationProject Application { get; set; }
}
