using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchTermandEdgeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_term",
                columns: table => new
                {
                    term_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    term_text = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    row_sum = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_term", x => x.term_id);
                });

            migrationBuilder.CreateTable(
                name: "search_term_edge",
                columns: table => new
                {
                    edge_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    term1_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    term2_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    weight = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_term_edge", x => x.edge_id);
                    table.ForeignKey(
                        name: "FK_search_term_edge_search_term_term1_id",
                        column: x => x.term1_id,
                        principalTable: "search_term",
                        principalColumn: "term_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_search_term_edge_search_term_term2_id",
                        column: x => x.term2_id,
                        principalTable: "search_term",
                        principalColumn: "term_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_search_term_term_text",
                table: "search_term",
                column: "term_text",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_term_edge_term1_id_term2_id",
                table: "search_term_edge",
                columns: new[] { "term1_id", "term2_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_term_edge_term2_id",
                table: "search_term_edge",
                column: "term2_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_term_edge");

            migrationBuilder.DropTable(
                name: "search_term");
        }
    }
}
