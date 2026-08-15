-- =============================================================
-- 校园二手交易平台 种子数据
-- 所有表名/列名为小写（EF Core 引号建表），故全部加双引号
-- 密码复用现有账号 zyzhuang 的 BCrypt hash（密码与 zyzhuang 一致）
-- =============================================================
SET DEFINE OFF

-- ---------- 用户 base_user + norm_user ----------
INSERT INTO "base_user" ("user_id","email","pw_hash","phone_number","user_type","gender","is_banned","register_time")
VALUES (10,'xiaoming@tongji.edu.cn','$2a$11$CkxT78dJSFVpoujUyk40N.PZt7OOYGpy3bDLphSHteCIuVXjsF0WW','13800138001',0,'male',0,TIMESTAMP '2026-07-01 09:00:00');
INSERT INTO "base_user" ("user_id","email","pw_hash","phone_number","user_type","gender","is_banned","register_time")
VALUES (11,'xiaohong@tongji.edu.cn','$2a$11$CkxT78dJSFVpoujUyk40N.PZt7OOYGpy3bDLphSHteCIuVXjsF0WW','13800138002',0,'female',0,TIMESTAMP '2026-07-03 14:20:00');
INSERT INTO "base_user" ("user_id","email","pw_hash","phone_number","user_type","gender","is_banned","register_time")
VALUES (12,'xiaogang@tongji.edu.cn','$2a$11$CkxT78dJSFVpoujUyk40N.PZt7OOYGpy3bDLphSHteCIuVXjsF0WW','13800138003',0,'male',0,TIMESTAMP '2026-07-05 20:11:00');
INSERT INTO "base_user" ("user_id","email","pw_hash","phone_number","user_type","gender","is_banned","register_time")
VALUES (13,'xiaoli@tongji.edu.cn','$2a$11$CkxT78dJSFVpoujUyk40N.PZt7OOYGpy3bDLphSHteCIuVXjsF0WW','13800138004',0,'female',0,TIMESTAMP '2026-07-08 11:40:00');

INSERT INTO "norm_user" ("user_id","user_name","credit","profile") VALUES (10,'小明',90,'同济大学 计算机系');
INSERT INTO "norm_user" ("user_id","user_name","credit","profile") VALUES (11,'小红',95,'同济大学 软件学院');
INSERT INTO "norm_user" ("user_id","user_name","credit","profile") VALUES (12,'小刚',88,'同济大学 土木工程');
INSERT INTO "norm_user" ("user_id","user_name","credit","profile") VALUES (13,'小丽',92,'同济大学 建筑系');

-- ---------- 分类 category（两级） ----------
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (1,'教材书籍',NULL);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (2,'电子产品',NULL);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (3,'生活用品',NULL);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (4,'体育用品',NULL);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (5,'服饰箱包',NULL);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (11,'教材',1);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (12,'课外书',1);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (13,'考研资料',1);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (21,'手机',2);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (22,'电脑',2);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (23,'配件',2);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (31,'家具',3);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (32,'日用品',3);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (33,'文具',3);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (41,'球类',4);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (42,'健身器材',4);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (51,'男装',5);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (52,'女装',5);
INSERT INTO "category" ("category_id","category_name","parent_id") VALUES (53,'箱包',5);

