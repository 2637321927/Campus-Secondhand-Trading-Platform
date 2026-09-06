using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Generated
{
    /// <inheritdoc />
    public partial class SearchTermSimilarityGenerated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_term_similarity",
                columns: table => new
                {
                    similarity_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    source_term_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    similar_term_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    similarity = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    rank = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_term_similarity", x => x.similarity_id);
                    table.ForeignKey(
                        name: "FK_search_term_similarity_search_term_similar_term_id",
                        column: x => x.similar_term_id,
                        principalTable: "search_term",
                        principalColumn: "term_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_search_term_similarity_search_term_source_term_id",
                        column: x => x.source_term_id,
                        principalTable: "search_term",
                        principalColumn: "term_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_search_term_similarity_similar_term_id",
                table: "search_term_similarity",
                column: "similar_term_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_term_similarity_source_term_id_rank",
                table: "search_term_similarity",
                columns: new[] { "source_term_id", "rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_term_similarity_source_term_id_similar_term_id",
                table: "search_term_similarity",
                columns: new[] { "source_term_id", "similar_term_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_term_similarity");
        }
    }
}
