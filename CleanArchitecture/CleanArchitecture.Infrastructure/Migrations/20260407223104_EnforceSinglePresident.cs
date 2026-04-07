using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSinglePresident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION check_single_president()
RETURNS TRIGGER AS $$
DECLARE
    role_name VARCHAR;
    president_count INTEGER;
BEGIN
    SELECT name INTO role_name FROM club_roles WHERE id = NEW.club_role_id;
    
    IF role_name = 'President' THEN
        SELECT count(*) INTO president_count 
        FROM user_clubs uc
        JOIN club_roles cr ON uc.club_role_id = cr.id
        WHERE uc.club_id = NEW.club_id AND cr.name = 'President' AND uc.user_id != NEW.user_id;

        IF president_count > 0 THEN
            RAISE EXCEPTION 'A club can only have one president.';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER enforce_single_president
BEFORE INSERT OR UPDATE ON user_clubs
FOR EACH ROW
EXECUTE FUNCTION check_single_president();
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS enforce_single_president ON user_clubs;
DROP FUNCTION IF EXISTS check_single_president();
            ");
        }
    }
}
