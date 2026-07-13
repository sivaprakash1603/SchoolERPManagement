using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddParentTeacherMeeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parentteachermeetings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    academiccalendarid = table.Column<int>(type: "integer", nullable: true),
                    academicyearid = table.Column<int>(type: "integer", nullable: false),
                    eventdate = table.Column<DateOnly>(type: "date", nullable: false),
                    starttime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    endtime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    slotdurationminutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("parentteachermeetings_pkey", x => x.id);
                    table.ForeignKey(
                        name: "parentteachermeetings_academiccalendarid_fkey",
                        column: x => x.academiccalendarid,
                        principalTable: "academiccalendar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "parentteachermeetings_academicyearid_fkey",
                        column: x => x.academicyearid,
                        principalTable: "academicyears",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parentteacherslots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    meetingid = table.Column<int>(type: "integer", nullable: false),
                    teacherid = table.Column<int>(type: "integer", nullable: false),
                    starttime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    endtime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    parentid = table.Column<int>(type: "integer", nullable: true),
                    studentid = table.Column<int>(type: "integer", nullable: true),
                    bookedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("parentteacherslots_pkey", x => x.id);
                    table.ForeignKey(
                        name: "parentteacherslots_meetingid_fkey",
                        column: x => x.meetingid,
                        principalTable: "parentteachermeetings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "parentteacherslots_parentid_fkey",
                        column: x => x.parentid,
                        principalTable: "parents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "parentteacherslots_studentid_fkey",
                        column: x => x.studentid,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "parentteacherslots_teacherid_fkey",
                        column: x => x.teacherid,
                        principalTable: "teachers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_parentteachermeetings_academiccalendarid",
                table: "parentteachermeetings",
                column: "academiccalendarid");

            migrationBuilder.CreateIndex(
                name: "IX_parentteachermeetings_academicyearid",
                table: "parentteachermeetings",
                column: "academicyearid");

            migrationBuilder.CreateIndex(
                name: "IX_parentteacherslots_parentid",
                table: "parentteacherslots",
                column: "parentid");

            migrationBuilder.CreateIndex(
                name: "IX_parentteacherslots_studentid",
                table: "parentteacherslots",
                column: "studentid");

            migrationBuilder.CreateIndex(
                name: "IX_parentteacherslots_teacherid",
                table: "parentteacherslots",
                column: "teacherid");

            migrationBuilder.CreateIndex(
                name: "parentteacherslots_meetingid_teacherid_starttime_key",
                table: "parentteacherslots",
                columns: new[] { "meetingid", "teacherid", "starttime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parentteacherslots");

            migrationBuilder.DropTable(
                name: "parentteachermeetings");
        }
    }
}
