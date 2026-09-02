--
-- PostgreSQL database dump
--


-- Dumped from database version 18.6
-- Dumped by pg_dump version 18.6

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS "users_DesignationId_fkey";
ALTER TABLE IF EXISTS ONLY public.reports DROP CONSTRAINT IF EXISTS reports_category_id_fkey;
ALTER TABLE IF EXISTS ONLY public.invoice_items DROP CONSTRAINT IF EXISTS invoice_items_invoice_id_fkey;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS "FK_Users_Roles";
ALTER TABLE IF EXISTS ONLY public.rolepermissions DROP CONSTRAINT IF EXISTS "FK_RolePermissions_Role";
ALTER TABLE IF EXISTS ONLY public.rolepermissions DROP CONSTRAINT IF EXISTS "FK_RolePermissions_Permission";
DROP INDEX IF EXISTS public.idx_purchases_status;
DROP INDEX IF EXISTS public.idx_purchases_approval_request_id;
DROP INDEX IF EXISTS public.idx_approval_requests_user_id;
DROP INDEX IF EXISTS public.idx_approval_requests_status;
DROP INDEX IF EXISTS public.idx_approval_requests_deleted_flag;
ALTER TABLE IF EXISTS ONLY public.menus DROP CONSTRAINT IF EXISTS ux_menus_key;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS users_pkey;
ALTER TABLE IF EXISTS ONLY public.user_sessions DROP CONSTRAINT IF EXISTS user_sessions_pkey;
ALTER TABLE IF EXISTS ONLY public.system_settings DROP CONSTRAINT IF EXISTS system_settings_setting_key_key;
ALTER TABLE IF EXISTS ONLY public.system_settings DROP CONSTRAINT IF EXISTS system_settings_pkey;
ALTER TABLE IF EXISTS ONLY public.setting_categories DROP CONSTRAINT IF EXISTS setting_categories_pkey;
ALTER TABLE IF EXISTS ONLY public.setting_categories DROP CONSTRAINT IF EXISTS setting_categories_name_key;
ALTER TABLE IF EXISTS ONLY public.schedules DROP CONSTRAINT IF EXISTS schedules_pkey;
ALTER TABLE IF EXISTS ONLY public.roles DROP CONSTRAINT IF EXISTS roles_pkey;
ALTER TABLE IF EXISTS ONLY public.reports DROP CONSTRAINT IF EXISTS reports_pkey;
ALTER TABLE IF EXISTS ONLY public.report_categories DROP CONSTRAINT IF EXISTS report_categories_pkey;
ALTER TABLE IF EXISTS ONLY public.report_categories DROP CONSTRAINT IF EXISTS report_categories_name_key;
ALTER TABLE IF EXISTS ONLY public.purchases DROP CONSTRAINT IF EXISTS purchases_pkey;
ALTER TABLE IF EXISTS ONLY public.projects DROP CONSTRAINT IF EXISTS projects_pkey;
ALTER TABLE IF EXISTS ONLY public.project_categories DROP CONSTRAINT IF EXISTS project_categories_pkey;
ALTER TABLE IF EXISTS ONLY public.project_categories DROP CONSTRAINT IF EXISTS project_categories_name_key;
ALTER TABLE IF EXISTS ONLY public.permissions DROP CONSTRAINT IF EXISTS permissions_pkey;
ALTER TABLE IF EXISTS ONLY public.menus DROP CONSTRAINT IF EXISTS menus_pkey;
ALTER TABLE IF EXISTS ONLY public.invoices DROP CONSTRAINT IF EXISTS invoices_pkey;
ALTER TABLE IF EXISTS ONLY public.invoices DROP CONSTRAINT IF EXISTS invoices_invoice_number_key;
ALTER TABLE IF EXISTS ONLY public.invoice_items DROP CONSTRAINT IF EXISTS invoice_items_pkey;
ALTER TABLE IF EXISTS ONLY public.event_types DROP CONSTRAINT IF EXISTS event_types_pkey;
ALTER TABLE IF EXISTS ONLY public.event_types DROP CONSTRAINT IF EXISTS event_types_name_key;
ALTER TABLE IF EXISTS ONLY public.designations DROP CONSTRAINT IF EXISTS designations_pkey;
ALTER TABLE IF EXISTS ONLY public.designations DROP CONSTRAINT IF EXISTS "designations_Name_key";
ALTER TABLE IF EXISTS ONLY public.departments DROP CONSTRAINT IF EXISTS departments_pkey;
ALTER TABLE IF EXISTS ONLY public.departmentpermissions DROP CONSTRAINT IF EXISTS departmentpermissions_pkey;
ALTER TABLE IF EXISTS ONLY public.audit_logs DROP CONSTRAINT IF EXISTS audit_logs_pkey;
ALTER TABLE IF EXISTS ONLY public.approval_requests DROP CONSTRAINT IF EXISTS approval_requests_pkey;
ALTER TABLE IF EXISTS ONLY public.users DROP CONSTRAINT IF EXISTS "UX_Users_Email";
ALTER TABLE IF EXISTS ONLY public.roles DROP CONSTRAINT IF EXISTS "UX_Roles_Name_Active";
ALTER TABLE IF EXISTS ONLY public.permissions DROP CONSTRAINT IF EXISTS "UX_Permissions_Key";
ALTER TABLE IF EXISTS ONLY public.rolepermissions DROP CONSTRAINT IF EXISTS "PK_RolePermissions";
ALTER TABLE IF EXISTS public.user_sessions ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.system_settings ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.setting_categories ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.schedules ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.rolepermissions ALTER COLUMN "Id" DROP DEFAULT;
ALTER TABLE IF EXISTS public.reports ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.report_categories ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.purchases ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.projects ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.project_categories ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.invoices ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.invoice_items ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.event_types ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.designations ALTER COLUMN "Id" DROP DEFAULT;
ALTER TABLE IF EXISTS public.departments ALTER COLUMN "Id" DROP DEFAULT;
ALTER TABLE IF EXISTS public.departmentpermissions ALTER COLUMN "Id" DROP DEFAULT;
ALTER TABLE IF EXISTS public.audit_logs ALTER COLUMN id DROP DEFAULT;
ALTER TABLE IF EXISTS public.approval_requests ALTER COLUMN id DROP DEFAULT;
DROP TABLE IF EXISTS public.users;
DROP SEQUENCE IF EXISTS public.user_sessions_id_seq;
DROP TABLE IF EXISTS public.user_sessions;
DROP SEQUENCE IF EXISTS public.system_settings_id_seq;
DROP TABLE IF EXISTS public.system_settings;
DROP SEQUENCE IF EXISTS public.setting_categories_id_seq;
DROP TABLE IF EXISTS public.setting_categories;
DROP SEQUENCE IF EXISTS public.schedules_id_seq;
DROP TABLE IF EXISTS public.schedules;
DROP TABLE IF EXISTS public.roles;
DROP SEQUENCE IF EXISTS public."rolepermissions_Id_seq";
DROP TABLE IF EXISTS public.rolepermissions;
DROP SEQUENCE IF EXISTS public.reports_id_seq;
DROP TABLE IF EXISTS public.reports;
DROP SEQUENCE IF EXISTS public.report_categories_id_seq;
DROP TABLE IF EXISTS public.report_categories;
DROP SEQUENCE IF EXISTS public.purchases_id_seq;
DROP TABLE IF EXISTS public.purchases;
DROP SEQUENCE IF EXISTS public.projects_id_seq;
DROP TABLE IF EXISTS public.projects;
DROP SEQUENCE IF EXISTS public.project_categories_id_seq;
DROP TABLE IF EXISTS public.project_categories;
DROP TABLE IF EXISTS public.permissions;
DROP TABLE IF EXISTS public.menus;
DROP SEQUENCE IF EXISTS public.invoices_id_seq;
DROP TABLE IF EXISTS public.invoices;
DROP SEQUENCE IF EXISTS public.invoice_items_id_seq;
DROP TABLE IF EXISTS public.invoice_items;
DROP SEQUENCE IF EXISTS public.event_types_id_seq;
DROP TABLE IF EXISTS public.event_types;
DROP SEQUENCE IF EXISTS public."designations_Id_seq";
DROP TABLE IF EXISTS public.designations;
DROP SEQUENCE IF EXISTS public."departments_Id_seq";
DROP TABLE IF EXISTS public.departments;
DROP SEQUENCE IF EXISTS public."departmentpermissions_Id_seq";
DROP TABLE IF EXISTS public.departmentpermissions;
DROP SEQUENCE IF EXISTS public.audit_logs_id_seq;
DROP TABLE IF EXISTS public.audit_logs;
DROP SEQUENCE IF EXISTS public.approval_requests_id_seq;
DROP TABLE IF EXISTS public.approval_requests;
--
-- Name: public; Type: SCHEMA; Schema: -; Owner: pg_database_owner
--




--
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--



SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: approval_requests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.approval_requests (
    id integer NOT NULL,
    user_id integer NOT NULL,
    employee_name character varying(150) NOT NULL,
    employee_email character varying(150) NOT NULL,
    department_name character varying(150),
    item_name character varying(200) NOT NULL,
    category character varying(100) DEFAULT 'Hardware & Devices'::character varying NOT NULL,
    description text NOT NULL,
    quantity integer DEFAULT 1 NOT NULL,
    priority character varying(50) DEFAULT 'Medium'::character varying NOT NULL,
    estimated_amount numeric(12,2),
    status character varying(50) DEFAULT 'Pending'::character varying NOT NULL,
    comments text,
    reviewed_by_id integer,
    reviewed_by_name character varying(150),
    reviewed_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT (now() AT TIME ZONE 'utc'::text) NOT NULL,
    updated_at timestamp with time zone,
    deleted_flag integer DEFAULT 1 NOT NULL
);



--
-- Name: approval_requests_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.approval_requests_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: approval_requests_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.approval_requests_id_seq OWNED BY public.approval_requests.id;


--
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.audit_logs (
    id integer NOT NULL,
    action character varying(100) NOT NULL,
    module character varying(100) NOT NULL,
    performed_by character varying(150) NOT NULL,
    details text NOT NULL,
    ip_address character varying(50) DEFAULT '127.0.0.1'::character varying,
    status character varying(50) DEFAULT 'Success'::character varying,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    deleted_flag integer DEFAULT 1
);



--
-- Name: audit_logs_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.audit_logs_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: audit_logs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.audit_logs_id_seq OWNED BY public.audit_logs.id;


--
-- Name: departmentpermissions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.departmentpermissions (
    "Id" integer NOT NULL,
    "DepartmentId" integer NOT NULL,
    "PermissionId" integer NOT NULL
);



--
-- Name: departmentpermissions_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."departmentpermissions_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: departmentpermissions_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."departmentpermissions_Id_seq" OWNED BY public.departmentpermissions."Id";


--
-- Name: departments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.departments (
    "Id" integer NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(255),
    "DeletedFlag" integer DEFAULT 1 NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT (now() AT TIME ZONE 'utc'::text) NOT NULL
);



--
-- Name: departments_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."departments_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: departments_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."departments_Id_seq" OWNED BY public.departments."Id";


--
-- Name: designations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.designations (
    "Id" integer NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(255) DEFAULT ''::character varying,
    "DeletedFlag" integer DEFAULT 1,
    "CreatedAt" timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    "DepartmentId" integer
);



--
-- Name: designations_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."designations_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: designations_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."designations_Id_seq" OWNED BY public.designations."Id";


--
-- Name: event_types; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.event_types (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    color character varying(50) DEFAULT '#3b82f6'::character varying,
    icon character varying(50) DEFAULT 'Event'::character varying,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    created_by character varying(150) DEFAULT 'System Admin'::character varying,
    deleted_flag integer DEFAULT 1
);



--
-- Name: event_types_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.event_types_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: event_types_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.event_types_id_seq OWNED BY public.event_types.id;


