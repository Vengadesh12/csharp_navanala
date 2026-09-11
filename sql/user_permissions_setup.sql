-- ==============================================================================
-- Migration / Setup Script: User Permissions Management & Super Admin Access
-- Database: PostgreSQL
-- ==============================================================================

-- 1. Create 'userpermissions' table if it does not exist
CREATE TABLE IF NOT EXISTS "userpermissions" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NOT NULL,
    "PermissionId" INT NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_userpermissions_user FOREIGN KEY ("UserId") REFERENCES "users"("Id") ON DELETE CASCADE,
    CONSTRAINT fk_userpermissions_permission FOREIGN KEY ("PermissionId") REFERENCES "permissions"("Id") ON DELETE CASCADE
);

-- Create compound index for fast lookups
CREATE INDEX IF NOT EXISTS ix_userpermissions_userid_permissionid 
ON "userpermissions" ("UserId", "PermissionId");

-- 2. Ensure standard permissions for User Permissions exist
INSERT INTO "permissions" ("PermissionKey", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt")
VALUES 
    ('user_permissions.view', 'View User Permissions', 'View assigned user direct permissions and access levels', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('user_permissions.manage', 'Manage User Permissions', 'Assign and revoke direct user permissions', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT ("PermissionKey") DO NOTHING;

-- 3. Grant full permissions to Super Admin role (RoleId = 2)
INSERT INTO "rolepermissions" ("RoleId", "PermissionId", "Access", "CreatedAt", "UpdatedAt")
SELECT 2, p."Id", 'Allow', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM "permissions" p
WHERE p."PermissionKey" IN ('user_permissions.view', 'user_permissions.manage', 'permissions.manage')
  AND NOT EXISTS (
      SELECT 1 FROM "rolepermissions" rp 
      WHERE rp."RoleId" = 2 AND rp."PermissionId" = p."Id"
  );

-- 4. Register the new 'User Permissions' menu
INSERT INTO "menus" ("menukey", "label", "icon", "route", "groupname", "description", "orderindex", "permissionkey", "deletedflag", "created_at", "updated_at")
VALUES (
    'user_permissions.view', 
    'User Permissions', 
    '🛡️', 
    '/user-permissions', 
    'Core Access', 
    'User direct permission access control & assignment', 
    6, 
    'permissions.manage', 
    1, 
    CURRENT_TIMESTAMP, 
    CURRENT_TIMESTAMP
)
ON CONFLICT ("menukey") DO UPDATE SET
    "label" = EXCLUDED."label",
    "route" = EXCLUDED."route",
    "groupname" = EXCLUDED."groupname",
    "permissionkey" = EXCLUDED."permissionkey",
    "deletedflag" = 1;
