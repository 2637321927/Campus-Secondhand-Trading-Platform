# 校园二手交易平台（Campus Secondhand Trading Platform）

一个面向校园场景的二手物品交易平台，采用**前后端分离**架构：用户既可作为买家浏览、下单与评价，也可作为卖家发布、管理自己的闲置物品；平台管理员负责用户治理、商品审核、举报申诉仲裁与公告运营。

- 后端：`Code/Backend/Backend`（.NET 8 + ASP.NET Core Web API + EF Core + Oracle）
- 前端：`Code/Frontend`（Vue 3 + TypeScript + Vite + Element Plus）

---

## 一、技术栈

### 后端

| 类别 | 技术 | 版本 |
|------|------|------|
| 开发框架 | ASP.NET Core Web API | 8.0（net8.0） |
| ORM | Entity Framework Core | 8.0.3 |
| 数据库 | Oracle | 21c+（Oracle.EntityFrameworkCore 8.23.60） |
| 身份认证 | JWT Bearer | 8.0.11 |
| 密码安全 | BCrypt.Net-Next | 4.2.0 |
| 接口文档 | Swagger（Swashbuckle） | 6.6.2 |
| 中文分词 | jieba.NET | 0.42.2 |

### 前端

| 类别 | 技术 | 版本 |
|------|------|------|
| 框架 | Vue 3（Composition API） | 3.5.x |
| 语言 | TypeScript | 6.0.x |
| 构建工具 | Vite | 8.1.x |
| UI 组件库 | Element Plus | 2.14.x |
| 状态管理 | Pinia | 4.0.x |
| 路由 | Vue Router | 5.2.x |
| HTTP 客户端 | Axios | 1.19.x |
| 类型检查 | vue-tsc | 3.3.x |

---

## 二、系统架构

```
浏览器（Vue 3 SPA）
   │  RESTful + JSON + Authorization: Bearer <JWT>
   ▼
ASP.NET Core Web API
   ├─ Controller 层：路由、鉴权、参数接收，取 userId 后交给业务层
   ├─ Service 层：业务规则、状态机、缓存、推荐计算
   ├─ Repository 层：EF Core 数据访问（查询 / 保存）
   └─ 辅助能力：搜索子系统（jieba 分词 + 词条图）、文件服务（Uploads）、JWT 认证
   ▼
Oracle 数据库（表结构由 EF Core Migrations 管理）
```

- **分层约束**：Controller 不写业务判断、不直接访问数据库；Service 不接触 HttpContext/JWT；Repository 只做数据存取
- **JWT 只在 Controller 感知**：解析出 `int userId` 后以普通参数传给 Service
- **统一返回 DTO**：不直接返回实体，避免泄露密码哈希等敏感字段

---

## 三、子系统划分

| 子系统 | 主要职责 |
|--------|----------|
| 用户与账户管理 | 注册（邮箱/手机号）、登录认证（JWT）、角色与权限、个人资料与头像、性别与个性签名、收货地址簿、账号状态（正常/禁言/限制发布/封禁）与信誉积分 |
| 商品与收藏管理 | 两级分类体系、商品发布/编辑/下架、浏览足迹、商品搜索（分词 + 词条图）、收藏与收藏夹管理、个性化推荐 |
| 订单交易与评价 | 下单、模拟支付、订单状态机（待付款→已付款→发货→收货→完成/取消）、订单时间线、订单评价与信誉统计 |
| 商品咨询与消息 | 商品留言与回复、买卖双方站内会话与消息（文字/图片、未读统计）、平台通知 |
| 平台治理与运营管理 | 用户治理（封禁/禁言/限制发布/警告）、商品审核与下架恢复、举报与申诉仲裁、平台公告、数据统计 |

---

## 四、项目结构

```
Campus-Secondhand-Trading-Platform/
├── Code/
│   ├── Backend/
│   │   ├── Backend.sln
│   │   └── Backend/
│   │       ├── Controllers/          # API 控制器（用户端 + 管理员端，共 27 个）
│   │       ├── Services/             # 业务逻辑层（接口 + 实现）
│   │       ├── Repositories/         # 仓储层（接口 + 实现）
│   │       ├── Dtos/                 # 按模块划分的请求/响应 DTO
│   │       ├── Models/               # 实体模型（含 Enums/）
│   │       ├── Data/                 # AppDbContext 与 EntityConfigs（Fluent API 配置）
│   │       ├── Migrations/           # EF Core 迁移文件
│   │       ├── Utilities/            # 搜索词条图、分词、分享码等工具
│   │       ├── Uploads/              # 上传文件（商品图、头像、聊天附件）
│   │       ├── Program.cs            # 应用入口：DI 注册、JWT、CORS、Swagger、启动初始化
│   │       └── appsettings.json      # 数据库连接、JWT、文件限制等配置
│   └── Frontend/
│       ├── src/
│       │   ├── api/                  # axios 实例 + 按模块划分的接口封装
│       │   ├── components/           # 通用组件（商品卡片、用户头像等）
│       │   ├── composables/          # 组合式函数（商品图片、头像加载等）
│       │   ├── layouts/              # 前台与后台布局
│       │   ├── router/               # 路由与登录守卫
│       │   ├── stores/               # Pinia 状态（登录态、消息等）
│       │   ├── types/api/            # 与后端对齐的类型定义
│       │   ├── utils/                # 令牌、错误处理、格式化等
│       │   └── views/                # 页面（auth / home / product / order / message / user / admin ...）
│       └── package.json
├── Code/后端开发流程.md              # 后端分层规范与新增功能 6 步流程
├── Code/API文档.md                   # 接口定义
└── README.md
```

