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
                name: "base_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    email = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    pw_hash = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: true),
                    user_type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    avatar_file_id = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    gender = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    is_banned = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    banned_until = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    account_status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    register_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_base_user", x => x.user_id);
                });

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
                    is_deleted = table.Column<bool>(type: "NUMBER(1)", nullable: false),
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
                name: "product",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    info = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    release_date = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    status = table.Column<int>(type: "NUMBER(10)", maxLength: 10, nullable: false),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    category_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    shipping_type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    shipping_fee = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    allow_pickup = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    reject_reason = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    reviewed_by = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_admin_user_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "admin_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "user_warning",
                columns: table => new
                {
                    warning_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    user_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    admin_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    reason = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_warning", x => x.warning_id);
                    table.ForeignKey(
                        name: "FK_user_warning_admin_user_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admin_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_warning_norm_user_user_id",
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
                    img_file_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    img_index = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    product_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prod_image", x => x.img_file_id);
                    table.ForeignKey(
                        name: "FK_prod_image_files_img_file_id",
                        column: x => x.img_file_id,
                        principalTable: "files",
                        principalColumn: "file_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prod_image_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    address_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    shipping_method = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: true),
                    shipping_address = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    receiving_address = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    tracking_number = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true)
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
                    target_type = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    target_id = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    appeal_against_id = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    result = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    handle_action = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
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
                    table.ForeignKey(
                        name: "FK_work_order_work_order_appeal_against_id",
                        column: x => x.appeal_against_id,
                        principalTable: "work_order",
                        principalColumn: "work_order_id",
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
                    file_id = table.Column<long>(type: "NUMBER(19)", nullable: true),
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
                        name: "FK_message_files_file_id",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "file_id");
                    table.ForeignKey(
                        name: "FK_message_norm_user_sender_id",
                        column: x => x.sender_id,
                        principalTable: "norm_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    purchase_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    reply_info = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    reply_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    is_hidden = table.Column<int>(type: "NUMBER(10)", nullable: false)
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
                name: "work_order_timeline",
                columns: table => new
                {
                    timeline_id = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    work_order_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    action = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    note = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    admin_id = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    create_time = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_timeline", x => x.timeline_id);
                    table.ForeignKey(
                        name: "FK_work_order_timeline_admin_user_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admin_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_order_timeline_work_order_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "work_order",
                        principalColumn: "work_order_id",
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
                    img_file_id = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    img_index = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    review_id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rev_image", x => x.img_file_id);
                    table.ForeignKey(
                        name: "FK_rev_image_files_img_file_id",
                        column: x => x.img_file_id,
                        principalTable: "files",
                        principalColumn: "file_id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_base_user_avatar_file_id",
                table: "base_user",
                column: "avatar_file_id");

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
                name: "IX_files_uploader_id",
                table: "files",
                column: "uploader_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_file_id",
                table: "message",
                column: "file_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_sender_id",
                table: "message",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_timeline_purchase_id",
                table: "order_timeline",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_purchase_id",
                table: "payment",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "IX_prod_image_product_id",
                table: "prod_image",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_category_id",
                table: "product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_reviewed_by",
                table: "product",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_product_user_id",
                table: "product",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_audit_log_admin_id",
                table: "product_audit_log",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_audit_log_product_id",
                table: "product_audit_log",
                column: "product_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_sys_info_user_id",
                table: "sys_info",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_warning_admin_id",
                table: "user_warning",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_warning_user_id",
                table: "user_warning",
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
                name: "IX_work_order_appeal_against_id",
                table: "work_order",
                column: "appeal_against_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_initiator_id",
                table: "work_order",
                column: "initiator_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_product_id",
                table: "work_order",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_timeline_admin_id",
                table: "work_order_timeline",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_timeline_work_order_id",
                table: "work_order_timeline",
                column: "work_order_id");

            migrationBuilder.AddForeignKey(
                name: "FK_address_norm_user_user_id",
                table: "address",
                column: "user_id",
                principalTable: "norm_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_admin_user_base_user_user_id",
                table: "admin_user",
                column: "user_id",
                principalTable: "base_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_base_user_files_avatar_file_id",
                table: "base_user",
                column: "avatar_file_id",
                principalTable: "files",
                principalColumn: "file_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_files_base_user_uploader_id",
                table: "files");

            migrationBuilder.DropTable(
                name: "announcement");

            migrationBuilder.DropTable(
                name: "collection");

            migrationBuilder.DropTable(
                name: "message");

            migrationBuilder.DropTable(
                name: "order_timeline");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "prod_image");

            migrationBuilder.DropTable(
                name: "product_audit_log");

            migrationBuilder.DropTable(
                name: "product_comment");

            migrationBuilder.DropTable(
                name: "product_view");

            migrationBuilder.DropTable(
                name: "refund_review");

            migrationBuilder.DropTable(
                name: "rev_image");

            migrationBuilder.DropTable(
                name: "search_term_edge");

            migrationBuilder.DropTable(
                name: "search_term_similarity");

            migrationBuilder.DropTable(
                name: "sys_info");

            migrationBuilder.DropTable(
                name: "user_warning");

            migrationBuilder.DropTable(
                name: "work_order_timeline");

            migrationBuilder.DropTable(
                name: "conversation");

            migrationBuilder.DropTable(
                name: "refund");

            migrationBuilder.DropTable(
                name: "review");

            migrationBuilder.DropTable(
                name: "search_term");

            migrationBuilder.DropTable(
                name: "work_order");

            migrationBuilder.DropTable(
                name: "purchase");

            migrationBuilder.DropTable(
                name: "address");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "admin_user");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "norm_user");

            migrationBuilder.DropTable(
                name: "base_user");

            migrationBuilder.DropTable(
                name: "files");
        }
    }
}
