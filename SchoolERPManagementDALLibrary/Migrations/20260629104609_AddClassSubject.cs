using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddClassSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classsubjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    classid = table.Column<int>(type: "integer", nullable: false),
                    subjectid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("classsubjects_pkey", x => x.id);
                    table.ForeignKey(
                        name: "classsubjects_classid_fkey",
                        column: x => x.classid,
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "classsubjects_subjectid_fkey",
                        column: x => x.subjectid,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_classsubjects_classid",
                table: "classsubjects",
                column: "classid");

            migrationBuilder.CreateIndex(
                name: "IX_classsubjects_subjectid",
                table: "classsubjects",
                column: "subjectid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "classsubjects");
        }
    }
}
