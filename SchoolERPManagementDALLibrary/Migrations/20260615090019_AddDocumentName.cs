using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "feepayments_feestructureid_fkey",
                table: "feepayments");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "assets");

            migrationBuilder.RenameColumn(
                name: "feestructureid",
                table: "feepayments",
                newName: "Feestructureid");

            migrationBuilder.RenameIndex(
                name: "IX_feepayments_feestructureid",
                table: "feepayments",
                newName: "IX_feepayments_Feestructureid");

            migrationBuilder.AddColumn<string>(
                name: "documentname",
                table: "teacherdocuments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "documentname",
                table: "studentdocuments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "documentname",
                table: "parentdocuments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "paymentdate",
                table: "feepayments",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "CURRENT_DATE",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddForeignKey(
                name: "FK_feepayments_feestructures_Feestructureid",
                table: "feepayments",
                column: "Feestructureid",
                principalTable: "feestructures",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_feepayments_feestructures_Feestructureid",
                table: "feepayments");

            migrationBuilder.DropColumn(
                name: "documentname",
                table: "teacherdocuments");

            migrationBuilder.DropColumn(
                name: "documentname",
                table: "studentdocuments");

            migrationBuilder.DropColumn(
                name: "documentname",
                table: "parentdocuments");

            migrationBuilder.RenameColumn(
                name: "Feestructureid",
                table: "feepayments",
                newName: "feestructureid");

            migrationBuilder.RenameIndex(
                name: "IX_feepayments_Feestructureid",
                table: "feepayments",
                newName: "IX_feepayments_feestructureid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "paymentdate",
                table: "feepayments",
                type: "timestamp without time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_DATE");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "assets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "feepayments_feestructureid_fkey",
                table: "feepayments",
                column: "feestructureid",
                principalTable: "feestructures",
                principalColumn: "id");
        }
    }
}