---

## 五、数据库表（共 29 张）

| 分组 | 表 |
|------|----|
| 用户与账号 | `base_user`、`norm_user`、`admin_user`、`address`、`user_warning` |
| 商品域 | `category`、`product`、`prod_image`、`product_comment`、`product_view`、`product_audit_log`、`collection` |
| 交易域 | `purchase`、`payment`、`order_timeline`、`review`、`rev_image`、`refund`、`refund_review` |
| 通讯域 | `conversation`、`message` |
| 治理域 | `work_order`、`work_order_timeline`、`announcement` |
| 搜索域 | `search_term`、`search_term_edge`、`search_term_similarity` |
| 基础 | `files`（上传文件）、`sys_info` |

> 表结构变更统一通过 `dotnet ef migrations add <名称>` 生成迁移并 `dotnet ef database update` 应用，不要手工改库。

---

## 六、快速开始

### 后端（工作目录 `Code/Backend/Backend`）

```bash
dotnet restore                 # 安装依赖
dotnet ef database update      # 应用数据库迁移（首次或改表后）
dotnet build                   # 编译
dotnet run                     # 启动，默认 http://localhost:5141
```

- Swagger 文档：`http://localhost:5141/swagger`（右上角可填入 JWT 调试需登录接口）
- 无独立测试项目，验证方式为「编译通过 + Swagger 手测」

### 前端（工作目录 `Code/Frontend`）

```bash
npm install        # 安装依赖
npm run dev        # 启动开发服务器，默认 http://localhost:5173
npm run build      # 先做 vue-tsc 类型检查，再打包（类型错误会阻断构建）
```

- 接口基地址配置在 `.env` 的 `VITE_API_BASE_URL`（指向后端 5141）

---

## 七、配置说明

| 配置项 | 位置 | 说明 |
|--------|------|------|
| 数据库连接 | `appsettings.json` → `ConnectionStrings:DefaultConnection` | Oracle 连接串（FREEPDB1） |
| JWT 密钥 | user-secrets 优先，`appsettings.Development.json` 兜底 | 组员拉取代码后无需额外配置；生产环境须用环境变量/密钥服务 |
| 跨域 | `Program.cs` 的 CORS 策略 | 仅允许 `http://localhost:5173` 与 `http://localhost:3000` |
| 文件上传 | `appsettings.json` → `FileStorage:SizeLimits` | 商品图、头像、聊天附件均存于 `Uploads` |

JWT 自定义（可选）：

```bash
dotnet user-secrets set "Jwt:Key" "自定义密钥"
dotnet user-secrets set "Jwt:Issuer" "ecom-platform"
dotnet user-secrets set "Jwt:Audience" "ecom-client"
dotnet user-secrets set "Jwt:ExpireHours" "72"
```

---

## 八、主要功能模块

| 模块 | 关键能力 |
|------|----------|
| 认证与权限 | 邮箱/手机号登录、BCrypt 密码哈希、JWT 鉴权、管理员角色校验、修改密码、找回密码（验证码重置） |
| 用户资料 | 资料编辑（昵称/手机号/性别/个性签名）、头像上传与替换、收货地址簿（默认地址互斥与顶替）、他人主页 |
| 商品 | 发布/编辑/删除、图片管理、两级分类（商品挂小分类，支持大分类聚合查询）、浏览足迹、审核状态机与审计日志 |
| 搜索 | jieba 分词 + 词条共现图 + 相似词扩展、相关性排序、结果缓存与翻页、分类过滤 |
| 收藏 | 收藏/取消收藏（开关式）、收藏夹检索与批量取消、商品被收藏人数 |
| 推荐 | 基于浏览/收藏/成交的分类偏好推荐，热度兜底；详情页"猜你想看"结合当前商品与用户偏好 |
| 订单与支付 | 下单校验与商品锁定、订单状态机与时间线、模拟支付与回调、发货与确认收货 |
| 评价与信誉 | 完成订单后评价、被评价方回复、好评率与平均分统计 |
| 咨询与消息 | 商品留言与回复、站内会话与消息（文字/图片、未读统计） |
| 平台治理 | 管理员用户治理（封禁/禁言/限制发布/警告）、商品审核、举报与申诉仲裁（含处罚自动反转）、公告管理、数据统计 |

完整的接口口径以 `Code/API文档.md` 与运行后的 Swagger 为准。

---

## 九、开发约定

- 新增功能按 6 步流程：DTO → Repository → Service → Controller → `Program.cs` 注册 DI → 编译 + Swagger 验证
- 命名：`XxxController`、`IXxxService` + `XxxService`、`IXxxRepository` + `XxxRepository`
- 分支协作：个人分支按模块命名，PR 目标分支为 `main`（详见 `Code/后端开发流程.md`）

---
