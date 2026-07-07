# Campus-Secondhand-Trading-Platform

校园二手交易平台 — 基于 .NET 8.0 + Oracle 数据库开发的前后端分离项目

## 技术栈

| 层级 | 技术 | 版本 |
|------|------|------|
| 后端框架 | ASP.NET Core | 8.0 |
| 数据库 | Oracle | 21c+ |
| ORM | Entity Framework Core | 8.0 |
| API文档 | Swagger | 6.6 |
| 密码加密 | BCrypt | 4.2 |

## 项目结构

```
Campus-Secondhand-Trading-Platform/
├── .vscode/                    # VS Code 配置文件
│   ├── launch.json             # 调试配置
│   └── tasks.json              # 任务配置
├── Code/                       # 代码目录
│   ├── Backend/                # 后端代码
│   │   ├── Backend/            # 主项目目录
│   │   │   ├── Controllers/    # API 控制器层
│   │   │   │   ├── ProductController.cs  # 商品接口
│   │   │   │   └── UserController.cs     # 用户接口
│   │   │   ├── Data/           # 数据层
│   │   │   │   ├── EntityConfigs/  # EF Core 实体配置
│   │   │   │   └── AppDbContext.cs   # 数据库上下文
│   │   │   ├── Dtos/           # 数据传输对象
│   │   │   │   ├── Product/    # 商品 DTO
│   │   │   │   └── User/       # 用户 DTO
│   │   │   ├── Migrations/     # 数据库迁移文件
│   │   │   ├── Models/         # 数据模型
│   │   │   │   ├── Enums/      # 枚举类型定义
│   │   │   │   └── *.cs        # 实体类
│   │   │   ├── Properties/     # 项目属性
│   │   │   │   └── launchSettings.json
│   │   │   ├── Repositories/   # 仓储层（接口+实现）
│   │   │   ├── Services/       # 业务逻辑层（接口+实现）
│   │   │   ├── Backend.csproj  # 项目文件
│   │   │   ├── Backend.http    # HTTP 接口测试文件
│   │   │   ├── Program.cs      # 应用入口
│   │   │   └── appsettings.json # 配置文件
│   │   └── Backend.sln         # 解决方案文件
│   └── Frontend/               # 前端代码（待开发）
├── .gitignore                  # Git 忽略规则
└── README.md                   # 项目说明文档
```

## 目录说明

### Backend/Backend/

| 目录/文件 | 说明 |
|-----------|------|
| **Controllers/** | REST API 控制器，处理 HTTP 请求和响应 |
| **Data/** | 数据访问层，包含数据库上下文和实体配置 |
| **Data/EntityConfigs/** | EF Core Fluent API 配置，定义表名、列名、索引等 |
| **Dtos/** | 数据传输对象，用于 API 请求和响应的数据结构 |
| **Migrations/** | EF Core 数据库迁移文件，记录数据库结构变更 |
| **Models/** | 实体模型，映射数据库表结构 |
| **Models/Enums/** | 枚举类型定义（用户类型、订单状态、商品状态等） |
| **Repositories/** | 仓储层，封装数据访问逻辑（接口 + 实现类） |
| **Services/** | 业务逻辑层，处理核心业务逻辑（接口 + 实现类） |
| **Program.cs** | 应用程序入口，配置服务依赖注入和中间件 |
| **appsettings.json** | 应用配置文件，包含数据库连接字符串、JWT 配置等 |
| **Backend.http** | Visual Studio Code REST Client 测试文件 |

## 数据库表结构

| 表名 | 说明 |
|------|------|
| `base_user` | 用户基础表（公共信息：邮箱、密码、手机号等） |
| `norm_user` | 普通用户扩展表 |
| `admin_user` | 管理员扩展表 |
| `address` | 收货地址表 |
| `category` | 商品分类表 |
| `product` | 商品表 |
| `prod_image` | 商品图片表 |
| `collection` | 收藏表 |
| `purchase` | 购买订单表 |
| `review` | 商品评价表 |
| `rev_image` | 评价图片表 |
| `refund` | 退款申请表 |
| `refund_review` | 退款审核表 |
| `conversation` | 会话表 |
| `message` | 消息表 |
| `work_order` | 工单表 |
| `announcement` | 公告表 |
| `sys_info` | 系统信息表 |

## 启动方式

### 开发环境

```bash
# 进入后端项目目录
cd Code/Backend/Backend

# 安装依赖
dotnet restore

# 运行数据库迁移（首次运行）
dotnet ef database update

# 启动服务
dotnet run
```

服务启动后访问：
- API 文档：`https://localhost:5001/swagger`
- 接口地址：`https://localhost:5001/api/`

### 配置说明

在 [appsettings.json](file:///d:/数据库课设/Campus-Secondhand-Trading-Platform/Code/Backend/Backend/appsettings.json) 中配置：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=your-oracle-host:1521/FREEPDB1;User Id=username;Password=password;"
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-min-32-chars!",
    "Issuer": "CampusSecondhand",
    "Audience": "CampusSecondhandUsers",
    "ExpireMinutes": 1440
  }
}
```

## 现有 API 接口

### 用户接口 (`/api/User`)

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/User/register` | 用户注册 |
| GET | `/api/User/{id}` | 根据 ID 获取用户信息 |

### 商品接口 (`/api/Product`)

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/Product` | 获取所有商品 |
| GET | `/api/Product/available` | 获取在售商品 |
| GET | `/api/Product/{id}` | 根据 ID 获取商品详情 |
| POST | `/api/Product` | 发布商品 |
| PUT | `/api/Product/{id}` | 更新商品信息 |
| DELETE | `/api/Product/{id}` | 删除商品 |

## 设计模式

- **三层架构**: Controller → Service → Repository
- **依赖注入**: ASP.NET Core 内置 DI 容器
- **仓储模式**: 封装数据访问逻辑
- **DTO 模式**: 分离数据模型和传输对象

## 项目状态

- ✅ 后端框架搭建完成
- ✅ 数据库模型设计完成
- ✅ 基础 CRUD 接口实现（用户、商品）
- ⏳ 前端开发待启动
