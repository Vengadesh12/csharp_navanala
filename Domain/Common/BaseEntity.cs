namespace MyBackend.Domain.Common
{
    /// <summary>
    /// Base business object class containing common identifier and soft-delete state management.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Soft deletion flag (1 = Active, 0 = Inactive/Deleted).
        /// </summary>
        public int DeletedFlag { get; set; } = 1;

        /// <summary>
        /// Record creation timestamp.
        /// </summary>
        public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Record last modification timestamp.
        /// </summary>
        public virtual DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Marks the business object as soft-deleted.
        /// </summary>
        public virtual void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Restores a soft-deleted business object.
        /// </summary>
        public virtual void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the business object is currently active.
        /// </summary>
        public virtual bool IsActive => DeletedFlag == 1;
    }
}
