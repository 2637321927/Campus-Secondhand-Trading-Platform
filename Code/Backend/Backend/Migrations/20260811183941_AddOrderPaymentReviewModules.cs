using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentReviewModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "is_hidden",
                table: "review",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "reply_info",
                table: "review",
                type: "NVARCHAR2(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reply_time",
                table: "review",
                type: "TIMESTAMP(7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "receiving_address",
                table: "purchase",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shipping_address",
                table: "purchase",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shipping_method",
                table: "purchase",
                type: "NVARCHAR2(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tracking_number",
                table: "purchase",
                type: "NVARCHAR2(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "order_timeline",
                columns: table => new
                {
                    timeline_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    old_status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    new_status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    change_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    operator_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    note = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    purchase_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_timeline", x => x.timeline_id);
                    table.ForeignKey(
                        name: "FK_order_timeline_purchase_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchase",
                        principalColumn: "purchase_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    payment_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    status = table.Column<int>(type: "NUMBER(10)", maxLength: 20, nullable: false),
                    payment_method = table.Column<int>(type: "NUMBER(10)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    transaction_id = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    pay_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    cancel_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    purchase_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_payment_purchase_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchase",
                        principalColumn: "purchase_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_timeline_purchase_id",
                table: "order_timeline",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_purchase_id",
                table: "payment",
                column: "purchase_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_timeline");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropColumn(
                name: "is_hidden",
                table: "review");

            migrationBuilder.DropColumn(
                name: "reply_info",
                table: "review");

            migrationBuilder.DropColumn(
                name: "reply_time",
                table: "review");

            migrationBuilder.DropColumn(
                name: "receiving_address",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "shipping_address",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "shipping_method",
                table: "purchase");

            migrationBuilder.DropColumn(
                name: "tracking_number",
                table: "purchase");
        }
    }
}
