namespace CatFactory.EfCore
{
    public class EfCoreProject : Project
    {
        public EfCoreProject()
            : base()
        {
            Namespaces = new Namespaces();
        }

        public Namespaces Namespaces { get; set; }
    }
}