--
-- Name: invoice_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.invoice_items (
    id integer NOT NULL,
    invoice_id integer NOT NULL,
    product_name character varying(250) NOT NULL,
    description text,
    quantity integer DEFAULT 1 NOT NULL,
    unit_price numeric(18,2) DEFAULT 0.00 NOT NULL,
    tax_rate numeric(5,2) DEFAULT 18.00 NOT NULL,
    tax_amount numeric(18,2) DEFAULT 0.00 NOT NULL,
    total_amount numeric(18,2) DEFAULT 0.00 NOT NULL,
    order_index integer DEFAULT 0 NOT NULL,
    deleted_flag integer DEFAULT 1 NOT NULL
);



--
-- Name: invoice_items_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.invoice_items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: invoice_items_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.invoice_items_id_seq OWNED BY public.invoice_items.id;


--
-- Name: invoices; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.invoices (
    id integer NOT NULL,
    invoice_number character varying(50) NOT NULL,
    customer_name character varying(150) NOT NULL,
    customer_email character varying(150),
    customer_phone character varying(50),
    customer_address text,
    customer_gstin character varying(50),
    company_gstin character varying(50) DEFAULT '36AAAAA0000A1Z5'::character varying,
    invoice_date timestamp with time zone DEFAULT now() NOT NULL,
    due_date timestamp with time zone,
    subtotal numeric(18,2) DEFAULT 0.00 NOT NULL,
    tax_rate numeric(5,2) DEFAULT 18.00 NOT NULL,
    tax_amount numeric(18,2) DEFAULT 0.00 NOT NULL,
    discount_amount numeric(18,2) DEFAULT 0.00,
    total_amount numeric(18,2) DEFAULT 0.00 NOT NULL,
    total_amount_in_words text NOT NULL,
    status character varying(50) DEFAULT 'Draft'::character varying NOT NULL,
    payment_method character varying(50),
    notes text,
    terms_and_conditions text,
    created_by_user_id integer NOT NULL,
    created_by_name character varying(150),
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone,
    deleted_flag integer DEFAULT 1 NOT NULL
);



--
-- Name: invoices_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.invoices_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: invoices_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.invoices_id_seq OWNED BY public.invoices.id;


--
-- Name: menus; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.menus (
    id integer NOT NULL,
    menukey character varying(100) NOT NULL,
    label character varying(100) NOT NULL,
    icon character varying(100) DEFAULT ''::character varying NOT NULL,
    route character varying(255) NOT NULL,
    groupname character varying(100) DEFAULT 'Core Access'::character varying NOT NULL,
    description character varying(255) DEFAULT ''::character varying NOT NULL,
    orderindex integer DEFAULT 0 NOT NULL,
    permissionkey character varying(100),
    deletedflag smallint DEFAULT 1 NOT NULL
);



--
-- Name: menus_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.menus ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.menus_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: permissions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.permissions (
    "Id" integer NOT NULL,
    "PermissionKey" character varying(100) NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(255) DEFAULT ''::character varying NOT NULL,
    "DeletedFlag" smallint DEFAULT 1 NOT NULL
);



--
-- Name: permissions_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.permissions ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."permissions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: project_categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.project_categories (
    id integer NOT NULL,
    name character varying(150) NOT NULL,
    description text,
    deleted_flag integer DEFAULT 1,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);



--
-- Name: project_categories_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.project_categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: project_categories_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.project_categories_id_seq OWNED BY public.project_categories.id;


--
-- Name: projects; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.projects (
    id integer NOT NULL,
    name character varying(200) NOT NULL,
    description text NOT NULL,
    category character varying(100) NOT NULL,
    status character varying(50) DEFAULT 'In Progress'::character varying,
    priority character varying(50) DEFAULT 'Medium'::character varying,
    lead_name character varying(150) NOT NULL,
    progress_percentage integer DEFAULT 0,
    due_date character varying(100) NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    deleted_flag integer DEFAULT 1
);



--
-- Name: projects_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.projects_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: projects_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.projects_id_seq OWNED BY public.projects.id;


--
-- Name: purchases; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.purchases (
    id integer NOT NULL,
    approval_request_id integer NOT NULL,
    item_name character varying(200) NOT NULL,
    category character varying(100) NOT NULL,
    quantity integer DEFAULT 1 NOT NULL,
    estimated_amount numeric(12,2),
    employee_name character varying(150) NOT NULL,
    employee_email character varying(150) NOT NULL,
    department_name character varying(150),
    vendor_name character varying(200) NOT NULL,
    vendor_contact character varying(100),
    vendor_email character varying(150),
    quotation_number character varying(100),
    quotation_amount numeric(12,2) DEFAULT 0.00 NOT NULL,
    quotation_date timestamp with time zone DEFAULT (now() AT TIME ZONE 'utc'::text),
    delivery_timeline character varying(150),
    payment_terms character varying(150),
    notes text,
    status character varying(50) DEFAULT 'Quotation Received'::character varying NOT NULL,
    created_by_user_id integer NOT NULL,
    created_by_name character varying(150),
    created_at timestamp with time zone DEFAULT (now() AT TIME ZONE 'utc'::text) NOT NULL,
    updated_at timestamp with time zone,
    deleted_flag integer DEFAULT 1 NOT NULL
);



--
-- Name: purchases_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.purchases_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: purchases_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.purchases_id_seq OWNED BY public.purchases.id;


--
-- Name: report_categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.report_categories (
    id integer NOT NULL,
    name character varying(150) NOT NULL,
    description text,
    deleted_flag integer DEFAULT 1,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);



--
-- Name: report_categories_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.report_categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: report_categories_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.report_categories_id_seq OWNED BY public.report_categories.id;


--
-- Name: reports; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.reports (
    id integer NOT NULL,
    title character varying(200) NOT NULL,
    description text NOT NULL,
    category character varying(100) NOT NULL,
    format character varying(50) NOT NULL,
    created_by character varying(150) NOT NULL,
    status character varying(50) DEFAULT 'Generated'::character varying,
    file_size character varying(50) DEFAULT '1.2 MB'::character varying,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    deleted_flag integer DEFAULT 1,
    category_id integer,
    file_name character varying(255)
);



--
-- Name: reports_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.reports_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: reports_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.reports_id_seq OWNED BY public.reports.id;


--
-- Name: rolepermissions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rolepermissions (
    "RoleId" integer NOT NULL,
    "PermissionId" integer NOT NULL,
    "Id" integer NOT NULL
);



--
-- Name: rolepermissions_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public."rolepermissions_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: rolepermissions_Id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public."rolepermissions_Id_seq" OWNED BY public.rolepermissions."Id";


--
-- Name: roles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.roles (
    "Id" integer NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(255) DEFAULT ''::character varying NOT NULL,
    "DeletedFlag" smallint DEFAULT 1 NOT NULL
);



--
-- Name: roles_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.roles ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."roles_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: schedules; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.schedules (
    id integer NOT NULL,
    title character varying(200) NOT NULL,
    description text NOT NULL,
    event_type character varying(100) NOT NULL,
    event_date character varying(100) NOT NULL,
    start_time character varying(50) NOT NULL,
    end_time character varying(50) NOT NULL,
    location character varying(150) DEFAULT 'Virtual / Workspace'::character varying,
    organizer character varying(150) NOT NULL,
    status character varying(50) DEFAULT 'Scheduled'::character varying,
    priority character varying(50) DEFAULT 'Normal'::character varying,
    attendees_count integer DEFAULT 1,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    deleted_flag integer DEFAULT 1
);



--
-- Name: schedules_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.schedules_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: schedules_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.schedules_id_seq OWNED BY public.schedules.id;


--
-- Name: setting_categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.setting_categories (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    description text NOT NULL,
    icon character varying(100) DEFAULT 'Tune'::character varying,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    created_by character varying(150) DEFAULT 'System Admin'::character varying,
    deleted_flag integer DEFAULT 1
);



--
-- Name: setting_categories_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.setting_categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: setting_categories_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.setting_categories_id_seq OWNED BY public.setting_categories.id;


--
-- Name: system_settings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.system_settings (
    id integer NOT NULL,
    setting_key character varying(100) NOT NULL,
    setting_value text NOT NULL,
    category character varying(100) NOT NULL,
    description text NOT NULL,
    data_type character varying(50) DEFAULT 'string'::character varying,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_by character varying(150) DEFAULT 'System Admin'::character varying
);



--
-- Name: system_settings_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.system_settings_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: system_settings_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.system_settings_id_seq OWNED BY public.system_settings.id;


--
-- Name: user_sessions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_sessions (
    id integer NOT NULL,
    user_id integer NOT NULL,
    email character varying(150) NOT NULL,
    user_name character varying(150) NOT NULL,
    ip_address character varying(50) DEFAULT '127.0.0.1'::character varying,
    user_agent text,
    login_time timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    logout_time timestamp without time zone,
    session_token character varying(500),
    is_active boolean DEFAULT true,
    deleted_flag integer DEFAULT 1
);



--
-- Name: user_sessions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.user_sessions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: user_sessions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_sessions_id_seq OWNED BY public.user_sessions.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    "Id" integer NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "Password" character varying(255) NOT NULL,
    "Phone" character varying(20) NOT NULL,
    "Age" integer NOT NULL,
    "Address" character varying(255) NOT NULL,
    "RoleId" integer,
    "DeletedFlag" smallint DEFAULT 1 NOT NULL,
    "DesignationId" integer,
    "IsFirstLogin" boolean DEFAULT false
);



--
-- Name: users_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.users ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."users_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: approval_requests id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.approval_requests ALTER COLUMN id SET DEFAULT nextval('public.approval_requests_id_seq'::regclass);


--
-- Name: audit_logs id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs ALTER COLUMN id SET DEFAULT nextval('public.audit_logs_id_seq'::regclass);


--
-- Name: departmentpermissions Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departmentpermissions ALTER COLUMN "Id" SET DEFAULT nextval('public."departmentpermissions_Id_seq"'::regclass);


--
-- Name: departments Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments ALTER COLUMN "Id" SET DEFAULT nextval('public."departments_Id_seq"'::regclass);


--
-- Name: designations Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.designations ALTER COLUMN "Id" SET DEFAULT nextval('public."designations_Id_seq"'::regclass);


--
-- Name: event_types id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.event_types ALTER COLUMN id SET DEFAULT nextval('public.event_types_id_seq'::regclass);


--
-- Name: invoice_items id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoice_items ALTER COLUMN id SET DEFAULT nextval('public.invoice_items_id_seq'::regclass);


--
-- Name: invoices id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoices ALTER COLUMN id SET DEFAULT nextval('public.invoices_id_seq'::regclass);


--
-- Name: project_categories id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project_categories ALTER COLUMN id SET DEFAULT nextval('public.project_categories_id_seq'::regclass);


--
-- Name: projects id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.projects ALTER COLUMN id SET DEFAULT nextval('public.projects_id_seq'::regclass);


--
-- Name: purchases id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchases ALTER COLUMN id SET DEFAULT nextval('public.purchases_id_seq'::regclass);


--
-- Name: report_categories id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.report_categories ALTER COLUMN id SET DEFAULT nextval('public.report_categories_id_seq'::regclass);


--
-- Name: reports id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reports ALTER COLUMN id SET DEFAULT nextval('public.reports_id_seq'::regclass);


--
-- Name: rolepermissions Id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rolepermissions ALTER COLUMN "Id" SET DEFAULT nextval('public."rolepermissions_Id_seq"'::regclass);


--
-- Name: schedules id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedules ALTER COLUMN id SET DEFAULT nextval('public.schedules_id_seq'::regclass);


--
-- Name: setting_categories id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.setting_categories ALTER COLUMN id SET DEFAULT nextval('public.setting_categories_id_seq'::regclass);


--
-- Name: system_settings id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_settings ALTER COLUMN id SET DEFAULT nextval('public.system_settings_id_seq'::regclass);