-- ---------- 商品 product ----------
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (100,'高等数学（第七版）上册 同济大学',25.00,'九成新，笔记很少，期末复习用书，可当面交易。',TIMESTAMP '2026-07-10 10:00:00',0,10,11,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (101,'线性代数（第六版）同济大学',18.00,'几乎全新，仅封面有轻微折痕。',TIMESTAMP '2026-07-10 10:30:00',0,10,11,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (102,'iPhone 12 128G 国行',1800.00,'自用机，电池健康 88%，无维修记录，附原装充电线。',TIMESTAMP '2026-07-12 15:00:00',0,11,21,2,15,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (103,'联想小新 Pro 14 笔记本',2500.00,'i5-13500H 16G+512G，成色新，已售出。',TIMESTAMP '2026-07-15 09:00:00',1,11,22,2,20,0);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (104,'宜家台灯',30.00,'白色，使用一年，功能正常，灯泡自备。',TIMESTAMP '2026-07-18 13:00:00',0,12,32,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (105,'简易书架（五层）',80.00,'木质五层书架，可拆装，需自提或协商运费。',TIMESTAMP '2026-07-18 14:00:00',0,12,31,2,30,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (106,'考研英语一真题 2025 版',35.00,'含解析，八成新，部分页有铅笔标注已擦除。',TIMESTAMP '2026-07-20 19:00:00',0,10,13,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (107,'斯伯丁篮球',50.00,'七成新，气压正常，附打气筒。',TIMESTAMP '2026-07-22 10:00:00',0,2,41,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (108,'可调节哑铃一对 10kg',120.00,'家用哑铃一对，可调节重量，适合健身入门。',TIMESTAMP '2026-07-22 11:00:00',0,2,42,2,25,0);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (109,'波司登羽绒服 男款 L 码',200.00,'黑色，穿了一冬，已干洗，无破损。',TIMESTAMP '2026-07-25 16:00:00',0,13,51,2,15,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (110,'单肩帆布包',45.00,'深蓝色帆布单肩包，容量大，九成新。',TIMESTAMP '2026-07-25 16:30:00',0,13,53,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (111,'机械键盘 87 键 青轴',150.00,'IKBC 机械键盘，青轴，附键帽拔键器。',TIMESTAMP '2026-07-28 20:00:00',0,11,23,2,10,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (112,'《数据库系统概念》第六版',55.00,'教材，九五成新，无笔记。',TIMESTAMP '2026-07-30 09:00:00',0,10,12,0,0,1);

-- ---------- 商品留言 product_comment（含回复） ----------
INSERT INTO "product_comment" ("comment_id","product_id","user_id","content","index","create_time","ResponseToId")
VALUES (1000,100,2,'请问书有笔记吗？几成新？',0,TIMESTAMP '2026-08-01 10:05:00',NULL);
INSERT INTO "product_comment" ("comment_id","product_id","user_id","content","index","create_time","ResponseToId")
VALUES (1001,100,10,'笔记很少，九成新，可以当面交易。',1,TIMESTAMP '2026-08-01 10:20:00',1000);
INSERT INTO "product_comment" ("comment_id","product_id","user_id","content","index","create_time","ResponseToId")
VALUES (1002,102,13,'电池健康度多少？有没有磕碰？',0,TIMESTAMP '2026-08-06 15:10:00',NULL);
INSERT INTO "product_comment" ("comment_id","product_id","user_id","content","index","create_time","ResponseToId")
VALUES (1003,102,11,'电池健康 88%，边框轻微使用痕迹。',1,TIMESTAMP '2026-08-06 15:40:00',1002);
INSERT INTO "product_comment" ("comment_id","product_id","user_id","content","index","create_time","ResponseToId")
VALUES (1004,107,11,'篮球气压还足吗？',0,TIMESTAMP '2026-08-09 12:00:00',NULL);
INSERT INTO "product_comment" ("comment_id","product_id","user_id","content","index","create_time","ResponseToId")
VALUES (1005,107,2,'气压正常，一直放在气筒边上。',1,TIMESTAMP '2026-08-09 12:30:00',1004);

-- ---------- 收藏 collection ----------
INSERT INTO "collection" ("product_id","user_id","collection_time") VALUES (102,2,TIMESTAMP '2026-08-06 16:00:00');
INSERT INTO "collection" ("product_id","user_id","collection_time") VALUES (106,2,TIMESTAMP '2026-08-11 09:00:00');
INSERT INTO "collection" ("product_id","user_id","collection_time") VALUES (109,2,TIMESTAMP '2026-08-12 10:00:00');
INSERT INTO "collection" ("product_id","user_id","collection_time") VALUES (111,2,TIMESTAMP '2026-07-21 20:30:00');
INSERT INTO "collection" ("product_id","user_id","collection_time") VALUES (100,13,TIMESTAMP '2026-08-02 11:00:00');
INSERT INTO "collection" ("product_id","user_id","collection_time") VALUES (107,13,TIMESTAMP '2026-08-08 09:30:00');

-- ---------- 收货地址 address ----------
INSERT INTO "address" ("address_id","name","phone_number","detail_address","user_id","is_default")
VALUES (200,'张同学','13800138000','上海市杨浦区四平路1239号 同济大学四平路校区 3号楼501',2,1);
INSERT INTO "address" ("address_id","name","phone_number","detail_address","user_id","is_default")
VALUES (201,'张同学','13800138000','上海市杨浦区彰武路100号 同济大学彰武路校区 2号楼202',2,0);

-- ---------- 订单 purchase ----------
INSERT INTO "purchase" ("purchase_id","status","create_time","pay_time","shipping_time","delivery_time","complete_time","shipping_fees","responsible_for_ship","buyer_id","product_id","address_id","receiving_address","shipping_address","shipping_method","tracking_number")
VALUES (300,'success',TIMESTAMP '2026-08-01 10:00:00',TIMESTAMP '2026-08-01 10:15:00',TIMESTAMP '2026-08-02 09:00:00',TIMESTAMP '2026-08-03 14:00:00',TIMESTAMP '2026-08-05 10:00:00',0,0,2,100,200,'上海市杨浦区四平路1239号 同济大学四平路校区 3号楼501','上海市杨浦区四平路1239号 同济大学四平路校区','快递','SF1234567890');
INSERT INTO "purchase" ("purchase_id","status","create_time","pay_time","shipping_time","shipping_fees","responsible_for_ship","buyer_id","product_id","address_id","receiving_address","shipping_address","shipping_method","tracking_number")
VALUES (301,'shipping',TIMESTAMP '2026-08-06 15:00:00',TIMESTAMP '2026-08-06 15:20:00',TIMESTAMP '2026-08-08 09:00:00',15,1,2,102,200,'上海市杨浦区四平路1239号 同济大学四平路校区 3号楼501','上海市嘉定区安亭镇曹安公路4800号','顺丰速运','SF9876543210');
INSERT INTO "purchase" ("purchase_id","status","create_time","pay_time","shipping_fees","responsible_for_ship","buyer_id","product_id","address_id","receiving_address")
VALUES (302,'paid',TIMESTAMP '2026-08-10 11:00:00',TIMESTAMP '2026-08-10 11:10:00',0,0,2,104,201,'上海市杨浦区彰武路100号 同济大学彰武路校区 2号楼202');
INSERT INTO "purchase" ("purchase_id","status","create_time","shipping_fees","responsible_for_ship","buyer_id","product_id","address_id","receiving_address")
VALUES (303,'pending',TIMESTAMP '2026-08-12 10:00:00',15,1,2,109,201,'上海市杨浦区彰武路100号 同济大学彰武路校区 2号楼202');
INSERT INTO "purchase" ("purchase_id","status","create_time","cancel_time","shipping_fees","responsible_for_ship","buyer_id","product_id","address_id","receiving_address")
VALUES (305,'cancel',TIMESTAMP '2026-08-11 09:00:00',TIMESTAMP '2026-08-12 09:30:00',0,0,2,106,200,'上海市杨浦区四平路1239号 同济大学四平路校区 3号楼501');
INSERT INTO "purchase" ("purchase_id","status","create_time","pay_time","shipping_time","delivery_time","complete_time","shipping_fees","responsible_for_ship","buyer_id","product_id","address_id","receiving_address","shipping_address","shipping_method","tracking_number")
VALUES (306,'success',TIMESTAMP '2026-07-20 10:00:00',TIMESTAMP '2026-07-20 10:15:00',TIMESTAMP '2026-07-21 09:00:00',TIMESTAMP '2026-07-22 14:00:00',TIMESTAMP '2026-07-24 10:00:00',10,1,2,111,200,'上海市杨浦区四平路1239号 同济大学四平路校区 3号楼501','上海市嘉定区安亭镇曹安公路4800号','快递','YT888777666');

-- ---------- 支付 payment ----------
INSERT INTO "payment" ("payment_id","status","payment_method","amount","transaction_id","create_time","pay_time","purchase_id")
VALUES (400,1,0,25.00,'ALI20260801001',TIMESTAMP '2026-08-01 10:15:00',TIMESTAMP '2026-08-01 10:15:00',300);
INSERT INTO "payment" ("payment_id","status","payment_method","amount","transaction_id","create_time","pay_time","purchase_id")
VALUES (401,1,1,1815.00,'WX20260806002',TIMESTAMP '2026-08-06 15:20:00',TIMESTAMP '2026-08-06 15:20:00',301);
INSERT INTO "payment" ("payment_id","status","payment_method","amount","transaction_id","create_time","pay_time","purchase_id")
VALUES (402,1,0,30.00,'ALI20260810003',TIMESTAMP '2026-08-10 11:10:00',TIMESTAMP '2026-08-10 11:10:00',302);
INSERT INTO "payment" ("payment_id","status","payment_method","amount","create_time","purchase_id")
VALUES (403,0,1,215.00,TIMESTAMP '2026-08-12 10:00:00',303);
INSERT INTO "payment" ("payment_id","status","payment_method","amount","create_time","cancel_time","purchase_id")
VALUES (404,3,0,35.00,TIMESTAMP '2026-08-11 09:00:00',TIMESTAMP '2026-08-12 09:30:00',305);
INSERT INTO "payment" ("payment_id","status","payment_method","amount","transaction_id","create_time","pay_time","purchase_id")
VALUES (405,1,0,160.00,'ALI20260720004',TIMESTAMP '2026-07-20 10:15:00',TIMESTAMP '2026-07-20 10:15:00',306);

-- ---------- 评价 review ----------
INSERT INTO "review" ("review_id","rating","info","review_time","purchase_id","is_hidden","reply_info","reply_time")
VALUES (500,5,'书很新，卖家发货快，好评！',TIMESTAMP '2026-08-06 10:00:00',300,0,'谢谢支持，欢迎再来！',TIMESTAMP '2026-08-07 09:00:00');
INSERT INTO "review" ("review_id","rating","info","review_time","purchase_id","is_hidden")
VALUES (501,4,'键盘手感不错，略有使用痕迹，整体满意。',TIMESTAMP '2026-07-25 10:00:00',306,0);

-- ---------- 订单时间线 order_timeline ----------
INSERT INTO "order_timeline" ("timeline_id","old_status","new_status","change_time","operator_id","note","purchase_id")
VALUES (600,NULL,'pending',TIMESTAMP '2026-08-01 10:00:00',2,'创建订单',300);
INSERT INTO "order_timeline" ("timeline_id","old_status","new_status","change_time","operator_id","note","purchase_id")
VALUES (601,'pending','paid',TIMESTAMP '2026-08-01 10:15:00',2,'支付成功',300);
INSERT INTO "order_timeline" ("timeline_id","old_status","new_status","change_time","operator_id","note","purchase_id")
VALUES (602,'paid','shipping',TIMESTAMP '2026-08-02 09:00:00',10,'卖家已发货',300);
INSERT INTO "order_timeline" ("timeline_id","old_status","new_status","change_time","operator_id","note","purchase_id")
VALUES (603,'shipping','success',TIMESTAMP '2026-08-05 10:00:00',2,'确认收货',300);
INSERT INTO "order_timeline" ("timeline_id","old_status","new_status","change_time","operator_id","note","purchase_id")
VALUES (604,NULL,'pending',TIMESTAMP '2026-08-06 15:00:00',2,'创建订单',301);
INSERT INTO "order_timeline" ("timeline_id","old_status","new_status","change_time","operator_id","note","purchase_id")
VALUES (605,'pending','paid',TIMESTAMP '2026-08-06 15:20:00',2,'支付成功',301);
INSERT INTO "order_timeline" ("timeline_id","old_status","new_status","change_time","operator_id","note","purchase_id")
VALUES (606,'paid','shipping',TIMESTAMP '2026-08-08 09:00:00',11,'卖家已发货，顺丰速运',301);
INSERT INTO "order_timeline" ("timeline_id","old_status","new_status","change_time","operator_id","note","purchase_id")
VALUES (607,NULL,'pending',TIMESTAMP '2026-08-12 10:00:00',2,'创建订单',303);

-- ---------- 商品浏览记录 product_view ----------
INSERT INTO "product_view" ("view_id","user_id","product_id","view_time") VALUES (700,2,102,TIMESTAMP '2026-08-06 15:00:00');
INSERT INTO "product_view" ("view_id","user_id","product_id","view_time") VALUES (701,2,100,TIMESTAMP '2026-08-01 09:50:00');
INSERT INTO "product_view" ("view_id","user_id","product_id","view_time") VALUES (702,2,104,TIMESTAMP '2026-08-10 10:50:00');
INSERT INTO "product_view" ("view_id","user_id","product_id","view_time") VALUES (703,2,111,TIMESTAMP '2026-07-20 09:50:00');
INSERT INTO "product_view" ("view_id","user_id","product_id","view_time") VALUES (704,2,109,TIMESTAMP '2026-08-12 09:50:00');
INSERT INTO "product_view" ("view_id","user_id","product_id","view_time") VALUES (705,13,102,TIMESTAMP '2026-08-07 14:00:00');
INSERT INTO "product_view" ("view_id","user_id","product_id","view_time") VALUES (706,13,100,TIMESTAMP '2026-08-02 11:00:00');

COMMIT;

-- ---------- 重置自增序列，避免与显式 ID 冲突 ----------
ALTER SEQUENCE "ISEQ$$_72910" RESTART START WITH 100;  -- base_user.user_id
ALTER SEQUENCE "ISEQ$$_72913" RESTART START WITH 100;  -- category.category_id
ALTER SEQUENCE "ISEQ$$_72926" RESTART START WITH 200;  -- product.product_id
ALTER SEQUENCE "ISEQ$$_72988" RESTART START WITH 2000; -- product_comment.comment_id
ALTER SEQUENCE "ISEQ$$_72923" RESTART START WITH 300;  -- address.address_id
ALTER SEQUENCE "ISEQ$$_72940" RESTART START WITH 400;  -- purchase.purchase_id
ALTER SEQUENCE "ISEQ$$_73011" RESTART START WITH 500;  -- payment.payment_id
ALTER SEQUENCE "ISEQ$$_72951" RESTART START WITH 600;  -- review.review_id
ALTER SEQUENCE "ISEQ$$_73008" RESTART START WITH 700;  -- order_timeline.timeline_id
ALTER SEQUENCE "ISEQ$$_72991" RESTART START WITH 800;  -- product_view.view_id

EXIT;
