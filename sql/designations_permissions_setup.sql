-- ==============================================================================
-- Migration / Setup Script: Designation Management & Super Admin Permissions
-- Database: PostgreSQL
-- ==============================================================================

-- 1. Insert Designation permissions into "permissions" table
INSERT INTO "permissions" ("PermissionKey", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt")
VALUES 
    ('designations.view', 'View Designations', 'View workspace designations and job titles.', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('designations.create', 'Create Designations', 'Create and define new workspace designations.', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('designations.edit', 'Edit Designations', 'Modify designation titles, descriptions, and department links.', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('designations.delete', 'Delete Designations', 'Deactivate or delete designations.', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('designations.manage', 'Manage Designations', 'Full administrative control over designations.', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT ("PermissionKey") DO NOTHING;

-- 2. Grant all Designation permissions to Super Admin role (RoleId = 2)
INSERT INTO "rolepermissions" ("RoleId", "PermissionId", "Access", "CreatedAt", "UpdatedAt")
SELECT 2, p."Id", 'Allow', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM "permissions" p
WHERE p."PermissionKey" IN ('designations.view', 'designations.create', 'designations.edit', 'designations.delete', 'designations.manage')
  AND NOT EXISTS (
      SELECT 1 FROM "rolepermissions" rp 
      WHERE rp."RoleId" = 2 AND rp."PermissionId" = p."Id"
  );

-- 3. Register or update the 'Designations' menu in "menus" table
INSERT INTO "menus" ("menukey", "label", "icon", "route", "groupname", "description", "orderindex", "permissionkey", "deletedflag", "created_at", "updated_at")
VALUES (
    'designations.view', 
    'Designations', 
    '💼', 
    '/designations', 
    'Core Access', 
    'Workspace designations and title mapping', 
    4, 
    'designations.view', 
    1, 
    CURRENT_TIMESTAMP, 
    CURRENT_TIMESTAMP
)
ON CONFLICT ("menukey") DO UPDATE SET
    "label" = EXCLUDED."label",
    "icon" = EXCLUDED."icon",
    "route" = EXCLUDED."route",
    "groupname" = EXCLUDED."groupname",
    "permissionkey" = EXCLUDED."permissionkey",
    "deletedflag" = 1,
    "updated_at" = CURRENT_TIMESTAMP;