--
-- Name: user_sessions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_sessions ALTER COLUMN id SET DEFAULT nextval('public.user_sessions_id_seq'::regclass);


--
-- Data for Name: approval_requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.approval_requests (id, user_id, employee_name, employee_email, department_name, item_name, category, description, quantity, priority, estimated_amount, status, comments, reviewed_by_id, reviewed_by_name, reviewed_at, created_at, updated_at, deleted_flag) VALUES (16, 3, 'test', 'test@gmail.com', 'test', 'Mechanical Ergonomic Keyboard', 'Peripherals & Accessories', 'need keyboard for my device', 1, 'Medium', 14500.00, 'Approved', 'Approved for allocation.', 4, 'manager', '2026-08-28 11:19:42.398464+05:30', '2026-08-28 11:19:27.997668+05:30', '2026-08-28 11:19:42.398467+05:30', 1);
INSERT INTO public.approval_requests (id, user_id, employee_name, employee_email, department_name, item_name, category, description, quantity, priority, estimated_amount, status, comments, reviewed_by_id, reviewed_by_name, reviewed_at, created_at, updated_at, deleted_flag) VALUES (17, 3, 'test', 'test@gmail.com', 'test', 'mouse', 'Hardware & Devices', 'need mouse for my system', 1, 'Medium', 300.00, 'Approved', 'Approved for allocation.', 4, 'manager', '2026-08-28 12:19:20.892194+05:30', '2026-08-28 12:19:05.707275+05:30', '2026-08-28 12:19:20.892194+05:30', 1);


