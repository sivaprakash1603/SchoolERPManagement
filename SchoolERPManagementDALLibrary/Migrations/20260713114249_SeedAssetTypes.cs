using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class SeedAssetTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "assettypes",
                columns: new[] { "id", "typename" },
                values: new object[,]
                {
                    { 1, "Electronics" },
                    { 2, "Furniture" },
                    { 3, "IT Equipment" },
                    { 4, "Lab Equipment" },
                    { 5, "Sports Equipment" },
                    { 6, "Vehicles" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "assettypes",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "assettypes",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "assettypes",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "assettypes",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "assettypes",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "assettypes",
                keyColumn: "id",
                keyValue: 6);
        }
    }
}
