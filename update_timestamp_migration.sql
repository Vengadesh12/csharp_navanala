-- =========================================================================================
-- DATABASE MIGRATION SCRIPT: ADD updated_at & created_at TIMESTAMPS TO ALL TABLES
-- Database: PostgreSQL
-- Description: Adds updated_at and created_at columns, backfills existing data,
--              and sets up automatic update triggers on all 22 tables.
-- =========================================================================================

BEGIN;

-- -----------------------------------------------------------------------------------------
-- 1. Create generic PostgreSQL trigger functions to automatically update timestamp on UPDATE
-- -----------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION set_updated_at_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION "set_UpdatedAt_timestamp"()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- -----------------------------------------------------------------------------------------
-- 2. Add created_at and updated_at columns to all 22 database tables (safe & idempotent)
-- -----------------------------------------------------------------------------------------

-- 1. users
ALTER TABLE IF EXISTS "users" 
    ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 2. roles
ALTER TABLE IF EXISTS "roles" 
    ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 3. departments
ALTER TABLE IF EXISTS "departments" 
    ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 4. designations
ALTER TABLE IF EXISTS "designations" 
    ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 5. permissions
ALTER TABLE IF EXISTS "permissions" 
    ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 6. rolepermissions
ALTER TABLE IF EXISTS "rolepermissions" 
    ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 7. departmentpermissions
ALTER TABLE IF EXISTS "departmentpermissions" 
    ADD COLUMN IF NOT EXISTS "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 8. user_sessions
ALTER TABLE IF EXISTS user_sessions 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 9. menus
ALTER TABLE IF EXISTS menus 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 10. audit_logs
ALTER TABLE IF EXISTS audit_logs 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- 11. reports
ALTER TABLE IF EXISTS reports 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- 12. report_categories
ALTER TABLE IF EXISTS report_categories 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 13. projects
ALTER TABLE IF EXISTS projects 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- 14. project_categories
ALTER TABLE IF EXISTS project_categories 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 15. schedules
ALTER TABLE IF EXISTS schedules 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- 16. system_settings
ALTER TABLE IF EXISTS system_settings 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- 17. setting_categories
ALTER TABLE IF EXISTS setting_categories 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- 18. event_types
ALTER TABLE IF EXISTS event_types 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP;

-- 19. approval_requests
ALTER TABLE IF EXISTS approval_requests 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 20. purchases
ALTER TABLE IF EXISTS purchases 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 21. invoices
ALTER TABLE IF EXISTS invoices 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- 22. invoice_items
ALTER TABLE IF EXISTS invoice_items 
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP;

