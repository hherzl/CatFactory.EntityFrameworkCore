using System.Collections.Generic;

namespace CatFactory.EntityFrameworkCore
{
    public class AuditEntity
    {
        public AuditEntity()
        {
        }

        public string CreationUserColumnName { get; set; }

        public string CreationDateTimeColumnName { get; set; }

        public string LastUpdateUserColumnName { get; set; }

        public string LastUpdateDateTimeColumnName { get; set; }

        public IEnumerable<string> Names
            => new string[]
            {
                CreationUserColumnName,
                CreationDateTimeColumnName,
                LastUpdateUserColumnName,
                LastUpdateDateTimeColumnName
            };
    }
}
