using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAuditModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reject_reason",
                table: "product",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reviewed_at",
                table: "product",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reviewed_by",
                table: "product",
                type: "NUMBER(10)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "product_audit_log",
                columns: table => new
                {
                    audit_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    admin_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    action = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    old_status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    new_status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_audit_log", x => x.audit_id);
                    table.ForeignKey(
                        name: "FK_product_audit_log_admin_user_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admin_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_audit_log_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_reviewed_by",
                table: "product",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_product_audit_log_admin_id",
                table: "product_audit_log",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_audit_log_product_id",
                table: "product_audit_log",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_admin_user_reviewed_by",
                table: "product",
                column: "reviewed_by",
                principalTable: "admin_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_admin_user_reviewed_by",
                table: "product");

            migrationBuilder.DropTable(
                name: "product_audit_log");

            migrationBuilder.DropIndex(
                name: "IX_product_reviewed_by",
                table: "product");

            migrationBuilder.DropColumn(
                name: "reject_reason",
                table: "product");

            migrationBuilder.DropColumn(
                name: "reviewed_at",
                table: "product");

            migrationBuilder.DropColumn(
                name: "reviewed_by",
                table: "product");
        }
    }
}
