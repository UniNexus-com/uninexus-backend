using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultMemberRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed missing system roles safely
            migrationBuilder.Sql(@"
INSERT INTO club_roles (name, is_system_role, club_id, ""Created"", ""CreatedBy"")
SELECT 'Vice President', true, null, NOW(), 'migration'
WHERE NOT EXISTS (SELECT 1 FROM club_roles WHERE name = 'Vice President' AND is_system_role = true AND club_id IS NULL);

INSERT INTO club_roles (name, is_system_role, club_id, ""Created"", ""CreatedBy"")
SELECT 'Treasurer', true, null, NOW(), 'migration'
WHERE NOT EXISTS (SELECT 1 FROM club_roles WHERE name = 'Treasurer' AND is_system_role = true AND club_id IS NULL);

INSERT INTO club_roles (name, is_system_role, club_id, ""Created"", ""CreatedBy"")
SELECT 'Secretary', true, null, NOW(), 'migration'
WHERE NOT EXISTS (SELECT 1 FROM club_roles WHERE name = 'Secretary' AND is_system_role = true AND club_id IS NULL);

INSERT INTO club_roles (name, is_system_role, club_id, ""Created"", ""CreatedBy"")
SELECT 'Active Member', true, null, NOW(), 'migration'
WHERE NOT EXISTS (SELECT 1 FROM club_roles WHERE name = 'Active Member' AND is_system_role = true AND club_id IS NULL);

CREATE OR REPLACE FUNCTION assign_default_club_role()
RETURNS TRIGGER AS $$
BEGIN
    -- If no role ID is explicitly provided, assign the Active Member system role
    IF NEW.club_role_id IS NULL OR NEW.club_role_id = 0 THEN
        NEW.club_role_id := (SELECT id FROM club_roles WHERE name = 'Active Member' AND is_system_role = true AND club_id IS NULL LIMIT 1);
        
        IF NEW.club_role_id IS NULL THEN
            RAISE EXCEPTION 'Could not determine default role id for Active Member.';
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER default_club_role_trigger
BEFORE INSERT ON user_clubs
FOR EACH ROW
EXECUTE FUNCTION assign_default_club_role();
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS default_club_role_trigger ON user_clubs;
DROP FUNCTION IF EXISTS assign_default_club_role();
            ");
        }
    }
}
