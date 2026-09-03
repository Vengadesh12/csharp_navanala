namespace MyBackend.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public int DeletedFlag { get; set; } = 1;

        public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        public virtual bool IsActive => DeletedFlag == 1;
    }
}
