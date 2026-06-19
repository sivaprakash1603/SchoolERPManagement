using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToManyToManyParents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "students_parentid_fkey",
                table: "students");

            migrationBuilder.DropIndex(
                name: "IX_students_parentid",
                table: "students");

            migrationBuilder.DropColumn(
                name: "parentid",
                table: "students");

            migrationBuilder.DropColumn(
                name: "relation",
                table: "parents");

            migrationBuilder.CreateTable(
                name: "studentparents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    parentid = table.Column<int>(type: "integer", nullable: false),
                    relation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    isprimarycontact = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("studentparents_pkey", x => x.id);
                    table.ForeignKey(
                        name: "studentparents_parentid_fkey",
                        column: x => x.parentid,
                        principalTable: "parents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "studentparents_studentid_fkey",
                        column: x => x.studentid,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_studentparents_parentid",
                table: "studentparents",
                column: "parentid");

            migrationBuilder.CreateIndex(
                name: "IX_studentparents_studentid",
                table: "studentparents",
                column: "studentid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "studentparents");

            migrationBuilder.AddColumn<int>(
                name: "parentid",
                table: "students",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "relation",
                table: "parents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_parentid",
                table: "students",
                column: "parentid");

            migrationBuilder.AddForeignKey(
                name: "students_parentid_fkey",
                table: "students",
                column: "parentid",
                principalTable: "parents",
                principalColumn: "id");
        }
    }
}
