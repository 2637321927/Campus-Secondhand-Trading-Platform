using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class createupdatedfileschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "img_url",
                table: "rev_image");

            migrationBuilder.DropColumn(
                name: "img_url",
                table: "prod_image");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "base_user");

            migrationBuilder.RenameColumn(
                name: "img_id",
                table: "rev_image",
                newName: "img_file_id");

            migrationBuilder.RenameColumn(
                name: "img_id",
                table: "prod_image",
                newName: "img_file_id");

            migrationBuilder.AlterColumn<long>(
                name: "img_file_id",
                table: "rev_image",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AlterColumn<long>(
                name: "img_file_id",
                table: "prod_image",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)")
                .OldAnnotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<long>(
                name: "file_id",
                table: "message",
                type: "NUMBER(19)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "avatar_file_id",
                table: "base_user",
                type: "NUMBER(19)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    file_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    file_name = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    storage_path = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    mime_type = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    file_size = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    content_type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    upload_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    uploader_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    is_deleted = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    deleted_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.file_id);
                    table.ForeignKey(
                        name: "FK_files_base_user_uploader_id",
                        column: x => x.uploader_id,
                        principalTable: "base_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_message_file_id",
                table: "message",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_base_user_avatar_file_id",
                table: "base_user",
                column: "avatar_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_files_uploader_id",
                table: "files",
                column: "uploader_id");

            migrationBuilder.AddForeignKey(
                name: "FK_base_user_files_avatar_file_id",
                table: "base_user",
                column: "avatar_file_id",
                principalTable: "files",
                principalColumn: "file_id");

            migrationBuilder.AddForeignKey(
                name: "FK_message_files_file_id",
                table: "message",
                column: "file_id",
                principalTable: "files",
                principalColumn: "file_id");

            migrationBuilder.AddForeignKey(
                name: "FK_prod_image_files_img_file_id",
                table: "prod_image",
                column: "img_file_id",
                principalTable: "files",
                principalColumn: "file_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rev_image_files_img_file_id",
                table: "rev_image",
                column: "img_file_id",
                principalTable: "files",
                principalColumn: "file_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_base_user_files_avatar_file_id",
                table: "base_user");

            migrationBuilder.DropForeignKey(
                name: "FK_message_files_file_id",
                table: "message");

            migrationBuilder.DropForeignKey(
                name: "FK_prod_image_files_img_file_id",
                table: "prod_image");

            migrationBuilder.DropForeignKey(
                name: "FK_rev_image_files_img_file_id",
                table: "rev_image");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropIndex(
                name: "IX_message_file_id",
                table: "message");

            migrationBuilder.DropIndex(
                name: "IX_base_user_avatar_file_id",
                table: "base_user");

            migrationBuilder.DropColumn(
                name: "file_id",
                table: "message");

            migrationBuilder.DropColumn(
                name: "avatar_file_id",
                table: "base_user");

            migrationBuilder.RenameColumn(
                name: "img_file_id",
                table: "rev_image",
                newName: "img_id");

            migrationBuilder.RenameColumn(
                name: "img_file_id",
                table: "prod_image",
                newName: "img_id");

            migrationBuilder.AlterColumn<long>(
                name: "img_id",
                table: "rev_image",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<string>(
                name: "img_url",
                table: "rev_image",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "img_id",
                table: "prod_image",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)")
                .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1");

            migrationBuilder.AddColumn<string>(
                name: "img_url",
                table: "prod_image",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "base_user",
                type: "NVARCHAR2(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
