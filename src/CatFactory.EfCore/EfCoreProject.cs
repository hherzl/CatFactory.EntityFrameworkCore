using System;

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

        public Boolean UseDataAnnotations { get; set; }

        public Boolean DeclareDbSetPropertiesInDbContext { get; set; }
    }
}
