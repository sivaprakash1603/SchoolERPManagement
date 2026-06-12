using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    
    public partial class AddResetTokenToUser : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Resettoken",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Resettokenexpiry",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Resettoken",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Resettokenexpiry",
                table: "users");
        }
    }
}
