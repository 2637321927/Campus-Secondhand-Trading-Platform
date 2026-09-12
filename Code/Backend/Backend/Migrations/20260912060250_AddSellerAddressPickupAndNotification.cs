using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Generated
{
    /// <inheritdoc />
    public partial class AddSellerAddressPickupAndNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "address_id",
                table: "purchase",
                type: "NUMBER(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AddColumn<int>(
                name: "is_pickup",
                table: "purchase",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "seller_address",
                table: "product",
                type: "NVARCHAR2(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    notification_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    type = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    content = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    related_id = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    is_read = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_notification_base_user_user_id",
                        column: x => x.user_id,
                        principalTable: "base_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_user_id",
                table: "notification",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropColumn(
                name: "is_pickup",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "seller_address",
                table: "product");

            migrationBuilder.AlterColumn<int>(
                name: "address_id",
                table: "purchase",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldNullable: true);
        }
    }
}
