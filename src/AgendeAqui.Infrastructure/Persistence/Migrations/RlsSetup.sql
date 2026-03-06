-- Enable RLS on tenant tables
ALTER TABLE appointments ENABLE ROW LEVEL SECURITY;
ALTER TABLE schedules ENABLE ROW LEVEL SECURITY;
ALTER TABLE professionals ENABLE ROW LEVEL SECURITY;
ALTER TABLE services ENABLE ROW LEVEL SECURITY;
ALTER TABLE clients ENABLE ROW LEVEL SECURITY;

-- Create RLS policies for tenant isolation
CREATE POLICY tenant_isolation_appointments ON appointments
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

CREATE POLICY tenant_isolation_schedules ON schedules
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

CREATE POLICY tenant_isolation_professionals ON professionals
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

CREATE POLICY tenant_isolation_services ON services
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

CREATE POLICY tenant_isolation_clients ON clients
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

-- Grant permissions to the application role (adjust role name as needed)
-- ALTER TABLE appointments FORCE ROW LEVEL SECURITY;
-- ALTER TABLE schedules FORCE ROW LEVEL SECURITY;
-- ALTER TABLE professionals FORCE ROW LEVEL SECURITY;
-- ALTER TABLE services FORCE ROW LEVEL SECURITY;
-- ALTER TABLE clients FORCE ROW LEVEL SECURITY;
