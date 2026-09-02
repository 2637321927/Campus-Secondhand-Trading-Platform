using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class report_appeal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "appeal_against_id",
                table: "work_order",
                type: "NUMBER(19)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "handle_action",
                table: "work_order",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "result",
                table: "work_order",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "target_id",
                table: "work_order",
                type: "NUMBER(19)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target_type",
                table: "work_order",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "work_order_timeline",
                columns: table => new
                {
                    timeline_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    work_order_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    action = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    note = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    admin_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_timeline", x => x.timeline_id);
                    table.ForeignKey(
                        name: "FK_work_order_timeline_admin_user_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admin_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_order_timeline_work_order_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "work_order",
                        principalColumn: "work_order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_appeal_against_id",
                table: "work_order",
                column: "appeal_against_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_timeline_admin_id",
                table: "work_order_timeline",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_timeline_work_order_id",
                table: "work_order_timeline",
                column: "work_order_id");

            migrationBuilder.AddForeignKey(
                name: "FK_work_order_work_order_appeal_against_id",
                table: "work_order",
                column: "appeal_against_id",
                principalTable: "work_order",
                principalColumn: "work_order_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_work_order_work_order_appeal_against_id",
                table: "work_order");

            migrationBuilder.DropTable(
                name: "work_order_timeline");

            migrationBuilder.DropIndex(
                name: "IX_work_order_appeal_against_id",
                table: "work_order");

            migrationBuilder.DropColumn(
                name: "appeal_against_id",
                table: "work_order");

            migrationBuilder.DropColumn(
                name: "handle_action",
                table: "work_order");

            migrationBuilder.DropColumn(
                name: "result",
                table: "work_order");

            migrationBuilder.DropColumn(
                name: "target_id",
                table: "work_order");

            migrationBuilder.DropColumn(
                name: "target_type",
                table: "work_order");
        }
    }
}
