using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeStructureIdToFeePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Resettokenexpiry",
                table: "users",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "feestructureid",
                table: "feepayments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_feepayments_feestructureid",
                table: "feepayments",
                column: "feestructureid");

            migrationBuilder.AddForeignKey(
                name: "feepayments_feestructureid_fkey",
                table: "feepayments",
                column: "feestructureid",
                principalTable: "feestructures",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "feepayments_feestructureid_fkey",
                table: "feepayments");

            migrationBuilder.DropIndex(
                name: "IX_feepayments_feestructureid",
                table: "feepayments");

            migrationBuilder.DropColumn(
                name: "feestructureid",
                table: "feepayments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Resettokenexpiry",
                table: "users",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
