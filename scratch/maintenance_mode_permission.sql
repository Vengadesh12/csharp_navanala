-- 1. Insert 'settings.maintenance' permission if it doesn't already exist
INSERT INTO public.permissions ("PermissionKey", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt")
SELECT 'settings.maintenance', 'Maintenance Mode', 'Configure and toggle system-wide maintenance mode access.', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM public.permissions WHERE LOWER("PermissionKey") = 'settings.maintenance'
);

-- 2. Grant 'settings.maintenance' permission to Super Admin (Role ID 2) and Admin roles
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "CreatedAt", "UpdatedAt")
SELECT r."Id", p."Id", CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM public.roles r
CROSS JOIN public.permissions p
WHERE LOWER(p."PermissionKey") = 'settings.maintenance'
  AND (r."Id" = 2 OR LOWER(r."Name") LIKE '%super admin%' OR LOWER(r."Name") = 'admin')
  AND NOT EXISTS (
      SELECT 1 FROM public.rolepermissions rp 
      WHERE rp."RoleId" = r."Id" AND rp."PermissionId" = p."Id"
  );

-- 3. Insert 'settings.maintenance' Menu in the dynamic menus table if it doesn't exist
INSERT INTO public.menus ("menukey", "label", "icon", "route", "groupname", "description", "orderindex", "permissionkey", "deletedflag", "created_at", "updated_at")
SELECT 'settings.maintenance', 'Maintenance Mode', '🛠', '/settings', 'Preferences', 'Workspace maintenance mode & access restrictions', 16, 'settings.maintenance', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (
    SELECT 1 FROM public.menus WHERE LOWER("menukey") = 'settings.maintenance'
);