--
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (1, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 08:52:03 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 14:22:03.236588', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (2, '2FA Login Verification', 'Auth', 'Vengadesh M', 'User 2FA verified and signed in from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 14:22:25.441912', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (3, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 08:55:38 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 14:25:38.093469', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (4, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 14:25:40.184467', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (5, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 14:30:24.573336', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (6, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-26 09:00:41 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 14:30:41.260728', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (7, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 10:00:31 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:30:31.872116', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (8, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:32:53.375275', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (9, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:34:45.701621', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (10, 'User Login', 'Auth', 'System Administrator', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:47:31.686845', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (11, 'User Login', 'Auth', 'New Employee User', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:47:37.295189', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (12, 'User Login', 'Auth', 'New Employee User', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:47:37.682873', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (13, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 10:20:14 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:50:14.0153', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (14, 'User Login', 'Auth', 'ansukumar', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:50:32.372743', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (15, 'User Logout', 'Auth', 'ansukumar', 'User logged out at 2026-08-26 10:21:31 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:51:31.255022', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (16, 'User Login', 'Auth', 'ansukumar', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:51:38.239917', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (17, 'User Logout', 'Auth', 'ansukumar', 'User logged out at 2026-08-26 10:21:57 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 15:51:57.620192', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (18, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:01:18.346317', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (19, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #12 for user Vengadesh M (vengadesh.kc@gmail.com)', '127.0.0.1', 'Success', '2026-08-26 16:02:19.051913', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (20, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 10:32:19 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:02:19.640447', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (21, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:02:21.835812', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (22, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #9 for user New Employee User (testuser_1441067226@example.com)', '127.0.0.1', 'Success', '2026-08-26 16:02:30.990833', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (23, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #7 for user System Administrator (admin@example.com)', '127.0.0.1', 'Success', '2026-08-26 16:03:37.625543', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (24, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #6 for user test (test@gmail.com)', '127.0.0.1', 'Success', '2026-08-26 16:03:39.747731', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (25, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 10:38:37 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:08:37.328433', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (26, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:08:43.632233', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (27, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-26 10:38:55 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:08:55.963223', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (28, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:08:59.575618', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (29, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 11:02:14 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:32:14.039527', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (30, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:32:23.296863', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (31, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 11:14:52 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:44:52.578302', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (32, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:45:25.333956', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (33, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:47:29.659098', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (34, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-26 11:17:39 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:47:39.889308', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (35, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 11:22:49 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:52:49.044127', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (36, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:52:52.290596', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (37, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 11:26:29 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:56:29.551091', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (38, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:56:32.463861', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (39, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 11:26:37 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:56:37.454349', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (40, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 16:56:39.239749', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (41, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 11:33:23 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 17:03:23.996442', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (42, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 17:03:29.068175', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (43, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:06:59.771053', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (44, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:07:04.136632', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (45, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:07:09.204732', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (46, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:07:15.407035', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (47, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 12:38:06 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:08:06.1918', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (48, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:09:22.849688', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (49, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 12:40:08 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:10:08.226741', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (50, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:10:13.050828', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (51, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-26 12:40:18 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:10:18.154084', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (52, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:10:21.943183', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (53, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-26 12:40:51 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:10:51.94989', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (54, 'User Login', 'Auth', 'ansukumar', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:10:56.93362', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (55, 'User Logout', 'Auth', 'ansukumar', 'User logged out at 2026-08-26 12:41:01 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:11:01.50866', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (56, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:11:05.670036', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (57, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-26 12:41:08 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:11:08.894506', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (58, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:11:13.586001', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (59, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 13:00:49 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:30:49.394837', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (60, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:30:53.184384', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (61, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-26 13:04:02 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:34:02.30381', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (62, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:34:06.691688', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (63, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-26 13:04:17 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:34:17.803566', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (64, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:35:20.762033', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (65, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:36:34.026395', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (66, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-26 13:06:45 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-26 18:36:45.010325', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (67, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 08:50:54.482604', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (68, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #36 for user Vengadesh M (vengadesh.kc@gmail.com)', '127.0.0.1', 'Success', '2026-08-27 08:51:05.654473', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (69, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 03:21:06 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 08:51:06.278795', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (70, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 08:51:08.068415', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (71, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 04:37:15 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:07:15.479282', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (72, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:07:19.968147', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (73, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 04:37:50 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:07:50.660353', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (74, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:07:55.077355', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (75, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 04:38:45 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:08:45.665425', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (76, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:08:49.254259', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (77, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 04:56:44 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:26:44.454614', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (78, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:26:48.289644', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (79, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 04:57:32 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:27:32.906605', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (80, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:27:36.450974', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (81, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 10:46:27.410519', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (82, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:06:05.826117', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (83, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:06:15.997673', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (84, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:06:27.413361', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (85, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:06:39.912554', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (86, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:07:14.589649', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (87, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:10:30.041541', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (88, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-27 05:42:38 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:12:38.614812', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (89, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:13:52.571612', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (90, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:16:41.712018', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (91, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:19:12.222461', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (92, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:21:24.184838', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (93, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:21:40.673036', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (94, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 05:53:42 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:23:42.076734', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (95, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:23:46.02462', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (96, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 05:53:55 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:23:55.270231', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (97, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:25:19.149891', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (98, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 05:59:04 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:29:04.425503', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (99, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:29:58.434888', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (100, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 06:28:21 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 11:58:21.823367', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (101, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:48:06.508887', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (102, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 08:18:26 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:48:26.853676', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (103, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:48:31.084677', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (104, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:50:30.719574', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (105, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 08:21:40 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:51:40.539911', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (106, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:51:44.708036', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (107, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:52:03.909159', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (108, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 08:27:07 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:57:07.103456', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (109, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-27 08:27:10 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:57:10.960172', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (110, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:57:14.722306', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (111, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:57:50.224068', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (112, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 08:28:51 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:58:51.932522', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (113, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:58:56.501574', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (114, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 08:29:24 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:59:24.517924', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (115, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:59:28.437645', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (116, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 13:59:56.912748', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (117, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 08:30:22 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:00:22.532241', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (118, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 08:41:44 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:11:44.091945', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (119, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:11:50.839492', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (120, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 08:44:56 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:14:56.371875', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (121, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:15:00.311633', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (122, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-27 08:47:57 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:17:57.150703', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (123, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:18:01.051473', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (124, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 08:48:30 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:18:30.581208', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (125, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:18:34.345908', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (126, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-27 08:48:59 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:18:59.291143', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (127, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:19:02.597433', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (128, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 08:49:11 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:19:11.079493', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (129, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:19:15.004427', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (130, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 08:49:30 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:19:30.10733', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (131, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:19:34.028716', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (132, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-27 08:51:17 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:21:17.907396', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (133, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:21:21.63388', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (134, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 08:53:03 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:23:03.235019', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (135, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:23:06.844252', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (136, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 09:06:40 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:36:40.102641', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (137, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:36:43.988206', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (138, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 09:08:17 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:38:17.571157', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (139, 'User Login', 'Auth', 'ansukumar', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:38:23.030764', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (140, 'User Logout', 'Auth', 'ansukumar', 'User logged out at 2026-08-27 09:08:48 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:38:48.648277', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (141, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 14:38:53.16695', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (142, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 09:32:34 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:02:34.278286', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (143, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:02:38.308102', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (144, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 09:35:19 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:05:19.355085', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (145, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:20:09.716602', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (146, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 09:56:48 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:26:48.316587', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (147, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:26:51.858837', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (148, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-27 10:04:53 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:34:53.865899', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (149, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:38:25.27194', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (150, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 10:14:18 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:44:18.351329', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (151, 'User Login', 'Auth', 'Ansu kumar', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:45:42.703959', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (152, 'User Logout', 'Auth', 'Ansu kumar', 'User logged out at 2026-08-27 10:15:49 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 15:45:49.000698', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (153, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 16:11:18.773729', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (154, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 10:46:39 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 16:16:39.28062', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (155, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 16:33:38.10794', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (156, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 11:10:45 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 16:40:45.151663', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (157, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 16:41:04.64848', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (158, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 11:32:37 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:02:37.082043', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (159, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:02:41.339338', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (160, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 11:32:46 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:02:46.283102', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (161, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:03:55.203964', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (162, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-27 11:54:46 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:24:46.175923', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (163, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:25:41.725835', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (164, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 12:07:40 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:37:40.125495', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (165, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:37:41.78887', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (166, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 12:18:32 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:48:32.523137', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (167, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:48:37.371307', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (168, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-27 12:18:46 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:48:46.905812', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (169, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:48:50.467111', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (170, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-27 12:22:48 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:52:48.394636', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (171, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:52:50.692697', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (172, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-27 17:58:01.792233', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (173, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #99 for user test (test@gmail.com)', '127.0.0.1', 'Success', '2026-08-27 17:58:14.513862', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (174, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 08:44:26.943445', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (175, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 10:47:46.565034', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (176, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #100 for user Vengadesh M (vengadesh.kc@gmail.com)', '127.0.0.1', 'Success', '2026-08-28 10:47:54.822916', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (177, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-28 05:17:55 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 10:47:55.597064', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (178, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 10:47:58.466238', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (179, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:10:31.89975', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (180, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #104 for user Vengadesh M (vengadesh.kc@gmail.com)', '127.0.0.1', 'Success', '2026-08-28 11:10:44.331205', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (181, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-28 05:40:44 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:10:44.849158', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (182, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:10:46.894886', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (183, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-28 05:49:01 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:19:01.483923', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (184, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:19:06.796726', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (185, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-28 05:49:33 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:19:33.012759', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (186, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:19:36.805208', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (187, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-28 05:49:47 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:19:47.460975', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (188, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:19:51.629129', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (189, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-28 06:16:54 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:46:54.692412', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (190, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:46:58.620342', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (191, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-28 06:17:26 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:47:26.460979', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (192, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:47:31.237438', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (193, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-28 06:21:30 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:51:30.00424', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (194, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:51:34.622241', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (195, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-28 06:23:01 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:53:01.667161', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (196, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:53:03.957677', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (197, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-28 06:24:21 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:54:21.12466', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (198, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 11:54:24.865806', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (199, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-28 06:48:31 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 12:18:31.747959', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (200, 'User Login', 'Auth', 'test', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 12:18:36.484279', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (201, 'User Logout', 'Auth', 'test', 'User logged out at 2026-08-28 06:49:10 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 12:19:10.48708', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (202, 'User Login', 'Auth', 'manager', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 12:19:14.860793', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (203, 'User Logout', 'Auth', 'manager', 'User logged out at 2026-08-28 06:56:52 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 12:26:52.446347', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (204, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 12:26:56.190094', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (205, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 12:49:16.148852', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (206, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 14:32:30.815894', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (207, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 14:32:55.262492', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (208, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 15:27:24.889332', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (209, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #120 for user Vengadesh M (vengadesh.kc@gmail.com)', '127.0.0.1', 'Success', '2026-08-28 16:38:51.24592', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (210, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-28 11:08:52 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 16:38:52.029146', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (211, 'User Login', 'Auth', 'Vengadesh M', 'User logged in successfully from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 16:38:54.391085', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (212, 'User Logout', 'Auth', 'Vengadesh M', 'User logged out at 2026-08-28 11:36:05 UTC from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 17:06:05.402077', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (213, '2FA Login Verification', 'Auth', 'Vengadesh M', 'User 2FA verified and signed in from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 17:07:04.428128', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (214, '2FA Login Verification', 'Auth', 'test', 'User 2FA verified and signed in from IP 127.0.0.1', '127.0.0.1', 'Success', '2026-08-28 17:08:45.965648', 1);
INSERT INTO public.audit_logs (id, action, module, performed_by, details, ip_address, status, created_at, deleted_flag) VALUES (215, 'Force Terminate Session', 'Auth', 'Vengadesh M', 'Terminated active session #125 for user test (test@gmail.com)', '127.0.0.1', 'Success', '2026-08-28 17:09:02.746714', 1);


--
-- Data for Name: departmentpermissions; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (5, 2, 17);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (6, 2, 21);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (7, 2, 22);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (8, 2, 40);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (9, 2, 43);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (10, 3, 17);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (11, 3, 1);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (12, 3, 6);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (13, 3, 7);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (14, 3, 43);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (15, 3, 18);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (21, 5, 17);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (22, 5, 19);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (23, 5, 20);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (24, 5, 18);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (25, 5, 43);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (26, 6, 17);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (27, 6, 19);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (28, 6, 18);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (29, 6, 43);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (30, 7, 17);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (31, 7, 19);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (32, 7, 20);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (33, 7, 43);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (34, 4, 17);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (35, 4, 18);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (36, 4, 19);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (37, 4, 20);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (38, 4, 43);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (39, 4, 1);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (40, 4, 3);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (41, 4, 21);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (42, 4, 22);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (43, 4, 40);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (56, 2, 48);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (57, 2, 49);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (58, 3, 48);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (59, 3, 49);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (60, 4, 48);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (61, 4, 49);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (62, 5, 48);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (63, 5, 49);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (64, 6, 48);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (65, 6, 49);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (66, 7, 48);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (67, 7, 49);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (68, 1, 1);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (69, 1, 3);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (70, 1, 17);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (71, 1, 18);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (72, 1, 19);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (73, 1, 20);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (74, 1, 21);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (75, 1, 22);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (76, 1, 40);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (77, 1, 43);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (78, 1, 48);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (79, 1, 49);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (80, 1, 6);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (81, 3, 51);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (82, 3, 52);
INSERT INTO public.departmentpermissions ("Id", "DepartmentId", "PermissionId") VALUES (83, 3, 53);


--
-- Data for Name: departments; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.departments ("Id", "Name", "Description", "DeletedFlag", "CreatedAt") VALUES (3, 'Human Resources', 'People operations, talent acquisition, and employee relations.', 1, '2026-08-27 09:53:18.044161+05:30');
INSERT INTO public.departments ("Id", "Name", "Description", "DeletedFlag", "CreatedAt") VALUES (4, 'Product Management', 'Product roadmaps, feature strategy, and delivery management.', 1, '2026-08-27 09:53:18.04543+05:30');
INSERT INTO public.departments ("Id", "Name", "Description", "DeletedFlag", "CreatedAt") VALUES (5, 'Project Management', 'Project execution, sprint planning, and team coordination.', 1, '2026-08-27 09:53:18.046501+05:30');
INSERT INTO public.departments ("Id", "Name", "Description", "DeletedFlag", "CreatedAt") VALUES (6, 'Quality Assurance', 'Software test automation, QA verification, and release standards.', 1, '2026-08-27 09:53:18.04751+05:30');
INSERT INTO public.departments ("Id", "Name", "Description", "DeletedFlag", "CreatedAt") VALUES (7, 'UI/UX Design', 'User experience research, visual design, and interface design systems.', 1, '2026-08-27 09:53:18.048891+05:30');
INSERT INTO public.departments ("Id", "Name", "Description", "DeletedFlag", "CreatedAt") VALUES (1, 'Software Development', 'Core engineering, application architecture, and development teams.', 1, '2026-08-27 09:53:18.002211+05:30');
INSERT INTO public.departments ("Id", "Name", "Description", "DeletedFlag", "CreatedAt") VALUES (2, 'DevOps & Infrastructure', 'Cloud deployment pipelines, observability, and server maintenance.', 1, '2026-08-27 09:53:18.042773+05:30');
INSERT INTO public.departments ("Id", "Name", "Description", "DeletedFlag", "CreatedAt") VALUES (8, 'test', '', 1, '2026-08-27 11:11:36.779594+05:30');


--
-- Data for Name: designations; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (1, 'Software Engineer', 'Develops and maintains core applications and services.', 1, '2026-08-25 12:24:44.08242', 1);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (2, 'Senior Software Engineer', 'Leads feature development and system architecture.', 1, '2026-08-25 12:24:44.08242', 1);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (3, 'Frontend Developer', 'Builds responsive and interactive user interfaces.', 1, '2026-08-25 12:24:44.08242', 1);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (4, 'Backend Developer', 'Designs robust server APIs, microservices, and databases.', 1, '2026-08-25 12:24:44.08242', 1);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (5, 'Full Stack Developer', 'Works across client and server application stack.', 1, '2026-08-25 12:24:44.08242', 1);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (7, 'QA Engineer', 'Executes test automation and quality assurance workflows.', 1, '2026-08-25 12:24:44.08242', 6);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (8, 'UI/UX Designer', 'Creates user experience designs, wireframes, and design systems.', 1, '2026-08-25 12:24:44.08242', 7);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (9, 'Product Manager', 'Defines product roadmap and oversees feature delivery.', 1, '2026-08-25 12:24:44.08242', 4);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (10, 'Project Manager', 'Coordinates team deliverables, sprint milestones, and timelines.', 1, '2026-08-25 12:24:44.08242', 5);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (12, 'HR Manager', 'Oversees talent acquisition, onboarding, and people operations.', 1, '2026-08-25 12:24:44.08242', 3);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (6, 'DevOps Engineer', 'Manages CI/CD pipelines, cloud infrastructure, and releases.', 1, '2026-08-25 12:24:44.08242', 2);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (11, 'System Administrator', 'Monitors IT infrastructure, networks, and server health.', 1, '2026-08-25 12:24:44.08242', 2);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (27, 'manager', '', 1, '2026-08-27 11:55:41.38843', 3);
INSERT INTO public.designations ("Id", "Name", "Description", "DeletedFlag", "CreatedAt", "DepartmentId") VALUES (26, 'test', 'test', 1, '2026-08-27 11:11:28.015284', 8);


--
-- Data for Name: event_types; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.event_types (id, name, description, color, icon, created_at, created_by, deleted_flag) VALUES (1, 'Audit', 'Compliance, security and access audit sessions', '#6366f1', 'FactCheck', '2026-08-25 09:25:52.456924', 'System Admin', 1);
INSERT INTO public.event_types (id, name, description, color, icon, created_at, created_by, deleted_flag) VALUES (2, 'Training', 'Staff learning and skill certification workshops', '#10b981', 'School', '2026-08-25 09:25:52.45731', 'System Admin', 1);
INSERT INTO public.event_types (id, name, description, color, icon, created_at, created_by, deleted_flag) VALUES (3, 'Governance', 'Executive committee and policy oversight', '#f59e0b', 'Gavel', '2026-08-25 09:25:52.457311', 'System Admin', 1);
INSERT INTO public.event_types (id, name, description, color, icon, created_at, created_by, deleted_flag) VALUES (4, 'Review', 'Sprint, code, and access governance reviews', '#0ea5e9', 'RateReview', '2026-08-25 09:25:52.457311', 'System Admin', 1);
INSERT INTO public.event_types (id, name, description, color, icon, created_at, created_by, deleted_flag) VALUES (5, 'Certification', 'System and credentials re-certification', '#f43f5e', 'Verified', '2026-08-25 09:25:52.457311', 'System Admin', 1);


--
-- Data for Name: invoice_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.invoice_items (id, invoice_id, product_name, description, quantity, unit_price, tax_rate, tax_amount, total_amount, order_index, deleted_flag) VALUES (1, 1, 'Enterprise Cloud Architecture Consulting', 'Architecture assessment and multi-tenant microservices deployment setup.', 1, 60000.00, 18.00, 10800.00, 70800.00, 1, 1);
INSERT INTO public.invoice_items (id, invoice_id, product_name, description, quantity, unit_price, tax_rate, tax_amount, total_amount, order_index, deleted_flag) VALUES (2, 1, 'DevOps CI/CD Automation & Security Audit', 'Kubernetes orchestration pipelines, secret scanning, and automated release gates.', 1, 40000.00, 18.00, 7200.00, 47200.00, 2, 1);
INSERT INTO public.invoice_items (id, invoice_id, product_name, description, quantity, unit_price, tax_rate, tax_amount, total_amount, order_index, deleted_flag) VALUES (3, 2, 'UI/UX Design System & Mobile App Prototype', 'Figma design system tokens, responsive component wireframes, and design specs.', 1, 45000.00, 18.00, 8100.00, 53100.00, 1, 1);
INSERT INTO public.invoice_items (id, invoice_id, product_name, description, quantity, unit_price, tax_rate, tax_amount, total_amount, order_index, deleted_flag) VALUES (4, 3, 'Full-Stack Enterprise Web Application', 'React 19, TypeScript, TailwindCSS and ASP.NET 10 REST API backend', 2, 75000.00, 18.00, 27000.00, 177000.00, 1, 0);
INSERT INTO public.invoice_items (id, invoice_id, product_name, description, quantity, unit_price, tax_rate, tax_amount, total_amount, order_index, deleted_flag) VALUES (5, 3, 'Database Performance Optimization & Backup', 'PostgreSQL indexing, query tuning, and disaster recovery snapshot configuration', 1, 30000.00, 18.00, 5400.00, 35400.00, 2, 0);
INSERT INTO public.invoice_items (id, invoice_id, product_name, description, quantity, unit_price, tax_rate, tax_amount, total_amount, order_index, deleted_flag) VALUES (7, 4, 'SERVER', 'test', 1, 10000.00, 18.00, 1800.00, 11800.00, 1, 1);


--
-- Data for Name: invoices; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.invoices (id, invoice_number, customer_name, customer_email, customer_phone, customer_address, customer_gstin, company_gstin, invoice_date, due_date, subtotal, tax_rate, tax_amount, discount_amount, total_amount, total_amount_in_words, status, payment_method, notes, terms_and_conditions, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (1, 'INV-2026-0001', 'Acme Global Solutions Pvt Ltd', 'billing@acmeglobal.com', '+91 98765 43210', 'Plot 42, Hitec City, Phase II, Hyderabad, Telangana 500081', '36AACCA1234F1Z9', '36AAAAA0000A1Z5', '2026-08-23 14:18:48.322941+05:30', '2026-09-07 14:18:48.323034+05:30', 100000.00, 18.00, 18000.00, 0.00, 118000.00, 'Rupees One Lakh Eighteen Thousand Only', 'Paid', 'Bank Transfer', 'Annual Enterprise Cloud Architecture & Microservices Consulting retainer fee.', 'Payment due within 15 days of invoice issue date. 18% GST applicable as per Indian Tax rules.', 3, 'test', '2026-08-23 14:18:48.323928+05:30', NULL, 1);
INSERT INTO public.invoices (id, invoice_number, customer_name, customer_email, customer_phone, customer_address, customer_gstin, company_gstin, invoice_date, due_date, subtotal, tax_rate, tax_amount, discount_amount, total_amount, total_amount_in_words, status, payment_method, notes, terms_and_conditions, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (2, 'INV-2026-0002', 'Zenith Infotech Ltd', 'accounts@zenithinfotech.io', '+91 91234 56789', 'Cyber Towers, 4th Floor, Whitefield, Bangalore, Karnataka 560066', '29AAACZ9876E1Z2', '36AAAAA0000A1Z5', '2026-08-27 14:18:48.324986+05:30', '2026-09-11 14:18:48.325+05:30', 45000.00, 18.00, 8100.00, 0.00, 53100.00, 'Rupees Fifty-Three Thousand One Hundred Only', 'Pending', 'UPI', 'Custom UI/UX Design System and Front-end component toolkit delivery.', 'Standard payment terms apply. Please remit payment via bank NEFT/RTGS or UPI.', 3, 'test', '2026-08-27 14:18:48.325002+05:30', NULL, 1);
INSERT INTO public.invoices (id, invoice_number, customer_name, customer_email, customer_phone, customer_address, customer_gstin, company_gstin, invoice_date, due_date, subtotal, tax_rate, tax_amount, discount_amount, total_amount, total_amount_in_words, status, payment_method, notes, terms_and_conditions, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (3, 'INV-2026-0003', 'HyperScale Innovations Pvt Ltd', 'finance@hyperscale.io', '+91 98888 77777', 'Level 8, Orbit Tech Hub, Hyderabad', '36AAACH9999K1Z4', '36AAAAA0000A1Z5', '2026-08-28 14:32:55.428744+05:30', '2026-09-12 14:32:55.428744+05:30', 180000.00, 18.00, 32400.00, 0.00, 212400.00, 'Rupees Two Lakh Twelve Thousand Four Hundred Only', 'Pending', 'Bank Transfer', 'Custom Full-Stack Web App & Cloud Migration', NULL, 2, 'Vengadesh M', '2026-08-28 14:32:55.43214+05:30', '2026-08-28 14:32:55.544613+05:30', 0);
INSERT INTO public.invoices (id, invoice_number, customer_name, customer_email, customer_phone, customer_address, customer_gstin, company_gstin, invoice_date, due_date, subtotal, tax_rate, tax_amount, discount_amount, total_amount, total_amount_in_words, status, payment_method, notes, terms_and_conditions, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (4, 'INV-2026-0004', 'Ansu kumar', 'ansukumar@gmail.com', '8715458420', 'erode', 'HSGE4T3JFJDVJBFDG', '36AAAAA0000A1Z5', '2026-08-28 05:30:00+05:30', '2026-09-12 05:30:00+05:30', 10000.00, 18.00, 1800.00, 0.00, 11800.00, 'Rupees Eleven Thousand Eight Hundred Only', 'Paid', 'Bank Transfer', 'Thank you for doing business with NavaNala Technologies.', '1. Payment due within 15 days of invoice issue.
2. 18% GST applicable as per standard Indian Tax guidelines.
3. Remit payments via Bank Transfer / NEFT / RTGS or UPI.', 2, 'Vengadesh M', '2026-08-28 15:30:22.000356+05:30', '2026-08-28 15:30:35.876214+05:30', 1);


--
-- Data for Name: menus; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (1, 'dashboard.view', 'Dashboard', 'â—«', '/dashboard', 'Core Access', 'System metrics & access summary', 1, 'dashboard.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (2, 'users.view', 'User Directory', 'â–¦', '/add-user', 'Core Access', 'Manage members & assign roles', 2, 'users.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (3, 'roles.view', 'Roles', 'â™™', '/roles', 'Core Access', 'Configure workspace roles', 3, 'roles.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (4, 'permissions.manage', 'Permission Matrix', 'âš¿', '/permissions', 'Core Access', 'Role permission assignments', 4, 'permissions.manage', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (5, 'audit.view', 'Audit Logs', 'â—Œ', '/audit', 'Operations & Audit', 'Activity & security events', 5, 'audit.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (6, 'reports.view', 'Reports', 'â–¤', '/reports', 'Operations & Audit', 'Insights & exports', 6, 'reports.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (7, 'projects.view', 'Projects', 'â—‡', '/projects', 'Operations & Audit', 'Project initiatives', 7, 'projects.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (8, 'calendar.view', 'Schedule', 'â–¡', '/calendar', 'Operations & Audit', 'Team rhythm & reviews', 8, 'calendar.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (9, 'settings.view', 'Settings', 'âš™', '/settings', 'Preferences', 'Workspace configuration', 9, 'settings.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (19, 'user_activity.view', 'User Activity', 'â±', '/user-activity', 'Operations & Audit', 'Live active sessions & login/logout tracking', 6, 'user_activity.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (30, 'departments.view', 'Departments', 'ðŸ¢', '/departments', 'Core Access', 'Department hierarchy & designation mapping', 4, 'departments.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (31, 'approvals.view', 'Create Approval', 'âœ“', '/create-approval', 'Management', 'Raise and manage employee product & resource approvals', 6, 'approvals.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (32, 'purchases.view', 'Purchases', 'ðŸ›’', '/purchases', 'Management', 'Procure approved products and manage vendor quotations', 7, 'purchases.view', 1);
INSERT INTO public.menus (id, menukey, label, icon, route, groupname, description, orderindex, permissionkey, deletedflag) VALUES (33, 'invoices.view', 'Invoice', 'ðŸ§¾', '/invoices', 'Management', 'Generate and manage customer invoices with GST calculations and PDF download', 8, 'invoices.view', 1);


--
-- Data for Name: permissions; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (1, 'users.view', 'View users', 'See the user directory', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (2, 'users.manage', 'Manage users', 'Create, edit, and delete users', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (3, 'roles.view', 'View roles', 'See workspace roles', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (4, 'roles.manage', 'Manage roles', 'Create, edit, and delete roles', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (5, 'permissions.manage', 'Manage permissions', 'Assign permissions to roles', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (6, 'users.create', 'Add users', 'Create new user records', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (7, 'users.edit', 'Edit users', 'Update existing user records', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (8, 'users.delete', 'Delete users', 'Delete user records', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (9, 'roles.create', 'Add roles', 'Create new role records', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (10, 'roles.edit', 'Edit roles', 'Update existing role records', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (11, 'roles.delete', 'Delete roles', 'Delete roles', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (17, 'dashboard.view', 'View dashboard', 'See the workspace dashboard', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (18, 'reports.view', 'View reports', 'See workspace reports', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (19, 'projects.view', 'View projects', 'See workspace projects', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (20, 'calendar.view', 'View calendar', 'See the workspace calendar', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (21, 'settings.view', 'View settings', 'See workspace settings', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (22, 'audit.view', 'View audit log', 'See workspace activity history', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (40, 'user_activity.view', 'View User Activity', 'Inspect user login, logout activity history and view currently active logged-in users.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (41, 'user_activity.manage', 'Manage User Activity', 'Terminate active user sessions and manage login sessions.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (42, 'user_activity.force_logout', 'Force Logout Sessions', 'Immediately terminate active user sessions and force logout members.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (43, 'departments.view', 'View Departments', 'View workspace departments and designation hierarchy.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (44, 'departments.create', 'Create Departments', 'Create new organizational departments.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (45, 'departments.edit', 'Edit Departments', 'Update department details and designation mappings.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (46, 'departments.delete', 'Delete Departments', 'Deactivate or remove departments.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (47, 'departments.manage', 'Manage Departments', 'Full administrative control over departments and designation assignments.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (48, 'approvals.view', 'View Approvals', 'Access and view create approval workspace.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (49, 'approvals.create', 'Create Approval', 'Raise approval requests for hardware, software, laptops, and resources.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (50, 'approvals.manage', 'Approve or Reject Approvals', 'Review, approve or reject employee approval requests.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (51, 'purchases.view', 'View Purchases & Quotations', 'Access approved products, vendor quotes, and procurement tracking.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (52, 'purchases.create', 'Add Vendor Quotation', 'Add supplier quotes and commercial terms for approved products.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (53, 'purchases.manage', 'Manage Procurement', 'Full control over vendor quotations, PO issue, and purchase order lifecycles.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (57, 'invoices.view', 'View Invoices', 'Access and view billing & customer invoices.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (58, 'invoices.create', 'Add Invoice', 'Create and generate customer invoices with products and calculations.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (59, 'invoices.edit', 'Edit Invoice', 'Modify existing invoice records and line items.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (60, 'invoices.delete', 'Delete Invoice', 'Remove or cancel customer invoice records.', 1);
INSERT INTO public.permissions ("Id", "PermissionKey", "Name", "Description", "DeletedFlag") VALUES (61, 'invoices.manage', 'Manage Invoices & GST', 'Full administrative authority over invoices, tax settings, and GST number configuration.', 1);


--
-- Data for Name: project_categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.project_categories (id, name, description, deleted_flag, created_at) VALUES (1, 'RBAC Rollout', 'Role-based access control rollouts and permission matrices', 1, '2026-08-26 16:21:45.787378+05:30');
INSERT INTO public.project_categories (id, name, description, deleted_flag, created_at) VALUES (2, 'DevOps', 'CI/CD pipelines, containerization, and infrastructure automation', 1, '2026-08-26 16:21:45.787378+05:30');
INSERT INTO public.project_categories (id, name, description, deleted_flag, created_at) VALUES (3, 'Security', 'Security audits, credentials rotation, and compliance', 1, '2026-08-26 16:21:45.787378+05:30');
INSERT INTO public.project_categories (id, name, description, deleted_flag, created_at) VALUES (4, 'Finance', 'Financial reporting, billing reconciliation, and budgets', 1, '2026-08-26 16:21:45.787378+05:30');
INSERT INTO public.project_categories (id, name, description, deleted_flag, created_at) VALUES (5, 'Governance', 'Access policies, compliance reviews, and governance boards', 1, '2026-08-26 16:21:45.787378+05:30');


--
-- Data for Name: projects; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.projects (id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag) VALUES (2, 'Engineering Role Segmentation', 'Configuring least-privilege matrix roles for lead developers and DevOps engineers.', 'DevOps', 'In Progress', 'High', 'Arun Kumar', 75, 'Dec 15, 2026', '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.projects (id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag) VALUES (3, 'Finance Access Scope Cleanup', 'Revoking legacy administrative credentials and configuring view-only audit scopes.', 'Finance', 'Review', 'Medium', 'Kaviya R', 90, 'Nov 30, 2026', '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.projects (id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag) VALUES (5, 'SSO & SAML Enterprise Integration', 'Connecting workspace authentication with enterprise identity provider.', 'RBAC Rollout', 'In Progress', 'High', 'Divya S', 50, 'Feb 10, 2027', '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.projects (id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag) VALUES (7, 'Compliance Audit Log Archival', 'Encrypted long-term retention and daily backup export for system audit logs.', 'Security', 'Completed', 'Low', 'kaamesh', 100, 'Oct 25, 2026', '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.projects (id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag) VALUES (6, 'Automated Role Provisioning Pipeline', 'CI/CD integration for automated role and permission assignment via API.', 'DevOps', 'Completed', 'Medium', 'Praveen K', 100, 'Jan 05, 2027', '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.projects (id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag) VALUES (4, 'Quarterly Access Certification', 'Auditing all active directory member permissions and multi-factor compliance.', 'Security', 'In Progress', 'Critical', 'Vengadesh M', 79, 'Jan 20, 2027', '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.projects (id, name, description, category, status, priority, lead_name, progress_percentage, due_date, created_at, deleted_flag) VALUES (1, 'testing', 'security improve', 'Security', 'Planning', 'High', 'Anjana', 50, 'Dec 31, 2026', '2026-08-26 14:21:43.166367', 1);


--
-- Data for Name: purchases; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.purchases (id, approval_request_id, item_name, category, quantity, estimated_amount, employee_name, employee_email, department_name, vendor_name, vendor_contact, vendor_email, quotation_number, quotation_amount, quotation_date, delivery_timeline, payment_terms, notes, status, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (1, 16, 'Mechanical Ergonomic Keyboard', 'Peripherals & Accessories', 1, 14500.00, 'test', 'test@gmail.com', 'test', 'Dell', 'rajesh/8974512360', 'rajesh@gmail.com', 'QT-0258', 14000.00, '2026-08-28 05:30:00+05:30', '3-5 Business Days', 'Net 30 Days', 'warrity has 4 years', 'PO Issued', 2, 'Vengadesh M', '2026-08-28 11:59:29.923076+05:30', NULL, 1);
INSERT INTO public.purchases (id, approval_request_id, item_name, category, quantity, estimated_amount, employee_name, employee_email, department_name, vendor_name, vendor_contact, vendor_email, quotation_number, quotation_amount, quotation_date, delivery_timeline, payment_terms, notes, status, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (3, 17, 'mouse', 'Hardware & Devices', 1, 300.00, 'test', 'test@gmail.com', 'test', 'DELL', 'ansu/8765486790', 'ansu@gmail.com', 'QT-4569', 250.00, '2026-08-28 05:30:00+05:30', '3-5 Business Days', 'Net 15 Days', '5 years warranty', 'PO Issued', 4, 'manager', '2026-08-28 12:20:18.05182+05:30', NULL, 1);
INSERT INTO public.purchases (id, approval_request_id, item_name, category, quantity, estimated_amount, employee_name, employee_email, department_name, vendor_name, vendor_contact, vendor_email, quotation_number, quotation_amount, quotation_date, delivery_timeline, payment_terms, notes, status, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (2, 16, 'Mechanical Ergonomic Keyboard', 'Peripherals & Accessories', 1, 14500.00, 'test', 'test@gmail.com', 'test', 'HP', 'saran', 'saran@gmail.com', 'QT-8547', 15000.00, '2026-08-28 05:30:00+05:30', '3-5 Business Days', 'Net 30 Days', '3 years warranty', 'Cancelled', 2, 'Vengadesh M', '2026-08-28 12:09:42.847593+05:30', '2026-08-28 12:26:15.53264+05:30', 1);
INSERT INTO public.purchases (id, approval_request_id, item_name, category, quantity, estimated_amount, employee_name, employee_email, department_name, vendor_name, vendor_contact, vendor_email, quotation_number, quotation_amount, quotation_date, delivery_timeline, payment_terms, notes, status, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (4, 17, 'mouse', 'Hardware & Devices', 1, 300.00, 'test', 'test@gmail.com', 'test', 'HP', 'lokesh/896541270', 'lokesh@gmail.com', 'QT-85410', 350.00, '2026-08-28 05:30:00+05:30', '3-5 Business Days', 'Net 30 Days', '1 year  warranty', 'Quotation Received', 4, 'manager', '2026-08-28 12:21:18.174647+05:30', '2026-08-28 12:29:36.787835+05:30', 1);
INSERT INTO public.purchases (id, approval_request_id, item_name, category, quantity, estimated_amount, employee_name, employee_email, department_name, vendor_name, vendor_contact, vendor_email, quotation_number, quotation_amount, quotation_date, delivery_timeline, payment_terms, notes, status, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag) VALUES (5, 17, 'mouse', 'Hardware & Devices', 1, 300.00, 'test', 'test@gmail.com', 'test', 'test', 'test/8569741025', 'test@gmail.com', 'QT-8521470', 300.00, '2026-08-28 05:30:00+05:30', '3-5 Business Days', 'Net 30 Days', 'sdvfds', 'Quotation Received', 2, 'Vengadesh M', '2026-08-28 16:47:20.536073+05:30', NULL, 1);


--
-- Data for Name: report_categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.report_categories (id, name, description, deleted_flag, created_at) VALUES (1, 'Compliance', 'Compliance and regulatory adherence reports', 1, '2026-08-26 16:41:32.362653+05:30');
INSERT INTO public.report_categories (id, name, description, deleted_flag, created_at) VALUES (2, 'Security', 'Security audits, vulnerability scans, and access control', 1, '2026-08-26 16:41:32.362653+05:30');
INSERT INTO public.report_categories (id, name, description, deleted_flag, created_at) VALUES (3, 'Role Mapping', 'Role-to-permission mapping and access reviews', 1, '2026-08-26 16:41:32.362653+05:30');
INSERT INTO public.report_categories (id, name, description, deleted_flag, created_at) VALUES (4, 'Access Audit', 'User login activity and privilege escalation audit', 1, '2026-08-26 16:41:32.362653+05:30');
INSERT INTO public.report_categories (id, name, description, deleted_flag, created_at) VALUES (5, 'User Directory', 'User directory exports and account status reports', 1, '2026-08-26 16:41:32.362653+05:30');
INSERT INTO public.report_categories (id, name, description, deleted_flag, created_at) VALUES (6, 'Financial Audit', 'Billing, expense reconciliation, and financial audits', 1, '2026-08-26 16:41:32.362653+05:30');
INSERT INTO public.report_categories (id, name, description, deleted_flag, created_at) VALUES (7, 'Governance', 'Organizational policies and governance oversight', 1, '2026-08-26 16:41:32.362653+05:30');
INSERT INTO public.report_categories (id, name, description, deleted_flag, created_at) VALUES (8, 'test', 'test', 1, '2026-08-26 16:43:43.794503+05:30');


--
-- Data for Name: reports; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.reports (id, title, description, category, format, created_by, status, file_size, created_at, deleted_flag, category_id) VALUES (6, 'Failed Authentication & 2FA Logs', 'Failed login attempts and two-factor authentication recovery logs.', 'Compliance', 'CSV', 'System Administrator', 'Generated', '450 KB', '2026-08-26 14:24:43.110394', 1, 1);
INSERT INTO public.reports (id, title, description, category, format, created_by, status, file_size, created_at, deleted_flag, category_id) VALUES (3, 'Privileged Access Compliance', 'Super Admin activity tracking, elevation events, and compliance logs.', 'Compliance', 'CSV', 'Audit Daemon', 'Generated', '2.4 MB', '2026-08-26 14:24:43.110394', 1, 1);
INSERT INTO public.reports (id, title, description, category, format, created_by, status, file_size, created_at, deleted_flag, category_id) VALUES (5, 'User Session Security Report', 'Active login sessions, IP addresses, browser agents, and duration breakdown.', 'Security', 'PDF', 'Security Officer', 'Ready', '1.1 MB', '2026-08-26 14:24:43.110394', 1, 2);
INSERT INTO public.reports (id, title, description, category, format, created_by, status, file_size, created_at, deleted_flag, category_id) VALUES (2, 'Permission Matrix Audit', 'Historical log of granular capability grants, assignments, and revocations.', 'Security', 'JSON', 'Security Officer', 'Ready', '640 KB', '2026-08-26 14:24:43.110394', 1, 2);
INSERT INTO public.reports (id, title, description, category, format, created_by, status, file_size, created_at, deleted_flag, category_id) VALUES (1, 'User Directory & Role Mapping', 'Complete breakdown of active workspace members and assigned RBAC role tiers.', 'Role Mapping', 'PDF', 'System Administrator', 'Ready', '1.8 MB', '2026-08-26 14:24:43.110394', 1, 3);
INSERT INTO public.reports (id, title, description, category, format, created_by, status, file_size, created_at, deleted_flag, category_id) VALUES (4, 'Access Certification Summary', 'Quarterly access review certification for engineering and management units.', 'Access Audit', 'Excel', 'Compliance Lead', 'Ready', '920 KB', '2026-08-26 14:24:43.110394', 1, 4);


--
-- Data for Name: rolepermissions; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 42, 1);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 42, 2);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 1, 3);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 2, 4);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 3, 5);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 4, 6);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 5, 7);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 6, 8);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 7, 9);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 8, 10);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 9, 11);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 10, 12);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 11, 13);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 17, 14);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 18, 15);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 19, 16);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 20, 17);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 21, 18);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 22, 19);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 40, 20);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 41, 21);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 1, 22);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 3, 23);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 4, 24);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 10, 25);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 11, 26);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 17, 27);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 18, 33);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 19, 34);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 20, 35);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 21, 36);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 22, 37);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 40, 38);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 41, 39);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 1, 43);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 3, 44);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 7, 45);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 17, 46);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 18, 47);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 19, 48);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 20, 49);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 40, 50);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 21, 51);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 43, 52);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 43, 53);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 44, 54);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 45, 55);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 46, 56);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 47, 57);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 43, 58);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 45, 59);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 47, 60);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 48, 61);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (1, 49, 62);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 48, 63);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 50, 64);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 49, 65);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 48, 66);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 49, 67);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 50, 68);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 51, 69);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 52, 70);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 53, 71);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 51, 72);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 52, 73);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 53, 74);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 57, 75);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 58, 76);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 59, 77);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 60, 78);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (2, 61, 79);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 57, 80);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 58, 81);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 59, 82);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 60, 83);
INSERT INTO public.rolepermissions ("RoleId", "PermissionId", "Id") VALUES (3, 61, 84);


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.roles ("Id", "Name", "Description", "DeletedFlag") VALUES (1, 'Employee', 'employee', 1);
INSERT INTO public.roles ("Id", "Name", "Description", "DeletedFlag") VALUES (2, 'super admin', 'access to all', 1);
INSERT INTO public.roles ("Id", "Name", "Description", "DeletedFlag") VALUES (3, 'Manager', 'manager', 1);
INSERT INTO public.roles ("Id", "Name", "Description", "DeletedFlag") VALUES (4, 'test', 'check', 0);


