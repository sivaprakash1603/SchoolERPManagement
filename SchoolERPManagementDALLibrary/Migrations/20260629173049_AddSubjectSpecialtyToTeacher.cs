using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectSpecialtyToTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "subjectspecialtyid",
                table: "teachers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_teachers_subjectspecialtyid",
                table: "teachers",
                column: "subjectspecialtyid");

            migrationBuilder.AddForeignKey(
                name: "teachers_subjectspecialtyid_fkey",
                table: "teachers",
                column: "subjectspecialtyid",
                principalTable: "subjects",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "teachers_subjectspecialtyid_fkey",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "IX_teachers_subjectspecialtyid",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "subjectspecialtyid",
                table: "teachers");
        }
    }
}
