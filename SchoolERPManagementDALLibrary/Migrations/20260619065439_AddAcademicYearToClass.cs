using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearToClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "academicyearid",
                table: "classes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_classes_academicyearid",
                table: "classes",
                column: "academicyearid");

            migrationBuilder.AddForeignKey(
                name: "classes_academicyearid_fkey",
                table: "classes",
                column: "academicyearid",
                principalTable: "academicyears",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "classes_academicyearid_fkey",
                table: "classes");

            migrationBuilder.DropIndex(
                name: "IX_classes_academicyearid",
                table: "classes");

            migrationBuilder.DropColumn(
                name: "academicyearid",
                table: "classes");
        }
    }
}
