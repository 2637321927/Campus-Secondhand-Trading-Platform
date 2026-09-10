-- =============================================================
-- 校园二手交易平台 测试数据（补充 seed_data.sql 未覆盖的模块）
-- 表名/列名为小写（EF Core 引号建表），故全部加双引号
-- 密码复用现有账号 zyzhuang 的 BCrypt hash（密码与 zyzhuang 一致）
-- 显式 ID 从 9000 起，避免与已有数据 / 自增序列冲突；末尾统一重置序列
-- 执行方式：
--   docker exec -e NLS_LANG=AMERICAN_AMERICA.AL32UTF8 -i oracle26ai \
--     sqlplus -s system/2026DBScProj@//127.0.0.1:1521/FREEPDB1 < test_data.sql
-- =============================================================
SET DEFINE OFF
SET FEEDBACK OFF

-- ---------- 1. 管理员账号 base_user + admin_user ----------
INSERT INTO "base_user" ("user_id","email","pw_hash","phone_number","user_type","gender","is_banned","account_status","register_time")
VALUES (9001,'admin@tongji.edu.cn','$2a$11$CkxT78dJSFVpoujUyk40N.PZt7OOYGpy3bDLphSHteCIuVXjsF0WW','13800009001',1,'unknown',0,0,TIMESTAMP '2026-08-01 08:00:00');
INSERT INTO "base_user" ("user_id","email","pw_hash","phone_number","user_type","gender","is_banned","account_status","register_time")
VALUES (9002,'moderator@tongji.edu.cn','$2a$11$CkxT78dJSFVpoujUyk40N.PZt7OOYGpy3bDLphSHteCIuVXjsF0WW','13800009002',1,'unknown',0,0,TIMESTAMP '2026-08-02 08:30:00');

INSERT INTO "admin_user" ("user_id","permission") VALUES (9001,5);
INSERT INTO "admin_user" ("user_id","permission") VALUES (9002,1);

