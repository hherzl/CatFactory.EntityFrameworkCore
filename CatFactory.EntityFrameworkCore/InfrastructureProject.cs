using CatFactory.NetCore;

namespace CatFactory.EntityFrameworkCore;

public class InfrastructureProject : CSharpProject<InfrastructureProjectSettings>
{
    public InfrastructureProject()
    {
        Namespaces = new();
    }

    public CleanArchitecureNamespaces Namespaces { get; set; }
}
