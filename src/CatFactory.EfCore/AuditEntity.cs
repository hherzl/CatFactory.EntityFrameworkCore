using System;

namespace CatFactory.EfCore
{
    public class AuditEntity
    {
        public AuditEntity()
        {
        }

        public AuditEntity(String creationUserColumnName, String creationDateTimeColumnName, String lastUpdateUserColumnName, String lastUpdateDateTimeColumnName)
        {
            CreationUserColumnName = creationUserColumnName;
            CreationDateTimeColumnName = creationDateTimeColumnName;
            LastUpdateUserColumnName = lastUpdateUserColumnName;
            LastUpdateDateTimeColumnName = lastUpdateDateTimeColumnName;
        }

        public String CreationUserColumnName { get; set; }

        public String CreationDateTimeColumnName { get; set; }

        public String LastUpdateUserColumnName { get; set; }

        public String LastUpdateDateTimeColumnName { get; set; }

        public String[] Names
            => new String[]
            {
                CreationUserColumnName,
                CreationDateTimeColumnName,
                LastUpdateUserColumnName,
                LastUpdateDateTimeColumnName
            };
    }
}
