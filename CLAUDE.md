# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概览

校园二手交易平台，前后端分离：

- **后端** `Code/Backend/Backend`：.NET 8 + EF Core 8 + Oracle 21c+，三层架构（Controller → Service → Repository），JWT 认证，Swagger
- **前端** `Code/Frontend`：Vue 3 + TypeScript + Vite + Element Plus + Pinia + Vue Router

项目文档（均为中文，是权威参考）：
- `Code/后端开发流程.md` — 后端分层规范、新增功能 6 步流程、JWT 用法
- `Code/API文档.md` — 全部 API 接口定义
- `Code/Frontend/前端开发文档.md` — 前端目录与规范

## 常用命令

### 后端（工作目录 `Code/Backend/Backend`）

```bash
dotnet restore                      # 安装依赖
dotnet ef database update           # 应用迁移（新拉代码/改表后必跑）
dotnet build                        # 编译
dotnet run                          # 启动（http://localhost:5141）
dotnet ef migrations add <名称>      # 修改 Model 后生成迁移
```

- Swagger：`http://localhost:5141/swagger`（带 JWT Authorize 按钮）
- 运行后端口在 `Properties/launchSettings.json`（当前 5141）
- 无测试项目，验证方式 = 编译 + Swagger 手测
- 数据库连接在 `appsettings.json` 的 `ConnectionStrings:DefaultConnection`（Oracle FREEPDB1）；JWT 密钥优先级 user-secrets > appsettings.Development.json
- 仓库根目录 `seed_data.sql` 为种子数据

### 前端（工作目录 `Code/Frontend`）

```bash
npm install         # 安装依赖
npm run dev         # Vite 开发服务器（localhost:5173）
npm run build       # vue-tsc 类型检查 + 构建（类型错误会阻断构建）
```

- API 基础地址在 `.env` 的 `VITE_API_BASE_URL`（指向后端 5141）
- 后端 CORS 只允许 `localhost:5173` 和 `localhost:3000`

## 后端架构与分层规则

请求链：`HTTP → JWT中间件 → Controller → Service → Repository → AppDbContext → Oracle`

各层职责有硬性约束（详见 `Code/后端开发流程.md`）：

| 层 | 可以 | 不可以 |
|----|------|--------|
| Controller | 收发 HTTP，调 Service 接口 | 写业务判断，直接调 Repository |
| Service | 业务规则（所有 if/else/throw），调 Repository 接口 | 接触 HttpContext / DbContext / JWT |
| Repository | DbContext CRUD，`SaveAsync()` | 业务规则，调其他 Repository |

- **JWT 只在 Controller 层感知**：Controller 里 `int.Parse(User.FindFirst("userId")!.Value)` 取用户 ID，以普通 `int userId` 参数传给 Service。权限用 `[Authorize]`（登录）和 `[Authorize(Roles = "Admin")]`（管理员）控制
- **DTO 规范**：`XxxDto`（响应）、`CreateXxxDto`（创建请求）、`UpdateXxxDto`（更新请求，字段全可选）。禁止直接返回 Model（会泄露密码 hash 等）或匿名对象
- **命名**：`XxxController.cs` / `IXxxService.cs` + `XxxService.cs` / `IXxxRepository.cs` + `XxxRepository.cs`
- Repository 用表达式体风格（`public async Task<X?> GetByIdAsync(int id) => await _context.Xs.FindAsync(id);`），与现有代码保持一致

### 新增功能流程（6 步）

1. `Dtos/<模块>/` 建 DTO
2. `Repositories/` 建 `IXxxRepository.cs` + `XxxRepository.cs`
3. `Services/` 建 `IXxxService.cs` + `XxxService.cs`
4. `Controllers/` 建 `XxxController.cs`
5. **`Program.cs` 注册两个 DI**（`AddScoped`，漏注册会启动报错 `Unable to resolve service`）
6. `dotnet build` + Swagger 验证

### 关键子系统

- **用户模型**：`BaseUser`（公共信息）+ `NormUser`（普通用户扩展）+ `AdminUser`（管理员扩展）三表；userType：0=普通用户，1=管理员
- **实体配置**：`Data/EntityConfigs/` 中 Fluent API（表名、列名映射、索引），Oracle 用 `UseIdentityColumn()`
- **搜索子系统**：jieba.NET 分词 + `Utilities/TermGraph` 词条图；启动时（`Program.cs` 末尾）从数据库加载，为空则全量重建
- **文件上传**：`FileStorageService` 存到 `./Uploads`，大小限制在 `appsettings.json` 的 `FileStorage:SizeLimits`
- `Migrations/` 中每个迁移都有 Designer 快照，改表必须走 `dotnet ef migrations add`，不要手改快照

## 前端架构

- `src/api/http.ts`：axios 实例，请求拦截器自动附 `Authorization: Bearer <token>`，401 时清 token 并触发 `notifyUnauthorized()`
- `src/api/modules/<业务>.ts`：每个后端模块一个 API 文件（auth、product、user、category、collection、comment、conversation、notification、home、address）
- `src/stores/auth.ts`：Pinia 状态（token + currentUser），`isAdmin` = `userType === 1`
- `src/router/index.ts`：路由守卫 `meta.requiresAuth`，未登录重定向 `/login?redirect=...`
- 目录按业务模块划分：`src/views/{auth,home,product,user,seller,message}/`，布局在 `src/layouts/`
- 类型定义集中在 `src/types/api/`

## Git 协作

- PR 目标分支为 `main`；个人分支按模块命名（如 `backend_workorder`、`frontend_13-15`）
- 提交信息用中文或 `feat:`/`fix:` 前缀均可，与现有历史一致
