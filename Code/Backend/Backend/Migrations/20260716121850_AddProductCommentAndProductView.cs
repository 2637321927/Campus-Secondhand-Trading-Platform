using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCommentAndProductView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_comment",
                columns: table => new
                {
                    comment_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    content = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    index = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ResponseToId = table.Column<long>(type: "NUMBER(19)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_comment", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_product_comment_norm_user_user_id",
                        column: x => x.user_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_comment_product_comment_ResponseToId",
                        column: x => x.ResponseToId,
                        principalTable: "product_comment",
                        principalColumn: "comment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_comment_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_view",
                columns: table => new
                {
                    view_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    view_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_view", x => x.view_id);
                    table.ForeignKey(
                        name: "FK_product_view_norm_user_user_id",
                        column: x => x.user_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_view_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_comment_product_id_index",
                table: "product_comment",
                columns: new[] { "product_id", "index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_comment_ResponseToId",
                table: "product_comment",
                column: "ResponseToId");

            migrationBuilder.CreateIndex(
                name: "IX_product_comment_user_id",
                table: "product_comment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_view_product_id",
                table: "product_view",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_view_user_id",
                table: "product_view",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_comment");

            migrationBuilder.DropTable(
                name: "product_view");
        }
    }
}
