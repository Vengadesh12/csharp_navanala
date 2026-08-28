namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Join business object mapping assigned permissions to roles.
    /// </summary>
    public class RolePermission
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public static RolePermission Create(int roleId, int permissionId) => new()
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}