--
-- Data for Name: schedules; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.schedules (id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag) VALUES (1, 'Quarterly RBAC & Role Audit', 'Reviewing elevated role permissions with Managers and Super Admins.', 'Audit', '2026-08-27', '10:00 AM', '11:30 AM', 'Security Conference Room A', 'Vengadesh M', 'Scheduled', 'High', 8, '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.schedules (id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag) VALUES (2, 'New Team Lead Onboarding & Permission Grant', 'Provisioning new workspace managers and reviewing access policies.', 'Training', '2026-08-29', '02:30 PM', '03:30 PM', 'Virtual / Google Meet', 'Kaviya R', 'Scheduled', 'Normal', 4, '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.schedules (id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag) VALUES (3, 'Security Policy Governance Sync', 'Monthly security council meeting to review newly registered users and logs.', 'Governance', '2026-09-02', '09:00 AM', '10:00 AM', 'Executive Boardroom', 'Arun Kumar', 'Scheduled', 'Urgent', 12, '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.schedules (id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag) VALUES (4, 'Sprint Access & Role Review', 'Sprint milestone review of developer role capabilities and API keys.', 'Review', '2026-08-31', '04:00 PM', '05:00 PM', 'Meeting Room B', 'Divya S', 'Scheduled', 'Normal', 6, '2026-08-26 14:24:43.110394', 1);
INSERT INTO public.schedules (id, title, description, event_type, event_date, start_time, end_time, location, organizer, status, priority, attendees_count, created_at, deleted_flag) VALUES (5, 'ISO 27001 Access Certification Walkthrough', 'Annual compliance certification walkthrough with external auditors.', 'Certification', '2026-09-05', '11:00 AM', '01:00 PM', 'Virtual / Zoom', 'Compliance Lead', 'Scheduled', 'High', 15, '2026-08-26 14:24:43.110394', 1);


--
-- Data for Name: setting_categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (1, 'General', 'Workspace profile, identity, and general workspace parameters.', 'Settings', '2026-08-24 14:24:44.056631', 'System Admin', 1);
INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (2, 'Security', 'Authentication policies, JWT tokens, and multi-factor enforcement.', 'ShieldOutlined', '2026-08-24 14:24:44.057054', 'System Admin', 1);
INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (3, 'Notifications', 'Email notification alerts, webhooks, and dispatch rules.', 'NotificationsNoneOutlined', '2026-08-24 14:24:44.057054', 'System Admin', 1);
INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (4, 'RBAC', 'Role-based access control, inheritance, and permission policies.', 'KeyOutlined', '2026-08-24 14:24:44.057055', 'System Admin', 1);
INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (5, 'Sessions', 'Session timeouts, token expiry, and concurrent access rules.', 'AccessTime', '2026-08-24 14:24:44.057055', 'System Admin', 1);
INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (6, 'Email', 'Configure outgoing SMTP mail server and email templates.', 'LanguageOutlined', '2026-08-24 15:28:00.413317', 'System Admin', 1);
INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (7, 'Appearance', 'Workspace theme, branding colors, and interface density.', 'PaletteOutlined', '2026-08-24 15:28:00.413317', 'System Admin', 1);
INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (8, 'Backup', 'Automated database backups, exports, and disaster recovery.', 'StorageOutlined', '2026-08-24 15:28:00.41332', 'System Admin', 1);
INSERT INTO public.setting_categories (id, name, description, icon, created_at, created_by, deleted_flag) VALUES (9, 'Custom Category', 'testing', 'PaletteOutlined', '2026-08-24 15:40:29.355425', 'Vengadesh M', 1);


--
-- Data for Name: system_settings; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (27, 'dark_mode_enabled', 'false', 'General', 'dark_mode_enabled', 'string', '2026-08-28 17:00:46.288062', 'Vengadesh M');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (35, 'two_factor_auth', 'false', 'Security', 'Require 2FA for all admin and elevated accounts.', 'boolean', '2026-08-28 17:08:51.83488', 'test');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (26, 'session_timeout', '24 Hours', 'General', 'session_timeout', 'string', '2026-08-26 14:47:04.098212', 'Vengadesh M');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (28, 'app_name', 'Role Management System', 'General', 'Application name displayed across the system.', 'string', '2026-08-26 14:47:04.093531', 'Vengadesh M');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (29, 'app_url', 'http://localhost:5173', 'General', 'Public application origin URL.', 'string', '2026-08-26 14:47:04.094522', 'Vengadesh M');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (30, 'timezone', '(GMT+05:30) Asia/Kolkata', 'General', 'Default organization timezone.', 'string', '2026-08-26 14:47:04.095454', 'Vengadesh M');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (31, 'date_format', 'DD MMM YYYY', 'General', 'Standard date format for workspace timestamp displays.', 'string', '2026-08-26 14:47:04.096412', 'Vengadesh M');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (32, 'items_per_page', '10', 'General', 'Default table pagination limit per page.', 'number', '2026-08-26 14:47:04.097322', 'Vengadesh M');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (42, 'browser_push_enabled', 'true', 'Notifications', 'Enable in-app desktop push notifications.', 'boolean', '2026-08-26 13:57:45.586487', 'System Admin');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (43, 'auto_backup_enabled', 'true', 'Backup', 'Automated nightly PostgreSQL database snapshot backups.', 'boolean', '2026-08-26 13:57:45.586488', 'System Admin');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (33, 'enable_registration', 'true', 'General', 'Allow new users to register on login portal.', 'boolean', '2026-08-26 13:57:45.586467', 'System Admin');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (34, 'email_verification', 'true', 'General', 'Require email verification for newly registered users.', 'boolean', '2026-08-26 13:57:45.586467', 'System Admin');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (36, 'password_expiry', 'true', 'Security', 'Force password change every 90 days.', 'boolean', '2026-08-26 13:57:45.586468', 'Security Team');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (37, 'login_attempt_limit', 'true', 'Security', 'Lock account after 5 consecutive failed attempts.', 'boolean', '2026-08-26 13:57:45.586468', 'Security Team');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (38, 'maintenance_mode', 'false', 'General', 'Enable maintenance mode for non-admin users.', 'boolean', '2026-08-26 13:57:45.586474', 'System Admin');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (39, 'smtp_host', 'smtp.gmail.com', 'Email', 'Outgoing SMTP mail server hostname.', 'string', '2026-08-26 13:57:45.586474', 'System Admin');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (40, 'smtp_port', '587', 'Email', 'SMTP TLS/SSL connection port.', 'number', '2026-08-26 13:57:45.586475', 'System Admin');
INSERT INTO public.system_settings (id, setting_key, setting_value, category, description, data_type, updated_at, updated_by) VALUES (41, 'email_alerts_enabled', 'true', 'Notifications', 'Send instant Gmail alerts when permissions or roles are modified.', 'boolean', '2026-08-26 13:57:45.586487', 'System Admin');


--
-- Data for Name: user_sessions; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (96, 4, 'manager@gmail.com', 'manager', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-27 17:48:37.361452', '2026-08-27 17:48:46.901111', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Im1hbmFnZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIzIiwiZXhwIjoxNzg3ODQwMzE3LCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.vUUElGnbNBxP2VhBY-knrgVB12vmVUSbFe5raLxPc4s', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (97, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-27 17:48:50.465326', '2026-08-27 17:52:48.390315', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4Nzg0MDMzMCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.F2TJVVoQzFp6kTia7Aou4n50EpEzhQE6n6S0Rf2Z9cs', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (113, 4, 'manager@gmail.com', 'manager', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:53:03.953896', '2026-08-28 11:54:21.121494', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Im1hbmFnZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIzIiwiZXhwIjoxNzg3OTA1MzgzLCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.rPyJaU7hlWI5QXIY5gZaGtLIcPV9h4QtZ1fWRTqy9aw', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (114, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:54:24.863504', '2026-08-28 12:18:31.629969', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkwNTQ2NCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.vAtC-JRvRUDqogelHUqBoPk5msnF387hTaI_IbawdbM', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (99, 3, 'test@gmail.com', 'test', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36 Edg/151.0.0.0', '2026-08-27 17:58:01.789684', '2026-08-27 17:58:14.513862', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIxIiwiZXhwIjoxNzg3ODQwODgxLCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.rQap1dBBOIX1EGDwH_D5Sl9Bb-2EmjwXoXVkBRkyVgc', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (98, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-27 17:52:50.690007', '2026-08-28 10:47:54.822916', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4Nzg0MDU3MCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.24G67ShkqaaKXJxJbFYFKOlNt6Cqm2soHbLKvAr0oXU', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (115, 3, 'test@gmail.com', 'test', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 12:18:36.473947', '2026-08-28 12:19:10.481788', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIxIiwiZXhwIjoxNzg3OTA2OTE2LCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.svVLmNDIOis2CpPcaYJ4NT3s6OmpuMEasw0TjjzMNIM', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (100, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 08:44:26.88277', '2026-08-28 10:47:54.822916', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4Nzg5NDA2NiwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.kNYkhWlFxcLxkor3yY4qRA9xVK31L08IqfZ3Tf04TzA', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (116, 4, 'manager@gmail.com', 'manager', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 12:19:14.858787', '2026-08-28 12:26:52.443258', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Im1hbmFnZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIzIiwiZXhwIjoxNzg3OTA2OTU0LCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.OqiRxvwXOrydkuKlrKipgXjJR0Gg2FM1Yu876hpE6CM', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (101, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 10:47:46.204896', '2026-08-28 10:47:54.822916', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkwMTQ2NiwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.2KIoLXQXHYgiSgvlSH9pzAkgn1QfVrNyp-b549_FXGo', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (117, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 12:26:56.187436', '2026-08-28 16:38:51.202282', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkwNzQxNiwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.Cg2II13dL0gPf9zImfe3G-BcF4CdAgDdseVdO7nfV34', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (102, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', NULL, '2026-08-28 10:42:55.522771', '2026-08-28 10:47:55.522771', '', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (103, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 10:47:58.455355', '2026-08-28 11:10:44.331205', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkwMTQ3OCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.aIeUfKQNqyDieCJd22AUI_h5u1opS8UdG0TPgyPDB8I', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (118, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 12:49:15.964063', '2026-08-28 16:38:51.202282', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkwODc1NSwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.yExQiKD3avfCUmUUeECSFl2nQfzlslxs7YuWjnvgYD0', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (104, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:10:31.810755', '2026-08-28 11:10:44.331205', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkwMjgzMSwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.-sSHQRDSNeX6NAne_oIrUT4mKPfNodKWu77Skc_J_7s', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (105, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', NULL, '2026-08-28 11:05:44.822562', '2026-08-28 11:10:44.822562', '', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (106, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:10:46.880981', '2026-08-28 11:19:01.474554', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkwMjg0NiwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.s4NGGlhOyjBY79xz-sdPAApv7PqnlqPYsyzECE5_BDQ', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (119, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT; Windows NT 10.0; en-US) WindowsPowerShell/5.1.19041.6456', '2026-08-28 14:32:30.740079', '2026-08-28 16:38:51.202282', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkxNDk1MCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.TC1OaEy6M1iz6dIGJLeRe1EjctLFaFkHOY4ff1oW-oo', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (120, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT; Windows NT 10.0; en-US) WindowsPowerShell/5.1.19041.6456', '2026-08-28 14:32:55.260503', '2026-08-28 16:38:51.202282', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkxNDk3NSwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.9CYRNb4o7A3fQZh70Tqlv9pruvDQ7chS_MgRC-cPrlY', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (107, 3, 'test@gmail.com', 'test', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:19:06.784313', '2026-08-28 11:19:33.001726', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIxIiwiZXhwIjoxNzg3OTAzMzQ2LCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.VAyOW6ucJ3tfvOZosvbC9xV54uQaMooGgdAVmTMm8DI', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (121, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 15:27:24.885793', '2026-08-28 16:38:51.202282', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkxODI0NCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.6uWzVRiz2MWrRcq-6jNWuBkJurrpmdDZETmW0uZ2RGA', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (108, 4, 'manager@gmail.com', 'manager', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:19:36.796503', '2026-08-28 11:19:47.454892', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Im1hbmFnZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIzIiwiZXhwIjoxNzg3OTAzMzc2LCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.Pu-5ObzdSYH6YEhU6-u7Ipd6aIQ42AqSYVrRWneNJVU', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (109, 3, 'test@gmail.com', 'test', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:19:51.62337', '2026-08-28 11:46:54.550972', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIxIiwiZXhwIjoxNzg3OTAzMzkxLCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.BmCSG7rx7J4iI9OkLa4QD6qRwizfe9wkWRq2p4K73pY', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (122, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', NULL, '2026-08-28 16:33:51.985008', '2026-08-28 16:38:51.985008', '', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (123, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 16:38:54.37896', '2026-08-28 17:06:05.398445', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkyMjUzNCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.qKD8WOepAnfrAjCPxxkWRPsAlUFYdFeFdfm2vn8Wuzw', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (110, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:46:58.59315', '2026-08-28 11:47:26.457171', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkwNTAxOCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.JflJUtdloWscaYN6cjsWzaG0l33TdeHD3eysTCdPZ74', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (124, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 17:07:04.426054', NULL, 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzkyNDIyNCwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.lpus5atsIQpvNWCgj4pKZqOoHGJ8UfwU3TkHGv81Pr0', true, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (111, 4, 'manager@gmail.com', 'manager', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:47:31.234474', '2026-08-28 11:51:29.879698', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Im1hbmFnZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIzIiwiZXhwIjoxNzg3OTA1MDUxLCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.z2iYxwPc7DXXVveAlnKG1Pc9etUYQpeOGNvTgt58fQY', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (125, 3, 'test@gmail.com', 'test', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36 Edg/151.0.0.0', '2026-08-28 17:08:45.960493', '2026-08-28 17:09:02.742743', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIzIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InRlc3QiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIxIiwiZXhwIjoxNzg3OTI0MzI1LCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.qR5zCkmgHuMNfeyP7b7vwvMe2LO_qoHLxnOwhRlpg_k', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (95, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-27 17:37:41.786403', '2026-08-27 17:48:32.50682', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiIyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlZlbmdhZGVzaCBNIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsImV4cCI6MTc4NzgzOTY2MSwiaXNzIjoiVXNlcnNwYWNlIiwiYXVkIjoiVXNlcnNwYWNlLldlYiJ9.bf1zDqYXI-KFE9plV4mRoUA_rX3oo-jUHU1kPKW2Tko', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (112, 4, 'manager@gmail.com', 'manager', '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36', '2026-08-28 11:51:34.595305', '2026-08-28 11:53:01.663917', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI0IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Im1hbmFnZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIzIiwiZXhwIjoxNzg3OTA1Mjk0LCJpc3MiOiJVc2Vyc3BhY2UiLCJhdWQiOiJVc2Vyc3BhY2UuV2ViIn0.jPqTkSd_BfS01oK2EClRbq6gkgHZnrontzBfrVnfbQg', false, 1);
INSERT INTO public.user_sessions (id, user_id, email, user_name, ip_address, user_agent, login_time, logout_time, session_token, is_active, deleted_flag) VALUES (94, 2, 'vengadesh.kc@gmail.com', 'Vengadesh M', '127.0.0.1', NULL, '2026-08-27 17:32:40.111173', '2026-08-27 17:37:40.111173', '', false, 1);


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.users ("Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DeletedFlag", "DesignationId", "IsFirstLogin") VALUES (3, 'test', 'test@gmail.com', 'AQAAAAIAAYagAAAAEBasb7jCVLpHL7cT7I88L2c0lS1LtoYwLVJoc4ewfHsblYDbjpuNRuTETwEefQ9LUg==', '9876543210', 20, 'coimbatore', 1, 1, 26, false);
INSERT INTO public.users ("Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DeletedFlag", "DesignationId", "IsFirstLogin") VALUES (4, 'manager', 'manager@gmail.com', 'AQAAAAIAAYagAAAAEJ2czBJK5SdJ58LS6Yukp01JVL3DyvXSWlrhYdEiaD9R8ax2xb5Dwya/qvqWcYo4EQ==', '9876543210', 40, 'coimbatore', 3, 1, 27, false);
INSERT INTO public.users ("Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DeletedFlag", "DesignationId", "IsFirstLogin") VALUES (6, 'Ansu kumar', 'ansukumar2007510@gmail.com', 'AQAAAAIAAYagAAAAEGS+DVXhgEeQwEYPIHkkvRcUEBpkVvPWnTgfmggyAQRpN96S1dY+dbDykOUZjeTGVw==', '8124337117', 18, 'erode', 3, 1, 5, false);
INSERT INTO public.users ("Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DeletedFlag", "DesignationId", "IsFirstLogin") VALUES (16, 'ansukumar', 'ansukumar0518@gmail.com', 'AQAAAAIAAYagAAAAEKTWP4hqA7xpb6vzKLtNjYZOzw3F49f5EzEIrpL0rxyqLFoBtYPf61wHdvRwZiPoEQ==', '8124337117', 18, 'erode', 1, 1, 4, false);
INSERT INTO public.users ("Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DeletedFlag", "DesignationId", "IsFirstLogin") VALUES (5, 'ansu kumar', 'ansu@gmail.com', 'AQAAAAIAAYagAAAAEPTh/Lt+iLkVmv2GEcsinWw4GVb6xp+urbSgvjdiK7EoOLL+xYndj+2v2rvUAUQVsA==', '9875412360', 18, 'erode', 3, 1, 5, false);
INSERT INTO public.users ("Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DeletedFlag", "DesignationId", "IsFirstLogin") VALUES (2, 'Vengadesh M', 'vengadesh.kc@gmail.com', 'AQAAAAIAAYagAAAAEFimdH39eKOmsi+uT1dRPINjTPuWppkdwkQz8oS2pmDp4G+8pOyoOU/2OWIFBhJ2lA==', '7010980284', 27, 'kunnathur', 2, 1, 5, false);


--
-- Name: approval_requests_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.approval_requests_id_seq', 17, true);


--
-- Name: audit_logs_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.audit_logs_id_seq', 215, true);


--
-- Name: departmentpermissions_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."departmentpermissions_Id_seq"', 83, true);


--
-- Name: departments_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."departments_Id_seq"', 8, true);


--
-- Name: designations_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."designations_Id_seq"', 27, true);


--
-- Name: event_types_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.event_types_id_seq', 6, true);


--
-- Name: invoice_items_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.invoice_items_id_seq', 7, true);


--
-- Name: invoices_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.invoices_id_seq', 4, true);


--
-- Name: menus_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.menus_id_seq', 34, true);


--
-- Name: permissions_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."permissions_Id_seq"', 66, true);


--
-- Name: project_categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.project_categories_id_seq', 6, true);


--
-- Name: projects_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.projects_id_seq', 7, true);


--
-- Name: purchases_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.purchases_id_seq', 5, true);


--
-- Name: report_categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.report_categories_id_seq', 8, true);


--
-- Name: reports_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.reports_id_seq', 6, true);


--
-- Name: rolepermissions_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."rolepermissions_Id_seq"', 84, true);


--
-- Name: roles_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."roles_Id_seq"', 4, true);


--
-- Name: schedules_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.schedules_id_seq', 5, true);


--
-- Name: setting_categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.setting_categories_id_seq', 9, true);


--
-- Name: system_settings_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.system_settings_id_seq', 43, true);


--
-- Name: user_sessions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.user_sessions_id_seq', 125, true);


--
-- Name: users_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."users_Id_seq"', 18, true);


--
-- Name: rolepermissions PK_RolePermissions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rolepermissions
    ADD CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("RoleId", "PermissionId");


--
-- Name: permissions UX_Permissions_Key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.permissions
    ADD CONSTRAINT "UX_Permissions_Key" UNIQUE ("PermissionKey");


--
-- Name: roles UX_Roles_Name_Active; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT "UX_Roles_Name_Active" UNIQUE ("Name", "DeletedFlag");


--
-- Name: users UX_Users_Email; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT "UX_Users_Email" UNIQUE ("Email");


--
-- Name: approval_requests approval_requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.approval_requests
    ADD CONSTRAINT approval_requests_pkey PRIMARY KEY (id);


--
-- Name: audit_logs audit_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (id);


--
-- Name: departmentpermissions departmentpermissions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departmentpermissions
    ADD CONSTRAINT departmentpermissions_pkey PRIMARY KEY ("Id");


--
-- Name: departments departments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_pkey PRIMARY KEY ("Id");


--
-- Name: designations designations_Name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.designations
    ADD CONSTRAINT "designations_Name_key" UNIQUE ("Name");


--
-- Name: designations designations_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.designations
    ADD CONSTRAINT designations_pkey PRIMARY KEY ("Id");


--
-- Name: event_types event_types_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.event_types
    ADD CONSTRAINT event_types_name_key UNIQUE (name);


--
-- Name: event_types event_types_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.event_types
    ADD CONSTRAINT event_types_pkey PRIMARY KEY (id);


--
-- Name: invoice_items invoice_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoice_items
    ADD CONSTRAINT invoice_items_pkey PRIMARY KEY (id);


--
-- Name: invoices invoices_invoice_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoices
    ADD CONSTRAINT invoices_invoice_number_key UNIQUE (invoice_number);


--
-- Name: invoices invoices_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoices
    ADD CONSTRAINT invoices_pkey PRIMARY KEY (id);


--
-- Name: menus menus_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.menus
    ADD CONSTRAINT menus_pkey PRIMARY KEY (id);


--
-- Name: permissions permissions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.permissions
    ADD CONSTRAINT permissions_pkey PRIMARY KEY ("Id");


--
-- Name: project_categories project_categories_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project_categories
    ADD CONSTRAINT project_categories_name_key UNIQUE (name);


--
-- Name: project_categories project_categories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.project_categories
    ADD CONSTRAINT project_categories_pkey PRIMARY KEY (id);


--
-- Name: projects projects_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.projects
    ADD CONSTRAINT projects_pkey PRIMARY KEY (id);


--
-- Name: purchases purchases_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.purchases
    ADD CONSTRAINT purchases_pkey PRIMARY KEY (id);


--
-- Name: report_categories report_categories_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.report_categories
    ADD CONSTRAINT report_categories_name_key UNIQUE (name);


--
-- Name: report_categories report_categories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.report_categories
    ADD CONSTRAINT report_categories_pkey PRIMARY KEY (id);


--
-- Name: reports reports_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reports
    ADD CONSTRAINT reports_pkey PRIMARY KEY (id);


--
-- Name: roles roles_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY ("Id");


--
-- Name: schedules schedules_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.schedules
    ADD CONSTRAINT schedules_pkey PRIMARY KEY (id);


--
-- Name: setting_categories setting_categories_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.setting_categories
    ADD CONSTRAINT setting_categories_name_key UNIQUE (name);


--
-- Name: setting_categories setting_categories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.setting_categories
    ADD CONSTRAINT setting_categories_pkey PRIMARY KEY (id);


--
-- Name: system_settings system_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_settings
    ADD CONSTRAINT system_settings_pkey PRIMARY KEY (id);


--
-- Name: system_settings system_settings_setting_key_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.system_settings
    ADD CONSTRAINT system_settings_setting_key_key UNIQUE (setting_key);


--
-- Name: user_sessions user_sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_sessions
    ADD CONSTRAINT user_sessions_pkey PRIMARY KEY (id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY ("Id");


--
-- Name: menus ux_menus_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.menus
    ADD CONSTRAINT ux_menus_key UNIQUE (menukey);


--
-- Name: idx_approval_requests_deleted_flag; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_approval_requests_deleted_flag ON public.approval_requests USING btree (deleted_flag);


--
-- Name: idx_approval_requests_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_approval_requests_status ON public.approval_requests USING btree (status);


--
-- Name: idx_approval_requests_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_approval_requests_user_id ON public.approval_requests USING btree (user_id);


--
-- Name: idx_purchases_approval_request_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchases_approval_request_id ON public.purchases USING btree (approval_request_id);


--
-- Name: idx_purchases_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_purchases_status ON public.purchases USING btree (status);


--
-- Name: rolepermissions FK_RolePermissions_Permission; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rolepermissions
    ADD CONSTRAINT "FK_RolePermissions_Permission" FOREIGN KEY ("PermissionId") REFERENCES public.permissions("Id");


--
-- Name: rolepermissions FK_RolePermissions_Role; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rolepermissions
    ADD CONSTRAINT "FK_RolePermissions_Role" FOREIGN KEY ("RoleId") REFERENCES public.roles("Id");


--
-- Name: users FK_Users_Roles; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT "FK_Users_Roles" FOREIGN KEY ("RoleId") REFERENCES public.roles("Id");


--
-- Name: invoice_items invoice_items_invoice_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.invoice_items
    ADD CONSTRAINT invoice_items_invoice_id_fkey FOREIGN KEY (invoice_id) REFERENCES public.invoices(id) ON DELETE CASCADE;


--
-- Name: reports reports_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.reports
    ADD CONSTRAINT reports_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.report_categories(id);


--
-- Name: users users_DesignationId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT "users_DesignationId_fkey" FOREIGN KEY ("DesignationId") REFERENCES public.designations("Id");


--
-- PostgreSQL database dump complete
--


