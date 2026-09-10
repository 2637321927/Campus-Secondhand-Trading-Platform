using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Generated
{
    /// <inheritdoc />
    public partial class AlignAnnouncementContentPublishTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE
                    v_info_cnt NUMBER;
                    v_content_cnt NUMBER;
                    v_publish_cnt NUMBER;
                BEGIN
                    SELECT COUNT(*) INTO v_info_cnt
                      FROM user_tab_columns
                     WHERE table_name = 'announcement' AND column_name = 'info';
                    SELECT COUNT(*) INTO v_content_cnt
                      FROM user_tab_columns
                     WHERE table_name = 'announcement' AND column_name = 'content';
                    SELECT COUNT(*) INTO v_publish_cnt
                      FROM user_tab_columns
                     WHERE table_name = 'announcement' AND column_name = 'publish_time';

                    IF v_info_cnt > 0 AND v_content_cnt = 0 THEN
                        EXECUTE IMMEDIATE 'ALTER TABLE "announcement" RENAME COLUMN "info" TO "content"';
                        EXECUTE IMMEDIATE 'ALTER TABLE "announcement" MODIFY ("content" NVARCHAR2(4000))';
                    ELSIF v_info_cnt > 0 AND v_content_cnt > 0 THEN
                        EXECUTE IMMEDIATE 'UPDATE "announcement" SET "content" = "info" WHERE "content" IS NULL';
                        EXECUTE IMMEDIATE 'ALTER TABLE "announcement" DROP COLUMN "info"';
                    END IF;

                    IF v_publish_cnt = 0 THEN
                        EXECUTE IMMEDIATE 'ALTER TABLE "announcement" ADD ("publish_time" TIMESTAMP(7))';
                    END IF;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE
                    v_info_cnt NUMBER;
                    v_content_cnt NUMBER;
                    v_publish_cnt NUMBER;
                BEGIN
                    SELECT COUNT(*) INTO v_info_cnt
                      FROM user_tab_columns
                     WHERE table_name = 'announcement' AND column_name = 'info';
                    SELECT COUNT(*) INTO v_content_cnt
                      FROM user_tab_columns
                     WHERE table_name = 'announcement' AND column_name = 'content';
                    SELECT COUNT(*) INTO v_publish_cnt
                      FROM user_tab_columns
                     WHERE table_name = 'announcement' AND column_name = 'publish_time';

                    IF v_content_cnt > 0 AND v_info_cnt = 0 THEN
                        EXECUTE IMMEDIATE 'ALTER TABLE "announcement" RENAME COLUMN "content" TO "info"';
                        EXECUTE IMMEDIATE 'ALTER TABLE "announcement" MODIFY ("info" NVARCHAR2(2000))';
                    ELSIF v_content_cnt > 0 AND v_info_cnt > 0 THEN
                        EXECUTE IMMEDIATE 'UPDATE "announcement" SET "info" = "content" WHERE "info" IS NULL';
                        EXECUTE IMMEDIATE 'ALTER TABLE "announcement" DROP COLUMN "content"';
                    END IF;

                    IF v_publish_cnt > 0 THEN
                        EXECUTE IMMEDIATE 'ALTER TABLE "announcement" DROP COLUMN "publish_time"';
                    END IF;
                END;
                """);
        }
    }
}
