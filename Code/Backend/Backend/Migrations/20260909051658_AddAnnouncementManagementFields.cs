using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Generated
{
    /// <inheritdoc />
    public partial class AddAnnouncementManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "info",
                table: "announcement",
                type: "NVARCHAR2(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<bool>(
                name: "is_pinned",
                table: "announcement",
                type: "BOOLEAN",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "announcement",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "published");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_pinned",
                table: "announcement");

            migrationBuilder.DropColumn(
                name: "status",
                table: "announcement");

            migrationBuilder.AlterColumn<string>(
                name: "info",
                table: "announcement",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldMaxLength: 2000);
        }
    }
}
