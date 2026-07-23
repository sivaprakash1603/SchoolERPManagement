using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddReadOnlyAIUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'readonly_ai_user') THEN

      CREATE ROLE readonly_ai_user LOGIN PASSWORD 'SchoolERP_AI_ReadOnly_2026$!';
   END IF;
END
$do$;

GRANT USAGE ON SCHEMA public TO readonly_ai_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly_ai_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO readonly_ai_user;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP OWNED BY readonly_ai_user;
DROP ROLE IF EXISTS readonly_ai_user;
");
        }
    }
}
