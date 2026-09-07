using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserStatusAndWarnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "account_status",
                table: "base_user",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "user_warning",
                columns: table => new
                {
                    warning_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    admin_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    reason = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_warning", x => x.warning_id);
                    table.ForeignKey(
                        name: "FK_user_warning_admin_user_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admin_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_warning_norm_user_user_id",
                        column: x => x.user_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_warning_admin_id",
                table: "user_warning",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_warning_user_id",
                table: "user_warning",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_warning");

            migrationBuilder.DropColumn(
                name: "account_status",
                table: "base_user");
        }
    }
}
