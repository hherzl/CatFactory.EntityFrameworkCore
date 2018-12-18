namespace CatFactory.EntityFrameworkCore
{
    public class AuditEntity
    {
        public AuditEntity()
        {
        }

        //public AuditEntity(string creationUserColumnName, string creationDateTimeColumnName, string lastUpdateUserColumnName, string lastUpdateDateTimeColumnName)
        //{
        //    CreationUserColumnName = creationUserColumnName;
        //    CreationDateTimeColumnName = creationDateTimeColumnName;
        //    LastUpdateUserColumnName = lastUpdateUserColumnName;
        //    LastUpdateDateTimeColumnName = lastUpdateDateTimeColumnName;
        //}

        public string CreationUserColumnName { get; set; }

        public string CreationDateTimeColumnName { get; set; }

        public string LastUpdateUserColumnName { get; set; }

        public string LastUpdateDateTimeColumnName { get; set; }

        public string[] Names
            => new string[] { CreationUserColumnName, CreationDateTimeColumnName, LastUpdateUserColumnName, LastUpdateDateTimeColumnName };
    }
}