-- ---------- 2. 商品 product（覆盖各种审核/管理状态） ----------
-- 状态：0=在售 1=已售 2=已下架 3=待审核 4=驳回 5=交易中 6=强制下架
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (9000,'高等数学（下）第七版 同济大学',22.00,'九成新，笔记很少，与上册配套。',TIMESTAMP '2026-08-20 10:00:00',3,10,11,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (9001,'小米手环 8',120.00,'用了三个月，成色新，附充电线。',TIMESTAMP '2026-08-21 14:00:00',3,12,21,2,10,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup","reject_reason","reviewed_by","reviewed_at")
VALUES (9002,'罗技鼠标 G304',99.00,'无线鼠标，功能正常。',TIMESTAMP '2026-08-22 09:00:00',4,11,23,2,8,1,'图片与实物不符，请重新上传清晰图片',9001,TIMESTAMP '2026-08-23 10:00:00');
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (9003,'《平凡的世界》全三册',30.00,'正版，保存完好。',TIMESTAMP '2026-08-24 11:00:00',2,10,12,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup","reject_reason","reviewed_by","reviewed_at")
VALUES (9004,'iPhone 15 Pro 256G',5200.00,'国行在保。',TIMESTAMP '2026-08-25 15:00:00',6,13,21,2,20,0,'涉嫌售卖违规商品，已强制下架',9001,TIMESTAMP '2026-08-26 09:00:00');
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (9005,'戴尔显示器 24寸',400.00,'IPS 屏，无坏点，已被下单锁定。',TIMESTAMP '2026-08-26 16:00:00',5,11,22,2,30,0);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (9006,'考研政治复习全书 2026 版',45.00,'含冲刺卷，八成新。',TIMESTAMP '2026-08-27 09:00:00',0,10,13,0,0,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (9007,'尤尼克斯羽毛球拍',180.00,'碳素拍，附拍套，轻微使用痕迹。',TIMESTAMP '2026-08-28 10:00:00',0,13,41,2,12,1);
INSERT INTO "product" ("product_id","name","price","info","release_date","status","user_id","category_id","shipping_type","shipping_fee","allow_pickup")
VALUES (9008,'桌面收纳盒（多层）',25.00,'白色，九成新，桌面整理好帮手。',TIMESTAMP '2026-08-29 11:00:00',0,12,32,0,0,1);

-- ---------- 3. 商品审核日志 product_audit_log ----------
INSERT INTO "product_audit_log" ("audit_id","product_id","admin_id","action","reason","old_status","new_status","create_time")
VALUES (9000,9004,9001,'remove','违规商品强制下架',0,6,TIMESTAMP '2026-08-26 09:00:00');
INSERT INTO "product_audit_log" ("audit_id","product_id","admin_id","action","reason","old_status","new_status","create_time")
VALUES (9001,9002,9001,'reject','图片与实物不符',3,4,TIMESTAMP '2026-08-23 10:00:00');
INSERT INTO "product_audit_log" ("audit_id","product_id","admin_id","action","reason","old_status","new_status","create_time")
VALUES (9002,9006,9001,'approve','审核通过',3,0,TIMESTAMP '2026-08-28 09:00:00');
INSERT INTO "product_audit_log" ("audit_id","product_id","admin_id","action","reason","old_status","new_status","create_time")
VALUES (9003,9008,9001,'approve','审核通过',3,0,TIMESTAMP '2026-08-30 09:00:00');

-- ---------- 4. 工单 work_order（举报 type=1 / 申诉 type=2） ----------
-- 待处理举报
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","initiator_id","accused_id","product_id","target_type","target_id")
VALUES (9000,1,'商品描述与实际不符','图片显示九成新，实际收到有明显破损。',TIMESTAMP '2026-08-28 10:30:00','waiting',10,11,9006,'product',9006);
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","initiator_id","accused_id","target_type","target_id")
VALUES (9001,1,'卖家态度恶劣','沟通中卖家出言不逊。',TIMESTAMP '2026-08-29 11:00:00','waiting',10,11,'user',11);
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","initiator_id","accused_id","target_type","target_id")
VALUES (9002,1,'留言包含不当内容','该留言含有广告引流信息。',TIMESTAMP '2026-08-30 12:00:00','waiting',12,10,'comment',1000);
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","initiator_id","accused_id","target_type","target_id")
VALUES (9006,1,'订单纠纷','卖家发货与描述不一致，申请平台介入。',TIMESTAMP '2026-09-01 09:00:00','waiting',13,10,'order',300);

-- 已处理举报
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","response","response_time","initiator_id","accused_id","product_id","target_type","target_id","result","handle_action","admin_id")
VALUES (9003,1,'售卖疑似违规商品','该商品未提供正规来源证明。',TIMESTAMP '2026-08-25 15:30:00','done','已核实，商品已下架',TIMESTAMP '2026-08-26 09:00:00',10,13,9004,'product',9004,'handled','remove_product',9001);
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","response","response_time","initiator_id","accused_id","target_type","target_id","result","handle_action","admin_id")
VALUES (9004,1,'发布虚假商品信息','商品价格与实际严重不符。',TIMESTAMP '2026-08-27 10:00:00','done','已对用户进行警告',TIMESTAMP '2026-08-27 15:00:00',10,12,'user',12,'handled','warn_user',9001);
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","response","response_time","initiator_id","accused_id","target_type","target_id","result","handle_action","admin_id")
VALUES (9005,1,'多次发布违规内容','该用户近期多次被举报。',TIMESTAMP '2026-08-30 14:00:00','done','已对用户进行警告',TIMESTAMP '2026-08-31 09:00:00',11,13,'user',13,'handled','warn_user',9001);

-- 申诉
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","initiator_id","accused_id","product_id","target_type","target_id","appeal_against_id")
VALUES (9007,2,'申诉：商品来源正规','商品购于官方渠道，可提供发票。',TIMESTAMP '2026-08-26 10:00:00','waiting',13,10,9004,'product',9004,9003);
INSERT INTO "work_order" ("work_order_id","type","reason","info","create_time","status","response","response_time","initiator_id","accused_id","target_type","target_id","appeal_against_id","result","handle_action","admin_id")
VALUES (9008,2,'申诉：警告处理不当','举报内容失实，请求撤销警告。',TIMESTAMP '2026-08-28 09:00:00','done','申诉通过，撤销警告',TIMESTAMP '2026-08-29 09:00:00',12,10,'user',12,9004,'approved','none',9001);

-- ---------- 5. 工单时间线 work_order_timeline ----------
INSERT INTO "work_order_timeline" ("timeline_id","work_order_id","action","note","admin_id","create_time")
VALUES (10000,9003,'handle','已核实，商品已下架',9001,TIMESTAMP '2026-08-26 09:00:00');
INSERT INTO "work_order_timeline" ("timeline_id","work_order_id","action","note","admin_id","create_time")
VALUES (10001,9004,'handle','已对用户进行警告',9001,TIMESTAMP '2026-08-27 15:00:00');
INSERT INTO "work_order_timeline" ("timeline_id","work_order_id","action","note","admin_id","create_time")
VALUES (10002,9005,'handle','已对用户进行警告',9001,TIMESTAMP '2026-08-31 09:00:00');
INSERT INTO "work_order_timeline" ("timeline_id","work_order_id","action","note","admin_id","create_time")
VALUES (10003,9008,'reply','我们会尽快核实您的申诉',9001,TIMESTAMP '2026-08-28 09:30:00');
INSERT INTO "work_order_timeline" ("timeline_id","work_order_id","action","note","admin_id","create_time")
VALUES (10004,9008,'handle','申诉通过，撤销警告',9001,TIMESTAMP '2026-08-29 09:00:00');

-- ---------- 6. 公告 announcement ----------
INSERT INTO "announcement" ("announcement_id","title","content","is_pinned","status","release_time","publish_time","admin_id")
VALUES (9000,'平台交易规则更新公告','为规范校园二手交易，平台对以下规则进行调整：1. 所有商品须实名发布；2. 电子产品需提供购买凭证；3. 禁止售卖管制物品。请各位同学仔细阅读并遵守。',1,'published',TIMESTAMP '2026-08-10 09:00:00',TIMESTAMP '2026-08-10 09:00:00',9001);
INSERT INTO "announcement" ("announcement_id","title","content","is_pinned","status","release_time","publish_time","admin_id")
VALUES (9001,'暑期二手交易节活动通知','开学季来临，平台举办二手交易节，活动期间发布商品免手续费，欢迎踊跃参与！',0,'published',TIMESTAMP '2026-08-15 10:00:00',TIMESTAMP '2026-08-15 10:00:00',9001);
INSERT INTO "announcement" ("announcement_id","title","content","is_pinned","status","release_time","admin_id")
VALUES (9002,'关于账号实名认证的通知（草稿）','为保障交易安全，平台近期将上线账号实名认证功能，请提前准备学生证信息。',0,'draft',TIMESTAMP '2026-09-01 09:00:00',9001);
INSERT INTO "announcement" ("announcement_id","title","content","is_pinned","status","release_time","publish_time","admin_id")
VALUES (9003,'2025 学年秋季开学通知','新学期将于 9 月 1 日正式开学，欢迎新生加入二手交易平台。',0,'archived',TIMESTAMP '2026-08-01 08:00:00',TIMESTAMP '2026-08-01 08:00:00',9001);

-- ---------- 7. 系统通知 sys_info ----------
INSERT INTO "sys_info" ("sys_info_id","detailed","release_time","user_id")
VALUES (9000,'您的商品「高等数学（下）第七版」已提交审核，请耐心等待。',TIMESTAMP '2026-08-20 10:05:00',10);
INSERT INTO "sys_info" ("sys_info_id","detailed","release_time","user_id")
VALUES (9001,'您的商品「iPhone 15 Pro 256G」因违规已被下架，如有异议可发起申诉。',TIMESTAMP '2026-08-26 09:00:00',13);
INSERT INTO "sys_info" ("sys_info_id","detailed","release_time","user_id")
VALUES (9002,'您收到一条新的买家评价。',TIMESTAMP '2026-08-07 10:00:00',11);
INSERT INTO "sys_info" ("sys_info_id","detailed","release_time","user_id")
VALUES (9003,'平台将于今晚 23:00 进行系统维护，期间暂停访问。',TIMESTAMP '2026-09-05 18:00:00',2);
INSERT INTO "sys_info" ("sys_info_id","detailed","release_time","user_id")
VALUES (9004,'您的订单已发货，请注意查收。',TIMESTAMP '2026-08-08 09:00:00',2);

-- ---------- 8. 用户警告 user_warning ----------
INSERT INTO "user_warning" ("warning_id","user_id","admin_id","reason","create_time")
VALUES (9000,12,9001,'发布商品描述不实，予以警告。',TIMESTAMP '2026-08-27 15:00:00');
INSERT INTO "user_warning" ("warning_id","user_id","admin_id","reason","create_time")
VALUES (9001,13,9001,'多次发布违规商品，予以警告。',TIMESTAMP '2026-08-31 09:00:00');

-- ---------- 9. 会话 conversation + 消息 message ----------
INSERT INTO "conversation" ("session_id","create_time","product_id","buyer_id")
VALUES (9000,TIMESTAMP '2026-08-28 10:00:00',9006,11);
INSERT INTO "conversation" ("session_id","create_time","product_id","buyer_id")
VALUES (9001,TIMESTAMP '2026-08-07 14:00:00',102,13);

INSERT INTO "message" ("session_id","msg_index","sender_id","msg_type","msg_content","send_time","is_read")
VALUES (9000,0,11,0,'你好，这本书还在吗？',TIMESTAMP '2026-08-28 10:00:00',1);
INSERT INTO "message" ("session_id","msg_index","sender_id","msg_type","msg_content","send_time","is_read")
VALUES (9000,1,10,0,'还在的，可以当面交易。',TIMESTAMP '2026-08-28 10:05:00',1);
INSERT INTO "message" ("session_id","msg_index","sender_id","msg_type","msg_content","send_time","is_read")
VALUES (9000,2,11,0,'好的，周六下午三点图书馆门口见。',TIMESTAMP '2026-08-28 10:08:00',0);
INSERT INTO "message" ("session_id","msg_index","sender_id","msg_type","msg_content","send_time","is_read")
VALUES (9001,0,13,0,'电池健康度能发图看看吗？',TIMESTAMP '2026-08-07 14:00:00',1);
INSERT INTO "message" ("session_id","msg_index","sender_id","msg_type","msg_content","send_time","is_read")
VALUES (9001,1,11,0,'可以，稍后发你。',TIMESTAMP '2026-08-07 14:10:00',0);

COMMIT;

-- ---------- 重置自增序列，避免与显式 ID 冲突 ----------
ALTER SEQUENCE "ISEQ$$_72910" RESTART START WITH 20000;  -- base_user.user_id
ALTER SEQUENCE "ISEQ$$_72926" RESTART START WITH 20000;  -- product.product_id
ALTER SEQUENCE "ISEQ$$_72943" RESTART START WITH 20000;  -- work_order.work_order_id
ALTER SEQUENCE "ISEQ$$_73063" RESTART START WITH 20000;  -- work_order_timeline.timeline_id
ALTER SEQUENCE "ISEQ$$_72920" RESTART START WITH 20000;  -- announcement.announcement_id
ALTER SEQUENCE "ISEQ$$_72929" RESTART START WITH 20000;  -- sys_info.sys_info_id
ALTER SEQUENCE "ISEQ$$_73052" RESTART START WITH 20000;  -- user_warning.warning_id
ALTER SEQUENCE "ISEQ$$_73057" RESTART START WITH 20000;  -- product_audit_log.audit_id
ALTER SEQUENCE "ISEQ$$_72934" RESTART START WITH 20000;  -- conversation.session_id

EXIT;