-- -----------------------------------------------------------------------------------------
-- 3. Backfill existing records (set updated_at = created_at or CURRENT_TIMESTAMP if null)
-- -----------------------------------------------------------------------------------------
UPDATE "users" SET "UpdatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP) WHERE "UpdatedAt" IS NULL;
UPDATE "roles" SET "UpdatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP) WHERE "UpdatedAt" IS NULL;
UPDATE "departments" SET "UpdatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP) WHERE "UpdatedAt" IS NULL;
UPDATE "designations" SET "UpdatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP) WHERE "UpdatedAt" IS NULL;
UPDATE "permissions" SET "UpdatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP) WHERE "UpdatedAt" IS NULL;
UPDATE "rolepermissions" SET "UpdatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP) WHERE "UpdatedAt" IS NULL;
UPDATE "departmentpermissions" SET "UpdatedAt" = COALESCE("CreatedAt", CURRENT_TIMESTAMP) WHERE "UpdatedAt" IS NULL;
UPDATE user_sessions SET updated_at = COALESCE(created_at, login_time, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE menus SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE audit_logs SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE reports SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE report_categories SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE projects SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE project_categories SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE schedules SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE system_settings SET updated_at = COALESCE(updated_at, CURRENT_TIMESTAMP), created_at = COALESCE(created_at, updated_at, CURRENT_TIMESTAMP);
UPDATE setting_categories SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE event_types SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE approval_requests SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE purchases SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE invoices SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;
UPDATE invoice_items SET updated_at = COALESCE(created_at, CURRENT_TIMESTAMP) WHERE updated_at IS NULL;

-- -----------------------------------------------------------------------------------------
-- 4. Attach BEFORE UPDATE triggers to automatically update timestamp on direct SQL updates
-- -----------------------------------------------------------------------------------------

-- PascalCase Tables
DROP TRIGGER IF EXISTS trg_users_updated_at ON "users";
CREATE TRIGGER trg_users_updated_at BEFORE UPDATE ON "users" FOR EACH ROW EXECUTE FUNCTION "set_UpdatedAt_timestamp"();

DROP TRIGGER IF EXISTS trg_roles_updated_at ON "roles";
CREATE TRIGGER trg_roles_updated_at BEFORE UPDATE ON "roles" FOR EACH ROW EXECUTE FUNCTION "set_UpdatedAt_timestamp"();

DROP TRIGGER IF EXISTS trg_departments_updated_at ON "departments";
CREATE TRIGGER trg_departments_updated_at BEFORE UPDATE ON "departments" FOR EACH ROW EXECUTE FUNCTION "set_UpdatedAt_timestamp"();

DROP TRIGGER IF EXISTS trg_designations_updated_at ON "designations";
CREATE TRIGGER trg_designations_updated_at BEFORE UPDATE ON "designations" FOR EACH ROW EXECUTE FUNCTION "set_UpdatedAt_timestamp"();

DROP TRIGGER IF EXISTS trg_permissions_updated_at ON "permissions";
CREATE TRIGGER trg_permissions_updated_at BEFORE UPDATE ON "permissions" FOR EACH ROW EXECUTE FUNCTION "set_UpdatedAt_timestamp"();

DROP TRIGGER IF EXISTS trg_rolepermissions_updated_at ON "rolepermissions";
CREATE TRIGGER trg_rolepermissions_updated_at BEFORE UPDATE ON "rolepermissions" FOR EACH ROW EXECUTE FUNCTION "set_UpdatedAt_timestamp"();

DROP TRIGGER IF EXISTS trg_departmentpermissions_updated_at ON "departmentpermissions";
CREATE TRIGGER trg_departmentpermissions_updated_at BEFORE UPDATE ON "departmentpermissions" FOR EACH ROW EXECUTE FUNCTION "set_UpdatedAt_timestamp"();

-- snake_case Tables
DROP TRIGGER IF EXISTS trg_user_sessions_updated_at ON user_sessions;
CREATE TRIGGER trg_user_sessions_updated_at BEFORE UPDATE ON user_sessions FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_menus_updated_at ON menus;
CREATE TRIGGER trg_menus_updated_at BEFORE UPDATE ON menus FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_audit_logs_updated_at ON audit_logs;
CREATE TRIGGER trg_audit_logs_updated_at BEFORE UPDATE ON audit_logs FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_reports_updated_at ON reports;
CREATE TRIGGER trg_reports_updated_at BEFORE UPDATE ON reports FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_report_categories_updated_at ON report_categories;
CREATE TRIGGER trg_report_categories_updated_at BEFORE UPDATE ON report_categories FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_projects_updated_at ON projects;
CREATE TRIGGER trg_projects_updated_at BEFORE UPDATE ON projects FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_project_categories_updated_at ON project_categories;
CREATE TRIGGER trg_project_categories_updated_at BEFORE UPDATE ON project_categories FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_schedules_updated_at ON schedules;
CREATE TRIGGER trg_schedules_updated_at BEFORE UPDATE ON schedules FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_system_settings_updated_at ON system_settings;
CREATE TRIGGER trg_system_settings_updated_at BEFORE UPDATE ON system_settings FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_setting_categories_updated_at ON setting_categories;
CREATE TRIGGER trg_setting_categories_updated_at BEFORE UPDATE ON setting_categories FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_event_types_updated_at ON event_types;
CREATE TRIGGER trg_event_types_updated_at BEFORE UPDATE ON event_types FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_approval_requests_updated_at ON approval_requests;
CREATE TRIGGER trg_approval_requests_updated_at BEFORE UPDATE ON approval_requests FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_purchases_updated_at ON purchases;
CREATE TRIGGER trg_purchases_updated_at BEFORE UPDATE ON purchases FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_invoices_updated_at ON invoices;
CREATE TRIGGER trg_invoices_updated_at BEFORE UPDATE ON invoices FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

DROP TRIGGER IF EXISTS trg_invoice_items_updated_at ON invoice_items;
CREATE TRIGGER trg_invoice_items_updated_at BEFORE UPDATE ON invoice_items FOR EACH ROW EXECUTE FUNCTION set_updated_at_timestamp();

COMMIT;
