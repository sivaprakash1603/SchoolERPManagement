using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class SplitNameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "name",
                table: "students");

            migrationBuilder.DropColumn(
                name: "name",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "name",
                table: "parents");

            migrationBuilder.AddColumn<string>(
                name: "firstname",
                table: "teachers",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "lastname",
                table: "teachers",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "firstname",
                table: "students",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "lastname",
                table: "students",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "firstname",
                table: "staff",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "lastname",
                table: "staff",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "firstname",
                table: "parents",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "lastname",
                table: "parents",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "firstname",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "lastname",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "firstname",
                table: "students");

            migrationBuilder.DropColumn(
                name: "lastname",
                table: "students");

            migrationBuilder.DropColumn(
                name: "firstname",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "lastname",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "firstname",
                table: "parents");

            migrationBuilder.DropColumn(
                name: "lastname",
                table: "parents");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "teachers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "students",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "staff",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "parents",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
