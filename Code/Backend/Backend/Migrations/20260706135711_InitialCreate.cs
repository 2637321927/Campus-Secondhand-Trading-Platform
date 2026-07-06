using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "base_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    email = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    pw_hash = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: true),
                    user_type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    avatar_url = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: true),
                    gender = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    is_banned = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    banned_until = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    register_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_base_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    category_name = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    parent_id = table.Column<long>(type: "NUMBER(19)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.category_id);
                    table.ForeignKey(
                        name: "FK_category_category_parent_id",
                        column: x => x.parent_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "admin_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    permission = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_user", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_admin_user_base_user_user_id",
                        column: x => x.user_id,
                        principalTable: "base_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "norm_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    user_name = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    credit = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    profile = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_norm_user", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_norm_user_base_user_user_id",
                        column: x => x.user_id,
                        principalTable: "base_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "announcement",
                columns: table => new
                {
                    announcement_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    title = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    info = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    release_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    admin_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcement", x => x.announcement_id);
                    table.ForeignKey(
                        name: "FK_announcement_admin_user_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admin_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "address",
                columns: table => new
                {
                    address_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    phone_number = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    detail_address = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    is_default = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address", x => x.address_id);
                    table.ForeignKey(
                        name: "FK_address_norm_user_user_id",
                        column: x => x.user_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    info = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    release_date = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    status = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    category_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_norm_user_user_id",
                        column: x => x.user_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sys_info",
                columns: table => new
                {
                    sys_info_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    detailed = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    release_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_info", x => x.sys_info_id);
                    table.ForeignKey(
                        name: "FK_sys_info_norm_user_user_id",
                        column: x => x.user_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "collection",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    collection_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection", x => new { x.product_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_collection_norm_user_user_id",
                        column: x => x.user_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_collection_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversation",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    buyer_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_conversation_norm_user_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_conversation_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prod_image",
                columns: table => new
                {
                    img_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    img_url = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    img_index = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prod_image", x => x.img_id);
                    table.ForeignKey(
                        name: "FK_prod_image_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase",
                columns: table => new
                {
                    purchase_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    status = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    cancel_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    pay_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    shipping_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    delivery_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    complete_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    shipping_fees = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    responsible_for_ship = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    buyer_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    address_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase", x => x.purchase_id);
                    table.ForeignKey(
                        name: "FK_purchase_address_address_id",
                        column: x => x.address_id,
                        principalTable: "address",
                        principalColumn: "address_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_norm_user_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "work_order",
                columns: table => new
                {
                    work_order_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    reason = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    info = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    status = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    response = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    response_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    initiator_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    accused_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    admin_id = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order", x => x.work_order_id);
                    table.ForeignKey(
                        name: "FK_work_order_admin_user_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admin_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_order_norm_user_accused_id",
                        column: x => x.accused_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_order_norm_user_initiator_id",
                        column: x => x.initiator_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_order_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "message",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    msg_index = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    sender_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    msg_type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    msg_content = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    send_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    is_read = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message", x => new { x.session_id, x.msg_index });
                    table.ForeignKey(
                        name: "FK_message_conversation_session_id",
                        column: x => x.session_id,
                        principalTable: "conversation",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_message_norm_user_sender_id",
                        column: x => x.sender_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refund",
                columns: table => new
                {
                    refund_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    reason = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    shipping_fees = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    responsible_for_ship = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    apply_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    purchase_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund", x => x.refund_id);
                    table.ForeignKey(
                        name: "FK_refund_purchase_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchase",
                        principalColumn: "purchase_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    rating = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    info = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    review_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    purchase_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_review_purchase_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchase",
                        principalColumn: "purchase_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refund_review",
                columns: table => new
                {
                    refund_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    reviewer_type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    result = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    review_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    info = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refund_review", x => new { x.refund_id, x.reviewer_type });
                    table.ForeignKey(
                        name: "FK_refund_review_refund_refund_id",
                        column: x => x.refund_id,
                        principalTable: "refund",
                        principalColumn: "refund_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rev_image",
                columns: table => new
                {
                    img_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    img_url = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    img_index = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    review_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rev_image", x => x.img_id);
                    table.ForeignKey(
                        name: "FK_rev_image_review_review_id",
                        column: x => x.review_id,
                        principalTable: "review",
                        principalColumn: "review_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_address_user_id",
                table: "address",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_admin_id",
                table: "announcement",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_base_user_email",
                table: "base_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_base_user_phone_number",
                table: "base_user",
                column: "phone_number",
                unique: true,
                filter: "\"phone_number\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_category_parent_id",
                table: "category",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_collection_user_id",
                table: "collection",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_buyer_id",
                table: "conversation",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_product_id",
                table: "conversation",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_sender_id",
                table: "message",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_prod_image_product_id",
                table: "prod_image",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_category_id",
                table: "product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_user_id",
                table: "product",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_address_id",
                table: "purchase",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_buyer_id",
                table: "purchase",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_product_id",
                table: "purchase",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_refund_purchase_id",
                table: "refund",
                column: "purchase_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rev_image_review_id",
                table: "rev_image",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_purchase_id",
                table: "review",
                column: "purchase_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_info_user_id",
                table: "sys_info",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_accused_id",
                table: "work_order",
                column: "accused_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_admin_id",
                table: "work_order",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_initiator_id",
                table: "work_order",
                column: "initiator_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_product_id",
                table: "work_order",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "announcement");

            migrationBuilder.DropTable(
                name: "collection");

            migrationBuilder.DropTable(
                name: "message");

            migrationBuilder.DropTable(
                name: "prod_image");

            migrationBuilder.DropTable(
                name: "refund_review");

            migrationBuilder.DropTable(
                name: "rev_image");

            migrationBuilder.DropTable(
                name: "sys_info");

            migrationBuilder.DropTable(
                name: "work_order");

            migrationBuilder.DropTable(
                name: "conversation");

            migrationBuilder.DropTable(
                name: "refund");

            migrationBuilder.DropTable(
                name: "review");

            migrationBuilder.DropTable(
                name: "admin_user");

            migrationBuilder.DropTable(
                name: "purchase");

            migrationBuilder.DropTable(
                name: "address");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "norm_user");

            migrationBuilder.DropTable(
                name: "base_user");
        }
    }
}
