# 校园二手交易平台 - API 文档

> 版本：v1.2 | 更新日期：2026-07-10  
> 基础地址：`http://localhost:5141`

---

## 目录

- [一、认证模块](#一认证模块)
  - [1. 注册](#1-注册)
  - [2. 登录](#2-登录)
  - [3. 退出登录](#3-退出登录)
  - [4. 获取当前用户信息](#4-获取当前用户信息)
  - [5. 修改密码](#5-修改密码)
  - [6. 发起重置密码](#6-发起重置密码)
  - [7. 确认重置密码](#7-确认重置密码)
  - [8. 权限校验](#8-权限校验)
- [二、用户模块](#二用户模块)
  - [9. 查询用户信息](#9-查询用户信息)
- [三、商品模块](#三商品模块)
  - [10. 查所有商品](#10-查所有商品)
  - [11. 查在售商品](#11-查在售商品)
  - [12. 查单个商品](#12-查单个商品)
  - [13. 发布商品](#13-发布商品)
  - [14. 修改商品](#14-修改商品)
  - [15. 删除商品](#15-删除商品)
- [四、认证说明](#四认证说明)
- [五、错误码说明](#五错误码说明)

---

## 一、认证模块

> 所有认证接口基础路径：`/api/auth`

### 1. 注册

新用户注册，自动创建用户并返回信息。

```
POST /api/auth/register
```

**请求体（JSON）：**

| 参数 | 类型 | 必填 | 最大长度 | 说明 |
|------|------|------|---------|------|
| email | string | ✅ | 50 | 邮箱，不可重复 |
| password | string | ✅ | — | 明文密码，后端自动 BCrypt 加密 |
| userName | string | ✅ | 20 | 用户昵称 |
| phoneNumber | string | ❌ | 11 | 手机号，不可重复 |

**请求示例：**

```json
{
  "email": "test@example.com",
  "password": "123456",
  "userName": "小明",
  "phoneNumber": "13800138000"
}
```

**成功响应：**

```http
HTTP/1.1 201 Created
```

```json
{
  "message": "注册成功",
  "user": {
    "userId": 1,
    "email": "test@example.com",
    "phoneNumber": "13800138000",
    "userName": "小明",
    "registerTime": "2026-07-10T12:00:00Z"
  }
}
```

> 邮箱或手机号已注册返回 `409 Conflict`

---

### 2. 登录

支持邮箱或手机号登录（二选一，优先邮箱）。

```
POST /api/auth/login
```

**请求体（JSON）：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| email | string? | ❌ | 注册邮箱（与手机号二选一） |
| phoneNumber | string? | ❌ | 手机号（与邮箱二选一） |
| password | string | ✅ | 密码 |

**请求示例：**

```json
// 邮箱登录
{
  "email": "test@example.com",
  "password": "123456"
}

// 手机号登录
{
  "phoneNumber": "13800138000",
  "password": "123456"
}
```

**成功响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": 1,
  "userType": 0,
  "userName": "小明"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| token | string | JWT Token，后续需登录的接口在 Header 中携带 |
| userId | int | 用户ID |
| userType | int | 0=普通用户，1=管理员 |
| userName | string | 昵称 |

> 账号或密码错误 / 账号被封禁 / 未提供登录标识 → `401 Unauthorized`

---

### 3. 退出登录

```
POST /api/auth/logout
```

> 🔒 需要登录：Header `Authorization: Bearer {token}`

**成功响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "message": "已退出登录"
}
```

> JWT 无状态，服务端不维护会话，客户端丢弃 Token 即完成登出。

---

### 4. 获取当前用户信息

获取已登录用户的完整身份、角色和基础信息。

```
GET /api/auth/me
```

> 🔒 需要登录

**成功响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "userId": 1,
  "email": "test@example.com",
  "phoneNumber": "13800138000",
  "userName": "小明",
  "userType": 0,
  "gender": "unknown",
  "avatarFileId": null,
  "isBanned": 0,
  "bannedUntil": null
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| userId | int | 用户ID |
| email | string | 邮箱 |
| phoneNumber | string? | 手机号 |
| userName | string | 昵称 |
| userType | int | 0=普通用户，1=管理员 |
| gender | string | 性别 |
| avatarFileId | long? | 头像文件ID（外键到UpdatedFile） |
| isBanned | int | 0=正常，1=已封禁 |
| bannedUntil | datetime? | 封禁截止时间 |

> 用户不存在返回 `404 Not Found`；未登录返回 `401`

---

### 5. 修改密码

已登录状态下修改当前账号密码。

```
PUT /api/auth/password
```

> 🔒 需要登录

**请求体（JSON）：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| oldPassword | string | ✅ | 原密码 |
| newPassword | string | ✅ | 新密码 |

**请求示例：**

```json
{
  "oldPassword": "123456",
  "newPassword": "newPassword123"
}
```

**成功响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "message": "密码修改成功"
}
```

> 原密码错误返回 `400 Bad Request`

---

### 6. 发起重置密码

忘记密码时，通过邮箱或手机号发起重置请求（验证码打印到控制台，实际应发邮件/短信）。

```
POST /api/auth/password/reset-request
```

**请求体（JSON）：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| email | string? | ❌ | 注册邮箱（与手机号二选一） |
| phoneNumber | string? | ❌ | 手机号（与邮箱二选一） |

**请求示例：**

```json
// 邮箱
{ "email": "test@example.com" }

// 手机号
{ "phoneNumber": "13800138000" }
```

**成功响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "message": "如果该账号已注册，您将收到重置验证码"
}
```

> ⚠️ 无论账号是否存在都返回 200，防止枚举攻击。验证码有效期 15 分钟，查看后端控制台输出。

---

### 7. 确认重置密码

输入账号标识、验证码和新密码，完成密码重置（须与第一步用相同方式）。

```
POST /api/auth/password/reset-confirm
```

**请求体（JSON）：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| email | string? | ❌ | 注册邮箱（与手机号二选一） |
| phoneNumber | string? | ❌ | 手机号（与邮箱二选一） |
| resetToken | string | ✅ | 6 位数字验证码 |
| newPassword | string | ✅ | 新密码 |

**请求示例：**

```json
{
  "email": "test@example.com",
  "resetToken": "123456",
  "newPassword": "newPassword456"
}
```

**成功响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "message": "密码重置成功，请重新登录"
}
```

> 验证码错误/过期返回 `400 Bad Request`

---

### 8. 权限校验

检查当前登录用户是否具备指定权限。

```
GET /api/auth/permission-check?permission={权限名}
```

> 🔒 需要登录

**查询参数：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| permission | string | ✅ | `admin` / `user` / `seller` |

**请求示例：**

```
GET /api/auth/permission-check?permission=admin
```

**成功响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "hasPermission": false,
  "permission": "admin"
}
```

---

## 二、用户模块

### 9. 查询用户信息

根据用户ID获取公开信息。

```
GET /api/users/{userId}
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| userId | int | 用户ID |

**请求示例：**

```
GET /api/users/1
```

**响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "userId": 1,
  "email": "test@example.com",
  "phoneNumber": "13800138000",
  "userName": "小明",
  "registerTime": "2026-07-10T12:00:00Z"
}
```

> 用户不存在返回 `404 Not Found`

---

## 三、商品模块

### 10. 查所有商品

```
GET /api/product
```

获取平台所有商品（含已售、已下架）。

**请求示例：**

```
GET /api/product
```

**响应：**

```http
HTTP/1.1 200 OK
```

```json
[
  {
    "productId": 1,
    "name": "二手 iPhone 15",
    "price": 2999.00,
    "info": "九成新，无维修",
    "status": "available",
    "releaseDate": "2026-07-06T22:34:17.149Z",
    "userId": 1,
    "categoryId": 1,
    "categoryName": "电子产品",
    "images": [
      {
        "imgId": 1,
        "imgUrl": "/uploads/xxx.jpg",
        "imgIndex": 1
      }
    ]
  }
]
```

| 字段 | 类型 | 说明 |
|------|------|------|
| productId | long | 商品ID |
| name | string | 商品名（≤30字） |
| price | decimal | 价格 |
| info | string? | 商品描述（≤100字） |
| status | string | available=在售 / sold=已售 / removed=已下架 |
| releaseDate | datetime | 发布时间 |
| userId | int | 卖家用户ID |
| categoryId | long | 分类ID |
| categoryName | string? | 分类名称 |
| images | array | 商品图片列表 |

---

### 11. 查在售商品

获取目前正在出售的商品。

```
GET /api/product/available
```

返回格式同 [查所有商品](#10-查所有商品)。

---

### 12. 查单个商品

```
GET /api/product/{productId}
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| productId | long | 商品ID |

**请求示例：**

```
GET /api/product/1
```

返回格式同上，单个对象。不存在返回 `404`。

---

### 13. 发布商品

```
POST /api/product
```

> ⚠️ 需要先注册用户，并确保分类 ID 存在。

**请求体（JSON）：**

| 参数 | 类型 | 必填 | 最大长度 | 说明 |
|------|------|------|---------|------|
| name | string | ✅ | 30 | 商品名称 |
| price | decimal | ✅ | — | 价格，如 2999.00 |
| info | string | ❌ | 100 | 商品描述 |
| userId | int | ✅ | — | 发布者用户ID |
| categoryId | long | ✅ | — | 分类ID |

**请求示例：**

```json
{
  "name": "二手 iPhone 15",
  "price": 2999.00,
  "info": "九成新，没有维修过",
  "userId": 1,
  "categoryId": 1
}
```

**响应：**

```http
HTTP/1.1 201 Created
Location: /api/product/1
```

```json
{
  "productId": 1,
  "name": "二手 iPhone 15",
  "price": 2999.00,
  "info": "九成新，没有维修过",
  "status": "available",
  "releaseDate": "2026-07-06T22:34:17.149Z",
  "userId": 1,
  "categoryId": 1,
  "categoryName": null,
  "images": []
}
```

---

### 14. 修改商品

```
PUT /api/product/{productId}
```

**路径参数：** `productId` — 商品ID

**请求体（JSON）— 所有字段均可选，只传要改的：**

| 参数 | 类型 | 说明 |
|------|------|------|
| name | string? | 新的商品名称 |
| price | decimal? | 新的价格 |
| info | string? | 新的描述 |
| status | string? | available / sold / removed |

**请求示例：**

```json
{
  "price": 2599.00,
  "info": "降价了，急出"
}
```

```json
{
  "status": "sold"
}
```

**响应：**

```http
HTTP/1.1 200 OK
```

返回更新后的商品对象。商品不存在返回 `404`。

---

### 15. 删除商品

```
DELETE /api/product/{productId}
```

**请求示例：**

```
DELETE /api/product/1
```

**响应：**

```http
HTTP/1.1 204 No Content
```

无返回体。商品不存在返回 `404`。

---

## 四、认证说明

所有需要登录的接口，需在请求头中携带 JWT Token：

```
Authorization: Bearer {token}
```

| 角色 | 说明 | 对应接口标记 |
|------|------|------------|
| 公开 | 无需登录即可访问 | 无标记 |
| 登录用户 | 需要携带有效 Token | 🔒 |
| 管理员 | 需要 Token + userType=1 | 🔒👑 |

Token 有效期 72 小时，过期后需重新登录。

---

## 五、错误码说明

| HTTP 状态码 | 含义 | 常见原因 |
|------------|------|---------|
| 200 | 成功 | — |
| 201 | 创建成功 | 注册/发布成功 |
| 204 | 删除成功 | 无返回内容 |
| 400 | 请求错误 | 参数格式不对、密码错误、验证码错误 |
| 401 | 未登录/认证失败 | Token 缺失、过期、邮箱密码错误 |
| 403 | 无权限 | 非管理员访问管理接口 |
| 404 | 不存在 | 用户/商品ID不存在 |
| 409 | 冲突 | 邮箱已注册 |

---

## 附录：测试流程

1. **注册用户** → `POST /api/auth/register`
2. **登录** → `POST /api/auth/login`，获取 `token`
3. **在 Swagger 右上角点 Authorize**，粘贴 `Bearer {token}`
4. **测试登录接口** → `GET /api/auth/me`、`PUT /api/auth/password` 等
5. **（手动）插入分类** → 数据库里跑 `INSERT INTO "category" ("category_name") VALUES ('电子产品');`
6. **发布商品** → `POST /api/product`（后续将改为从 Token 取 userId，无需手动传）
7. **查看商品** → `GET /api/product/1`
8. **查看列表** → `GET /api/product` 或 `GET /api/product/available`
